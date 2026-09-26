import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api, ApiError } from '../api/client';
import type { Match } from '../api/types';
import Avatar from '../components/Avatar';

export default function Matches() {
  const [matches, setMatches] = useState<Match[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      try { setMatches(await api.matches()); }
      catch (err) { setError(err instanceof ApiError ? err.message : 'Yüklenemedi.'); }
      finally { setLoading(false); }
    })();
  }, []);

  return (
    <div className="page">
      <div className="page-head">
        <h2>Eşleşmeler</h2>
        <Link to="/discover" className="link">← Keşfet</Link>
      </div>
      {loading && <div className="muted">Yükleniyor…</div>}
      {error && <div className="error">{error}</div>}
      {!loading && matches.length === 0 && <div className="empty"><p>Henüz eşleşmen yok. Keşfette beğen!</p></div>}

      <ul className="match-list">
        {matches.map(m => (
          <li key={m.matchId}>
            <Link to={`/chat/${m.matchId}`} className="match-row">
              <Avatar url={m.otherPhotoUrl} name={m.otherDisplayName} size={52} />
              <div className="match-meta">
                <div className="match-name">{m.otherDisplayName}</div>
                <div className="match-last">{m.lastMessage ?? 'Yeni eşleşme — merhaba de!'}</div>
              </div>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
