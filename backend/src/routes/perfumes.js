const express = require('express');
const prisma = require('../prisma/client');
const authMiddleware = require('../middleware/auth');

const router = express.Router();

// Mevsim → accord eşlemesi
const SEASON_ACCORDS = {
  ilkbahar: ['floral', 'white floral', 'green', 'fresh', 'fruity', 'rose'],
  yaz:      ['citrus', 'fresh', 'aquatic', 'light fresh', 'fruity', 'green'],
  sonbahar: ['woody', 'warm spicy', 'amber', 'leather', 'earthy', 'patchouli'],
  kis:      ['vanilla', 'sweet', 'oud', 'warm spicy', 'amber', 'powdery', 'musky'],
};

// Designer vs Niche: basit marka listesi
const DESIGNER_BRANDS = new Set([
  'dior','chanel','givenchy','yves-saint-laurent','giorgio-armani','calvin-klein',
  'gucci','prada','versace','burberry','hugo-boss','dolce-gabbana','hermes',
  'lancome','nina-ricci','balenciaga','valentino','ralph-lauren','carolina-herrera',
  'kenzo','bvlgari','mont-blanc','lacoste','emporio-armani','givenchy','cacharel',
]);

// GET /api/perfumes/meta  →  keşfet sayfası için kategori verileri
router.get('/meta', async (req, res) => {
  const [topBrands, topAccords, countries] = await Promise.all([
    prisma.perfume.groupBy({ by: ['brand'], _count: { id: true }, orderBy: { _count: { id: 'desc' } }, take: 16 }),
    (async () => {
      const map = {};
      for (const field of ['accord1','accord2','accord3','accord4','accord5']) {
        const rows = await prisma.perfume.groupBy({ by: [field], _count: { id: true }, where: { [field]: { not: null } }, orderBy: { _count: { id: 'desc' } }, take: 30 });
        rows.forEach(r => { map[r[field]] = (map[r[field]] || 0) + r._count.id; });
      }
      return Object.entries(map).sort((a,b) => b[1]-a[1]).slice(0, 20).map(([accord, count]) => ({ accord, count }));
    })(),
    prisma.perfume.groupBy({ by: ['country'], _count: { id: true }, where: { country: { not: null } }, orderBy: { _count: { id: 'desc' } }, take: 10 }),
  ]);

  res.json({
    topBrands: topBrands.map(b => ({ brand: b.brand, count: b._count.id })),
    topAccords,
    countries: countries.map(c => ({ country: c.country, count: c._count.id })),
    seasons: Object.keys(SEASON_ACCORDS),
  });
});

// GET /api/perfumes?search=&brand=&gender=&accord=&season=&country=&tier=&yearFrom=&yearTo=&sort=&page=&limit=
router.get('/', async (req, res) => {
  const {
    search, brand, gender, accord, season, country, tier,
    yearFrom, yearTo, sort = 'popular',
    page = 1, limit = 20
  } = req.query;
  const skip = (Number(page) - 1) * Number(limit);

  const AND = [];

  if (search) {
    AND.push({
      OR: [
        { name: { contains: search } },
        { brand: { contains: search } },
        { topNotes: { contains: search } },
        { middleNotes: { contains: search } },
        { baseNotes: { contains: search } },
      ],
    });
  }

  if (brand) AND.push({ brand: { contains: brand } });
  if (gender) AND.push({ gender });
  if (country) AND.push({ country: { contains: country } });
  if (yearFrom) AND.push({ year: { gte: Number(yearFrom) } });
  if (yearTo)   AND.push({ year: { lte: Number(yearTo) } });

  if (accord) {
    AND.push({
      OR: ['accord1','accord2','accord3','accord4','accord5'].map(f => ({
        [f]: { contains: accord },
      })),
    });
  }

  if (season && SEASON_ACCORDS[season]) {
    const seasonAccords = SEASON_ACCORDS[season];
    AND.push({
      OR: seasonAccords.flatMap(a =>
        ['accord1','accord2','accord3','accord4','accord5'].map(f => ({ [f]: a }))
      ),
    });
  }

  if (tier === 'designer') {
    AND.push({ brand: { in: [...DESIGNER_BRANDS] } });
  } else if (tier === 'niche') {
    AND.push({ brand: { notIn: [...DESIGNER_BRANDS] } });
  }

  const where = AND.length > 0 ? { AND } : {};

  const orderBy = sort === 'rating'  ? { ratingValue: 'desc' }
                : sort === 'newest'  ? { year: 'desc' }
                : sort === 'oldest'  ? { year: 'asc' }
                :                     { ratingCount: 'desc' }; // popular

  const [perfumes, total] = await Promise.all([
    prisma.perfume.findMany({ where, skip, take: Number(limit), orderBy }),
    prisma.perfume.count({ where }),
  ]);

  res.json({ perfumes, total, page: Number(page), totalPages: Math.ceil(total / Number(limit)) });
});

// GET /api/perfumes/:id
router.get('/:id', async (req, res) => {
  const perfume = await prisma.perfume.findUnique({
    where: { id: Number(req.params.id) },
    include: {
      reviews: {
        include: { user: { select: { id: true, username: true, avatar: true } } },
        orderBy: { createdAt: 'desc' },
        take: 10,
      },
      _count: { select: { likes: true, collections: true } },
    },
  });

  if (!perfume) return res.status(404).json({ error: 'Parfüm bulunamadı' });
  res.json(perfume);
});

// POST /api/perfumes (auth required)
router.post('/', authMiddleware, async (req, res) => {
  const { name, brand, country, gender, year, topNotes, middleNotes, baseNotes, accord1, accord2, accord3, accord4, accord5 } = req.body;

  if (!name || !brand) {
    return res.status(400).json({ error: 'İsim ve marka zorunlu' });
  }

  const perfume = await prisma.perfume.create({
    data: { name, brand, country, gender, year: year ? Number(year) : null, topNotes, middleNotes, baseNotes, accord1, accord2, accord3, accord4, accord5 },
  });

  res.status(201).json(perfume);
});

module.exports = router;
