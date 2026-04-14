const express = require('express');
const prisma = require('../prisma/client');
const auth = require('../middleware/auth');

const router = express.Router();

// GET /api/communities  →  tüm topluluklar (üyelik durumu ile)
router.get('/', auth, async (req, res) => {
  const communities = await prisma.community.findMany({
    include: {
      owner: { select: { id: true, username: true, avatar: true } },
      _count: { select: { members: true } },
    },
    orderBy: { createdAt: 'desc' },
  });

  const myMemberships = await prisma.communityMember.findMany({
    where: { userId: req.userId },
    select: { communityId: true },
  });
  const joinedIds = new Set(myMemberships.map(m => m.communityId));

  res.json(communities.map(c => ({ ...c, isMember: joinedIds.has(c.id) })));
});

// GET /api/communities/:id
router.get('/:id', auth, async (req, res) => {
  const id = parseInt(req.params.id);
  const community = await prisma.community.findUnique({
    where: { id },
    include: {
      owner: { select: { id: true, username: true, avatar: true } },
      members: {
        include: { user: { select: { id: true, username: true, avatar: true } } },
        take: 20,
      },
      _count: { select: { members: true } },
    },
  });

  if (!community) return res.status(404).json({ error: 'Topluluk bulunamadı' });

  const membership = await prisma.communityMember.findUnique({
    where: { userId_communityId: { userId: req.userId, communityId: id } },
  });

  res.json({ ...community, isMember: !!membership, myRole: membership?.role ?? null });
});

// POST /api/communities  →  topluluk oluştur
router.post('/', auth, async (req, res) => {
  const { name, description, imageUrl } = req.body;
  if (!name?.trim()) return res.status(400).json({ error: 'İsim zorunlu' });

  const existing = await prisma.community.findUnique({ where: { name: name.trim() } });
  if (existing) return res.status(409).json({ error: 'Bu isim zaten alınmış' });

  const community = await prisma.community.create({
    data: {
      name: name.trim(),
      description: description?.trim() || null,
      imageUrl: imageUrl || null,
      ownerId: req.userId,
      members: { create: { userId: req.userId, role: 'admin' } },
    },
    include: { _count: { select: { members: true } } },
  });

  res.status(201).json({ ...community, isMember: true, myRole: 'admin' });
});

// POST /api/communities/:id/join  (toggle)
router.post('/:id/join', auth, async (req, res) => {
  const communityId = parseInt(req.params.id);

  const community = await prisma.community.findUnique({ where: { id: communityId } });
  if (!community) return res.status(404).json({ error: 'Topluluk bulunamadı' });

  const existing = await prisma.communityMember.findUnique({
    where: { userId_communityId: { userId: req.userId, communityId } },
  });

  if (existing) {
    if (community.ownerId === req.userId) return res.status(400).json({ error: 'Kurucu ayrılamaz' });
    await prisma.communityMember.delete({ where: { id: existing.id } });
    return res.json({ joined: false });
  }

  await prisma.communityMember.create({ data: { userId: req.userId, communityId } });
  res.json({ joined: true });
});

// GET /api/communities/:id/posts
router.get('/:id/posts', auth, async (req, res) => {
  const communityId = parseInt(req.params.id);
  const page = parseInt(req.query.page) || 1;
  const limit = 20;
  const skip = (page - 1) * limit;

  const [posts, total] = await Promise.all([
    prisma.communityPost.findMany({
      where: { communityId },
      orderBy: { createdAt: 'desc' },
      skip,
      take: limit,
      include: {
        user: { select: { id: true, username: true, avatar: true } },
        perfume: { select: { id: true, name: true, brand: true, imageUrl: true, ratingValue: true, accord1: true } },
      },
    }),
    prisma.communityPost.count({ where: { communityId } }),
  ]);

  res.json({ posts, total });
});

// POST /api/communities/:id/posts
router.post('/:id/posts', auth, async (req, res) => {
  const communityId = parseInt(req.params.id);
  const { body, perfumeId } = req.body;
  if (!body?.trim()) return res.status(400).json({ error: 'İçerik boş olamaz' });

  // Üye mi?
  const membership = await prisma.communityMember.findUnique({
    where: { userId_communityId: { userId: req.userId, communityId } },
  });
  if (!membership) return res.status(403).json({ error: 'Önce topluluğa katılmalısın' });

  const post = await prisma.communityPost.create({
    data: {
      communityId,
      userId: req.userId,
      body: body.trim(),
      perfumeId: perfumeId ? Number(perfumeId) : null,
    },
    include: {
      user: { select: { id: true, username: true, avatar: true } },
      perfume: { select: { id: true, name: true, brand: true, imageUrl: true, ratingValue: true, accord1: true } },
    },
  });

  res.status(201).json(post);
});

// DELETE /api/communities/:id/posts/:postId
router.delete('/:id/posts/:postId', auth, async (req, res) => {
  const postId = parseInt(req.params.postId);
  const post = await prisma.communityPost.findUnique({ where: { id: postId } });
  if (!post) return res.status(404).json({ error: 'Post bulunamadı' });
  if (post.userId !== req.userId) return res.status(403).json({ error: 'Yetkisiz' });

  await prisma.communityPost.delete({ where: { id: postId } });
  res.status(204).send();
});

module.exports = router;
