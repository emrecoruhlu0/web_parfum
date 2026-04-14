const express = require('express');
const router = express.Router();
const prisma = require('../prisma/client');
const auth = require('../middleware/auth');

// Bildirim oluşturma yardımcısı (diğer route'lar tarafından çağrılır)
async function createNotification({ recipientId, actorId, type, perfumeId, communityId }) {
  if (recipientId === actorId) return; // kendine bildirim gönderme
  return prisma.notification.create({
    data: { recipientId, actorId, type, perfumeId: perfumeId ?? null, communityId: communityId ?? null },
  });
}

// GET /api/notifications
router.get('/', auth, async (req, res) => {
  const page = parseInt(req.query.page) || 1;
  const limit = parseInt(req.query.limit) || 20;
  const skip = (page - 1) * limit;

  const [notifications, total] = await Promise.all([
    prisma.notification.findMany({
      where: { recipientId: req.userId },
      orderBy: { createdAt: 'desc' },
      skip,
      take: limit,
      include: {
        actor: { select: { id: true, username: true, avatar: true } },
      },
    }),
    prisma.notification.count({ where: { recipientId: req.userId } }),
  ]);

  const unread = await prisma.notification.count({
    where: { recipientId: req.userId, isRead: false },
  });

  res.json({ notifications, total, unread });
});

// GET /api/notifications/unread-count
router.get('/unread-count', auth, async (req, res) => {
  const count = await prisma.notification.count({
    where: { recipientId: req.userId, isRead: false },
  });
  res.json({ count });
});

// PATCH /api/notifications/:id/read
router.patch('/:id/read', auth, async (req, res) => {
  const id = parseInt(req.params.id);
  const notif = await prisma.notification.findUnique({ where: { id } });
  if (!notif || notif.recipientId !== req.userId) return res.status(403).json({ error: 'Yasak' });

  await prisma.notification.update({ where: { id }, data: { isRead: true } });
  res.json({ ok: true });
});

// PATCH /api/notifications/read-all
router.patch('/read-all', auth, async (req, res) => {
  await prisma.notification.updateMany({
    where: { recipientId: req.userId, isRead: false },
    data: { isRead: true },
  });
  res.json({ ok: true });
});

module.exports = router;
module.exports.createNotification = createNotification;
