const express = require('express');
const prisma = require('../prisma/client');
const authMiddleware = require('../middleware/auth');
const { createNotification } = require('./notifications');

const router = express.Router();

// POST /api/reviews
router.post('/', authMiddleware, async (req, res) => {
  const { perfumeId, rating, body } = req.body;

  if (!perfumeId || !rating) {
    return res.status(400).json({ error: 'perfumeId ve rating zorunlu' });
  }
  if (rating < 1 || rating > 5) {
    return res.status(400).json({ error: 'rating 1-5 arasında olmalı' });
  }

  const existing = await prisma.review.findUnique({
    where: { userId_perfumeId: { userId: req.userId, perfumeId: Number(perfumeId) } },
  });
  if (existing) {
    return res.status(409).json({ error: 'Bu parfüme zaten yorum yaptınız' });
  }

  const review = await prisma.review.create({
    data: {
      userId: req.userId,
      perfumeId: Number(perfumeId),
      rating: Number(rating),
      body: body || null,
    },
    include: { user: { select: { id: true, username: true } } },
  });

  // Parfümü koleksiyonunda olan kullanıcılara (owned) bildirim gönder
  const owners = await prisma.collection.findMany({
    where: { perfumeId: Number(perfumeId), status: 'owned', userId: { not: req.userId } },
    select: { userId: true },
    take: 10,
  });
  owners.forEach(({ userId }) => {
    createNotification({ recipientId: userId, actorId: req.userId, type: 'review', perfumeId: Number(perfumeId) }).catch(() => {});
  });

  res.status(201).json(review);
});

// DELETE /api/reviews/:id
router.delete('/:id', authMiddleware, async (req, res) => {
  const review = await prisma.review.findUnique({ where: { id: Number(req.params.id) } });
  if (!review) return res.status(404).json({ error: 'Yorum bulunamadı' });
  if (review.userId !== req.userId) return res.status(403).json({ error: 'Yetkisiz' });

  await prisma.review.delete({ where: { id: Number(req.params.id) } });
  res.status(204).send();
});

module.exports = router;
