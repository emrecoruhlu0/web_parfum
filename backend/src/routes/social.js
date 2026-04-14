const express = require('express');
const prisma = require('../prisma/client');
const authMiddleware = require('../middleware/auth');
const { createNotification } = require('./notifications');

const router = express.Router();

// ─── LIKE ────────────────────────────────────────────────────────────────────

// POST /api/social/like/:perfumeId
router.post('/like/:perfumeId', authMiddleware, async (req, res) => {
  const perfumeId = Number(req.params.perfumeId);

  const existing = await prisma.like.findUnique({
    where: { userId_perfumeId: { userId: req.userId, perfumeId } },
  });

  if (existing) {
    await prisma.like.delete({ where: { id: existing.id } });
    const count = await prisma.like.count({ where: { perfumeId } });
    return res.json({ liked: false, count });
  }

  await prisma.like.create({ data: { userId: req.userId, perfumeId } });
  const count = await prisma.like.count({ where: { perfumeId } });

  // Parfümü koleksiyonunda "owned" statüsünde ekleyen kullanıcıya bildirim gönder
  // (parfümün "sahibi" yoksa atla)
  res.json({ liked: true, count });
});

// GET /api/social/likes?perfumeIds=1,2,3  →  hangileri beğenilmiş
router.get('/likes', authMiddleware, async (req, res) => {
  const ids = (req.query.perfumeIds || '').split(',').map(Number).filter(Boolean);
  if (ids.length === 0) return res.json([]);

  const likes = await prisma.like.findMany({
    where: { userId: req.userId, perfumeId: { in: ids } },
    select: { perfumeId: true },
  });

  res.json(likes.map(l => l.perfumeId));
});

// ─── FOLLOW ──────────────────────────────────────────────────────────────────

// POST /api/social/follow/:userId  (toggle)
router.post('/follow/:targetId', authMiddleware, async (req, res) => {
  const followingId = Number(req.params.targetId);
  if (followingId === req.userId) {
    return res.status(400).json({ error: 'Kendini takip edemezsin' });
  }

  const existing = await prisma.follow.findUnique({
    where: { followerId_followingId: { followerId: req.userId, followingId } },
  });

  if (existing) {
    await prisma.follow.delete({ where: { id: existing.id } });
    return res.json({ following: false });
  }

  await prisma.follow.create({ data: { followerId: req.userId, followingId } });

  // Takip edilen kişiye bildirim gönder
  createNotification({ recipientId: followingId, actorId: req.userId, type: 'follow' }).catch(() => {});

  res.json({ following: true });
});

// ─── USER PROFİL ─────────────────────────────────────────────────────────────

// GET /api/social/users/:username
router.get('/users/:username', authMiddleware, async (req, res) => {
  const user = await prisma.user.findUnique({
    where: { username: req.params.username },
    select: {
      id: true, username: true, bio: true, avatar: true, createdAt: true,
      _count: {
        select: {
          following: true,   // kaç kişiyi takip ediyor
          followers: true,   // kaç takipçisi var
          collections: true,
          reviews: true,
        },
      },
    },
  });

  if (!user) return res.status(404).json({ error: 'Kullanıcı bulunamadı' });

  // mevcut kullanıcı takip ediyor mu?
  const isFollowing = await prisma.follow.findUnique({
    where: { followerId_followingId: { followerId: req.userId, followingId: user.id } },
  });

  res.json({ ...user, isFollowing: !!isFollowing });
});

// GET /api/social/users/:username/collection
router.get('/users/:username/collection', authMiddleware, async (req, res) => {
  const user = await prisma.user.findUnique({ where: { username: req.params.username } });
  if (!user) return res.status(404).json({ error: 'Kullanıcı bulunamadı' });

  const { status } = req.query;
  const where = { userId: user.id };
  if (status) where.status = status;

  const items = await prisma.collection.findMany({
    where,
    include: { perfume: true },
    orderBy: { createdAt: 'desc' },
    take: 20,
  });

  res.json(items);
});

// ─── AKTİVİTE AKIŞI ──────────────────────────────────────────────────────────

// GET /api/social/feed  →  takip edilenlerin son aktiviteleri
router.get('/feed', authMiddleware, async (req, res) => {
  // Takip edilenlerin id listesi
  const follows = await prisma.follow.findMany({
    where: { followerId: req.userId },
    select: { followingId: true },
  });
  const followingIds = follows.map(f => f.followingId);

  // Kendi aktivitelerini de dahil et
  const userIds = [req.userId, ...followingIds];

  const [reviews, logs] = await Promise.all([
    prisma.review.findMany({
      where: { userId: { in: userIds } },
      include: {
        user: { select: { id: true, username: true, avatar: true } },
        perfume: { select: { id: true, name: true, brand: true, imageUrl: true } },
      },
      orderBy: { createdAt: 'desc' },
      take: 20,
    }),
    prisma.dailyLog.findMany({
      where: { userId: { in: userIds } },
      include: {
        user: { select: { id: true, username: true, avatar: true } },
        perfume: { select: { id: true, name: true, brand: true, imageUrl: true } },
      },
      orderBy: { date: 'desc' },
      take: 20,
    }),
  ]);

  // Birleştir ve tarihe göre sırala
  const feed = [
    ...reviews.map(r => ({ type: 'review', date: r.createdAt, data: r })),
    ...logs.map(l => ({ type: 'log', date: l.date, data: l })),
  ].sort((a, b) => new Date(b.date) - new Date(a.date)).slice(0, 30);

  res.json(feed);
});

module.exports = router;
