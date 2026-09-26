import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api, ApiError } from '../api/client';
import type { Candidate, SwipeDecision } from '../api/types';
import { professionLabel } from '../api/types';
import Avatar from '../components/Avatar';

export default function Discover() {
  const [cards, setCards] = useState<Candidate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [matchName, setMatchName] = useState<string | null>(null);
  const [leaving, setLeaving] = useState<'left' | 'right' | null>(null);

  async function load() {
    setLoading(true); setError(null);
    try { setCards(await api.candidates(20)); }
    catch (err) { setError(err instanceof ApiError ? err.message : 'Yüklenemedi.'); }
    finally { setLoading(false); }
  }
  useEffect(() => { load(); }, []);

  const top = cards[0];

  async function decide(decision: SwipeDecision) {
    if (!top) return;
    setLeaving(decision === 'Like' ? 'right' : 'left');
    const target = top;
    try {
      const r = await api.swipe(target.userId, decision);
      if (r.matched) setMatchName(target.displayName);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'İşlem başarısız.');
    } finally {
      setTimeout(() => {
        setCards(cs => cs.slice(1));
        setLeaving(null);
      }, 220);
    }
  }

  return (
    <div className="page">
      <div className="page-head">
        <h2>Keşfet</h2>
        <Link to="/matches" className="link">Eşleşmeler →</Link>
      </div>

      {loading && <div className="muted">Yükleniyor…</div>}
      {error && <div className="error">{error}</div>}

      {!loading && !error && cards.length === 0 && (
        <div className="empty">
          <p>Şimdilik gösterilecek yeni profil kalmadı.</p>
          <button className="btn" onClick={load}>Yenile</button>
        </div>
      )}

      {top && (
        <div className="deck">
          <article className={`card ${leaving ? 'leaving-' + leaving : ''}`}>
            <div className="card-photo">
              <Avatar url={top.photos.find(p => p.isPrimary)?.url ?? top.photos[0]?.url} name={top.displayName} size={140} />
              <span className="badge-verified" title="Doğrulanmış hekim">✓ Doğrulanmış</span>
            </div>
            <div className="card-body">
              <h3>{top.displayName}, {top.age}</h3>
              <div className="chips">
                <span className="chip">{professionLabel[top.profession]}</span>
                <span className="chip">{top.city}</span>
              </div>
              {top.bio && <p className="bio">{top.bio}</p>}
            </div>
          </article>

          <div className="deck-actions">
            <button className="round pass" onClick={() => decide('Pass')} aria-label="Geç">✕</button>
            <button className="round like" onClick={() => decide('Like')} aria-label="Beğen">♥</button>
          </div>
          <p className="muted small center">{cards.length} profil sırada</p>
        </div>
      )}

      {matchName && (
        <div className="match-overlay" onClick={() => setMatchName(null)}>
          <div className="match-modal">
            <div className="match-spark">✦</div>
            <h3>Eşleştiniz!</h3>
            <p>{matchName} ile karşılıklı beğeni.</p>
            <Link className="btn btn-primary" to="/matches">Sohbete git</Link>
            <button className="btn ghost" onClick={() => setMatchName(null)}>Keşfe devam</button>
          </div>
        </div>
      )}
    </div>
  );
}
