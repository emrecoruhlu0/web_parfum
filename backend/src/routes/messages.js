const express = require('express');
const prisma = require('../prisma/client');
const auth = require('../middleware/auth');

const router = express.Router();

// GET /api/messages/conversations  →  konuşma listesi (son mesaj, okunmamış sayı)
router.get('/conversations', auth, async (req, res) => {
  // Benimle konuşma yapmış benzersiz kullanıcı id'leri
  const sent = await prisma.message.findMany({
    where: { senderId: req.userId },
    select: { recipientId: true },
    distinct: ['recipientId'],
  });
  const received = await prisma.message.findMany({
    where: { recipientId: req.userId },
    select: { senderId: true },
    distinct: ['senderId'],
  });

  const partnerIds = [...new Set([
    ...sent.map(m => m.recipientId),
    ...received.map(m => m.senderId),
  ])];

  const conversations = await Promise.all(partnerIds.map(async (partnerId) => {
    const last = await prisma.message.findFirst({
      where: {
        OR: [
          { senderId: req.userId, recipientId: partnerId },
          { senderId: partnerId, recipientId: req.userId },
        ],
      },
      orderBy: { createdAt: 'desc' },
      include: { sender: { select: { id: true, username: true, avatar: true } } },
    });
    const unread = await prisma.message.count({
      where: { senderId: partnerId, recipientId: req.userId, isRead: false },
    });
    const partner = await prisma.user.findUnique({
      where: { id: partnerId },
      select: { id: true, username: true, avatar: true },
    });
    return { partner, lastMessage: last, unread };
  }));

  conversations.sort((a, b) => new Date(b.lastMessage?.createdAt) - new Date(a.lastMessage?.createdAt));
  res.json(conversations);
});

// GET /api/messages/:username  →  belirli kullanıcıyla mesajlar
router.get('/:username', auth, async (req, res) => {
  const partner = await prisma.user.findUnique({ where: { username: req.params.username } });
  if (!partner) return res.status(404).json({ error: 'Kullanıcı bulunamadı' });

  const messages = await prisma.message.findMany({
    where: {
      OR: [
        { senderId: req.userId, recipientId: partner.id },
        { senderId: partner.id, recipientId: req.userId },
      ],
    },
    orderBy: { createdAt: 'asc' },
    include: { sender: { select: { id: true, username: true, avatar: true } } },
  });

  // Okunmamışları okundu yap
  await prisma.message.updateMany({
    where: { senderId: partner.id, recipientId: req.userId, isRead: false },
    data: { isRead: true },
  });

  res.json({ partner, messages });
});

// POST /api/messages/:username  →  mesaj gönder
router.post('/:username', auth, async (req, res) => {
  const { body } = req.body;
  if (!body?.trim()) return res.status(400).json({ error: 'Mesaj boş olamaz' });

  const recipient = await prisma.user.findUnique({ where: { username: req.params.username } });
  if (!recipient) return res.status(404).json({ error: 'Kullanıcı bulunamadı' });
  if (recipient.id === req.userId) return res.status(400).json({ error: 'Kendine mesaj gönderemezsin' });

  const message = await prisma.message.create({
    data: { senderId: req.userId, recipientId: recipient.id, body: body.trim() },
    include: { sender: { select: { id: true, username: true, avatar: true } } },
  });

  res.status(201).json(message);
});

module.exports = router;
