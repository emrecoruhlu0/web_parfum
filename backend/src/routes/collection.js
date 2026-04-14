const express = require('express');
const prisma = require('../prisma/client');
const authMiddleware = require('../middleware/auth');

const router = express.Router();

// GET /api/collection?status=owned|wishlist|tried
router.get('/', authMiddleware, async (req, res) => {
  const { status } = req.query;
  const where = { userId: req.userId };
  if (status) where.status = status;

  const items = await prisma.collection.findMany({
    where,
    include: { perfume: true },
    orderBy: { createdAt: 'desc' },
  });

  res.json(items);
});

// POST /api/collection
router.post('/', authMiddleware, async (req, res) => {
  const { perfumeId, status, bottleLevel } = req.body;

  if (!perfumeId || !status) {
    return res.status(400).json({ error: 'perfumeId ve status zorunlu' });
  }
  if (!['owned', 'wishlist', 'tried'].includes(status)) {
    return res.status(400).json({ error: 'status: owned | wishlist | tried olmalı' });
  }

  const item = await prisma.collection.upsert({
    where: { userId_perfumeId: { userId: req.userId, perfumeId: Number(perfumeId) } },
    update: { status, bottleLevel: bottleLevel != null ? Number(bottleLevel) : undefined },
    create: { userId: req.userId, perfumeId: Number(perfumeId), status, bottleLevel: bottleLevel != null ? Number(bottleLevel) : null },
    include: { perfume: true },
  });

  res.status(201).json(item);
});

// PATCH /api/collection/:perfumeId
router.patch('/:perfumeId', authMiddleware, async (req, res) => {
  const { status, bottleLevel } = req.body;

  const item = await prisma.collection.update({
    where: { userId_perfumeId: { userId: req.userId, perfumeId: Number(req.params.perfumeId) } },
    data: {
      ...(status && { status }),
      ...(bottleLevel != null && { bottleLevel: Number(bottleLevel) }),
    },
    include: { perfume: true },
  });

  res.json(item);
});

// DELETE /api/collection/:perfumeId
router.delete('/:perfumeId', authMiddleware, async (req, res) => {
  await prisma.collection.delete({
    where: { userId_perfumeId: { userId: req.userId, perfumeId: Number(req.params.perfumeId) } },
  });

  res.status(204).send();
});

module.exports = router;
