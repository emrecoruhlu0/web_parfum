import React, { useEffect, useState, useRef } from 'react';
import { MessageCircle, Send } from 'lucide-react';
import Sidebar from '../components/Sidebar';
import { messagesApi } from '../api/messages';
import { useAuth } from '../context/AuthContext';

function ConvoItem({ convo, active, onClick }) {
  const { partner, lastMessage, unread } = convo;
  return (
    <button
      onClick={onClick}
      className={`w-full flex items-center gap-3 p-3 rounded-xl text-left transition-colors ${
        active ? 'bg-amber-50' : 'hover:bg-gray-50'
      }`}
    >
      <img
        src={partner.avatar || `https://i.pravatar.cc/150?u=${partner.username}`}
        alt={partner.username}
        className="w-10 h-10 rounded-full shrink-0"
      />
      <div className="flex-1 min-w-0">
        <div className="flex justify-between items-center">
          <p className="font-semibold text-sm text-gray-900 truncate">@{partner.username}</p>
          {unread > 0 && (
            <span className="bg-amber-500 text-white text-[10px] font-bold rounded-full min-w-[18px] h-[18px] flex items-center justify-center px-1 shrink-0">
              {unread}
            </span>
          )}
        </div>
        {lastMessage && (
          <p className="text-xs text-gray-400 truncate">{lastMessage.body}</p>
        )}
      </div>
    </button>
  );
}

function MessageBubble({ msg, isMe }) {
  return (
    <div className={`flex ${isMe ? 'justify-end' : 'justify-start'}`}>
      <div className={`max-w-[75%] px-4 py-2.5 rounded-2xl text-sm ${
        isMe
          ? 'bg-amber-600 text-white rounded-br-sm'
          : 'bg-white border border-gray-100 text-gray-800 rounded-bl-sm'
      }`}>
        <p>{msg.body}</p>
        <p className={`text-[10px] mt-1 ${isMe ? 'text-amber-200' : 'text-gray-400'}`}>
          {new Date(msg.createdAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}
        </p>
      </div>
    </div>
  );
}

export default function MessagesPage() {
  const { user } = useAuth();
  const [conversations, setConversations] = useState([]);
  const [activeUsername, setActiveUsername] = useState(null);
  const [thread, setThread] = useState(null);
  const [newMsg, setNewMsg] = useState('');
  const [sending, setSending] = useState(false);
  const bottomRef = useRef(null);

  useEffect(() => {
    messagesApi.getConversations().then(setConversations);
  }, []);

  useEffect(() => {
    if (!activeUsername) return;
    messagesApi.getThread(activeUsername).then(data => {
      setThread(data);
      // okunmamış sayısını sıfırla
      setConversations(prev => prev.map(c =>
        c.partner.username === activeUsername ? { ...c, unread: 0 } : c
      ));
    });
  }, [activeUsername]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [thread?.messages]);

  const handleSend = async (e) => {
    e.preventDefault();
    if (!newMsg.trim() || !activeUsername) return;
    setSending(true);
    try {
      const msg = await messagesApi.send(activeUsername, newMsg.trim());
      setNewMsg('');
      setThread(prev => ({ ...prev, messages: [...prev.messages, msg] }));
      setConversations(prev => {
        const updated = prev.map(c =>
          c.partner.username === activeUsername ? { ...c, lastMessage: msg } : c
        );
        if (!updated.find(c => c.partner.username === activeUsername)) {
          updated.unshift({ partner: thread.partner, lastMessage: msg, unread: 0 });
        }
        return updated;
      });
    } finally {
      setSending(false);
    }
  };

  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex">

        {/* Sol: Konuşmalar */}
        <div className="w-72 border-r border-gray-100 bg-white flex flex-col">
          <div className="p-5 border-b border-gray-100">
            <h2 className="font-serif text-xl font-bold flex items-center gap-2">
              <MessageCircle size={22} className="text-amber-500" /> Mesajlar
            </h2>
          </div>
          <div className="flex-1 overflow-y-auto p-3 space-y-1">
            {conversations.length === 0 && (
              <p className="text-center text-gray-400 text-sm py-8">Henüz mesaj yok.</p>
            )}
            {conversations.map(c => (
              <ConvoItem
                key={c.partner.id}
                convo={c}
                active={activeUsername === c.partner.username}
                onClick={() => setActiveUsername(c.partner.username)}
              />
            ))}
          </div>
        </div>

        {/* Sağ: Mesaj dizisi */}
        <div className="flex-1 flex flex-col">
          {!activeUsername ? (
            <div className="flex-1 flex items-center justify-center text-gray-400">
              <div className="text-center">
                <MessageCircle size={48} className="mx-auto mb-3 text-gray-200" />
                <p className="font-medium">Bir konuşma seç</p>
                <p className="text-sm mt-1">Kullanıcı profilinden yeni konuşma başlatabilirsin.</p>
              </div>
            </div>
          ) : (
            <>
              {/* Başlık */}
              {thread && (
                <div className="p-4 border-b border-gray-100 bg-white flex items-center gap-3">
                  <img
                    src={thread.partner.avatar || `https://i.pravatar.cc/150?u=${thread.partner.username}`}
                    alt={thread.partner.username}
                    className="w-9 h-9 rounded-full"
                  />
                  <p className="font-semibold text-sm">@{thread.partner.username}</p>
                </div>
              )}

              {/* Mesajlar */}
              <div className="flex-1 overflow-y-auto p-5 space-y-3">
                {thread?.messages.map(msg => (
                  <MessageBubble key={msg.id} msg={msg} isMe={msg.sender.id === user?.id} />
                ))}
                <div ref={bottomRef} />
              </div>

              {/* Giriş kutusu */}
              <form onSubmit={handleSend} className="p-4 border-t border-gray-100 bg-white flex gap-3">
                <input
                  value={newMsg}
                  onChange={e => setNewMsg(e.target.value)}
                  placeholder="Mesajını yaz..."
                  className="flex-1 bg-gray-50 border border-gray-200 rounded-xl px-4 py-2.5 text-sm outline-none focus:ring-1 focus:ring-amber-300"
                />
                <button
                  type="submit"
                  disabled={sending || !newMsg.trim()}
                  className="bg-amber-600 text-white px-4 py-2.5 rounded-xl hover:bg-amber-700 transition disabled:opacity-50"
                >
                  <Send size={18} />
                </button>
              </form>
            </>
          )}
        </div>

      </div>
    </div>
  );
}
