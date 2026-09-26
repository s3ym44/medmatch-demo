import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { ApiError } from '../api/client';

export default function Login() {
  const { login, register, loginDemo } = useAuth();
  const nav = useNavigate();
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null); setBusy(true);
    try {
      const hasProfile = mode === 'login'
        ? await login(email, password)
        : await register(email, password);
      nav(hasProfile ? '/discover' : '/onboarding', { replace: true });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Bir hata oluştu.');
    } finally { setBusy(false); }
  }

  async function demo() {
    setError(null); setBusy(true);
    try {
      const hasProfile = await loginDemo();
      nav(hasProfile ? '/discover' : '/onboarding', { replace: true });
    } catch {
      setError('Demo girişi başarısız. Backend çalışıyor mu?');
    } finally { setBusy(false); }
  }

  return (
    <div className="auth-screen">
      <div className="auth-card">
        <div className="brand">
          <span className="brand-mark">◐</span>
          <h1>MedMatch</h1>
        </div>
        <p className="tagline">Yalnızca doğrulanmış hekim ve diş hekimleri için tanışma.</p>

        <button className="btn btn-demo" onClick={demo} disabled={busy}>
          Demo hesabıyla tek tıkla gir
        </button>
        <div className="divider"><span>veya</span></div>

        <div className="tabs">
          <button className={mode === 'login' ? 'tab active' : 'tab'} onClick={() => setMode('login')}>Giriş</button>
          <button className={mode === 'register' ? 'tab active' : 'tab'} onClick={() => setMode('register')}>Kayıt</button>
        </div>

        <form onSubmit={submit}>
          <label>E-posta
            <input type="email" value={email} onChange={e => setEmail(e.target.value)}
              placeholder="ornek@hastane.gov.tr" required />
          </label>
          <label>Parola
            <input type="password" value={password} onChange={e => setPassword(e.target.value)}
              placeholder="En az 6 karakter" minLength={6} required />
          </label>
          {error && <div className="error">{error}</div>}
          <button className="btn btn-primary" type="submit" disabled={busy}>
            {busy ? 'Lütfen bekleyin…' : mode === 'login' ? 'Giriş yap' : 'Kayıt ol'}
          </button>
        </form>
      </div>
    </div>
  );
}
