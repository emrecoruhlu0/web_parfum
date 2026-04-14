const express = require('express');
const prisma = require('../prisma/client');
const authMiddleware = require('../middleware/auth');

const router = express.Router();

// GET /api/dailylog?month=2025-04  →  o aya ait tüm loglar
router.get('/', authMiddleware, async (req, res) => {
  const { month } = req.query; // "YYYY-MM"
  const where = { userId: req.userId };

  if (month) {
    const [year, mon] = month.split('-').map(Number);
    const start = new Date(year, mon - 1, 1);
    const end   = new Date(year, mon, 1);
    where.date = { gte: start, lt: end };
  }

  const logs = await prisma.dailyLog.findMany({
    where,
    include: { perfume: { select: { id: true, name: true, brand: true, imageUrl: true, topNotes: true, middleNotes: true, baseNotes: true } } },
    orderBy: { date: 'desc' },
  });

  res.json(logs);
});

// POST /api/dailylog
router.post('/', authMiddleware, async (req, res) => {
  const { perfumeId, date, note, sprays } = req.body;

  if (!perfumeId || !date) {
    return res.status(400).json({ error: 'perfumeId ve date zorunlu' });
  }

  const log = await prisma.dailyLog.create({
    data: {
      userId: req.userId,
      perfumeId: Number(perfumeId),
      date: new Date(date),
      note: note || null,
      sprays: sprays ? Number(sprays) : null,
    },
    include: { perfume: { select: { id: true, name: true, brand: true, imageUrl: true } } },
  });

  res.status(201).json(log);
});

// DELETE /api/dailylog/:id
router.delete('/:id', authMiddleware, async (req, res) => {
  const log = await prisma.dailyLog.findUnique({ where: { id: Number(req.params.id) } });
  if (!log) return res.status(404).json({ error: 'Log bulunamadı' });
  if (log.userId !== req.userId) return res.status(403).json({ error: 'Yetkisiz' });

  await prisma.dailyLog.delete({ where: { id: Number(req.params.id) } });
  res.status(204).send();
});

// GET /api/dailylog/profile  →  nota frekans analizi (koku profili)
router.get('/profile', authMiddleware, async (req, res) => {
  const logs = await prisma.dailyLog.findMany({
    where: { userId: req.userId },
    include: { perfume: { select: { topNotes: true, middleNotes: true, baseNotes: true, accord1: true, accord2: true, accord3: true } } },
  });

  const noteFreq = {};
  const accordFreq = {};

  for (const log of logs) {
    const p = log.perfume;
    const allNotes = [p.topNotes, p.middleNotes, p.baseNotes]
      .filter(Boolean)
      .flatMap(n => n.split(',').map(s => s.trim()).filter(Boolean));

    for (const note of allNotes) {
      noteFreq[note] = (noteFreq[note] || 0) + 1;
    }

    for (const accord of [p.accord1, p.accord2, p.accord3].filter(Boolean)) {
      accordFreq[accord] = (accordFreq[accord] || 0) + 1;
    }
  }

  const topNotes = Object.entries(noteFreq)
    .sort((a, b) => b[1] - a[1])
    .slice(0, 20)
    .map(([note, count]) => ({ note, count }));

  const topAccords = Object.entries(accordFreq)
    .sort((a, b) => b[1] - a[1])
    .slice(0, 10)
    .map(([accord, count]) => ({ accord, count }));

  res.json({ topNotes, topAccords, totalLogs: logs.length });
});

module.exports = router;
