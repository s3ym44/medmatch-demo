import { useEffect, useRef, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import { api, ApiError } from '../api/client';
import type { Message } from '../api/types';
import { useAuth } from '../auth/AuthContext';
import { createChatConnection } from '../realtime/signalr';

export default function Chat() {
  const { matchId = '' } = useParams();
  const { userId } = useAuth();
  const [messages, setMessages] = useState<Message[]>([]);
  const [text, setText] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [connected, setConnected] = useState(false);
  const endRef = useRef<HTMLDivElement | null>(null);

  function addUnique(msg: Message) {
    setMessages(prev => prev.some(m => m.id === msg.id) ? prev : [...prev, msg]);
  }

  // ilk yükleme
  useEffect(() => {
    (async () => {
      try { setMessages(await api.messages(matchId)); }
      catch (err) { setError(err instanceof ApiError ? err.message : 'Mesajlar yüklenemedi.'); }
    })();
  }, [matchId]);

  // SignalR bağlantısı
  useEffect(() => {
    const conn = createChatConnection();
    conn.on('ReceiveMessage', (msg: Message) => { if (msg.matchId === matchId) addUnique(msg); });
    let active = true;
    conn.start()
      .then(() => conn.invoke('JoinMatch', matchId))
      .then(() => { if (active) setConnected(true); })
      .catch(() => { if (active) setConnected(false); });
    return () => {
      active = false;
      if (conn.state === signalR.HubConnectionState.Connected) conn.invoke('LeaveMatch', matchId).catch(() => {});
      conn.stop().catch(() => {});
    };
  }, [matchId]);

  useEffect(() => { endRef.current?.scrollIntoView({ behavior: 'smooth' }); }, [messages]);

  async function send(e: React.FormEvent) {
    e.preventDefault();
    const content = text.trim();
    if (!content) return;
    setText('');
    try {
      const msg = await api.sendMessage(matchId, content);
      addUnique(msg); // yayın da gelecek ama id ile tekilleştiriyoruz
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Gönderilemedi.');
    }
  }

  return (
    <div className="page chat-page">
      <div className="page-head">
        <Link to="/matches" className="link">← Eşleşmeler</Link>
        <span className={connected ? 'conn ok' : 'conn'}>{connected ? '● canlı' : '○ bağlanıyor'}</span>
      </div>
      {error && <div className="error">{error}</div>}

      <div className="messages">
        {messages.map(m => (
          <div key={m.id} className={m.senderId === userId ? 'bubble mine' : 'bubble theirs'}>
            <span>{m.content}</span>
            <time>{new Date(m.sentAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}</time>
          </div>
        ))}
        <div ref={endRef} />
      </div>

      <form className="composer" onSubmit={send}>
        <input value={text} onChange={e => setText(e.target.value)} placeholder="Mesaj yaz…" />
        <button className="btn btn-primary" type="submit">Gönder</button>
      </form>
    </div>
  );
}
