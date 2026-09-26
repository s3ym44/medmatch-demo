import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api, ApiError } from '../api/client';
import type { VerificationMethod, VerificationStatus } from '../api/types';

const methods: { value: VerificationMethod; label: string; hint: string }[] = [
  { value: 'EDevletDocument', label: 'e-Devlet barkodlu belge', hint: 'YÖK mezuniyet belgesinin barkod/doğrulama kodu' },
  { value: 'InstitutionalEmail', label: 'Kurumsal e-posta', hint: 'Hastane veya üniversite uzantılı adres' },
  { value: 'RegistryNumber', label: 'TTB / TDB sicil no', hint: 'Oda sicil numarası' },
];

export default function Verify() {
  const nav = useNavigate();
  const [method, setMethod] = useState<VerificationMethod>('EDevletDocument');
  const [ref, setRef] = useState('');
  const [status, setStatus] = useState<VerificationStatus | null>(null);
  const [reason, setReason] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null); setBusy(true); setStatus(null);
    try {
      const r = await api.submitVerification(method, ref || undefined);
      setStatus(r.status);
      setReason(r.reason ?? null);
      if (r.status === 'Verified') setTimeout(() => nav('/discover', { replace: true }), 1200);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Doğrulama başarısız.');
    } finally { setBusy(false); }
  }

  return (
    <div className="page narrow">
      <h2>Meslek doğrulaması</h2>
      <div className="info-banner">
        Bu bir <strong>demo</strong>dur: doğrulama sahte bir sağlayıcıyla taklit edilir, gerçek belge
        gönderilmez. Gerçek üründe bu adım e-Devlet barkodlu belge, kurumsal e-posta veya oda sicili
        ile yapılır.
      </div>

      <form onSubmit={submit} className="form-grid">
        <label>Yöntem
          <select value={method} onChange={e => setMethod(e.target.value as VerificationMethod)}>
            {methods.map(m => <option key={m.value} value={m.value}>{m.label}</option>)}
          </select>
        </label>
        <p className="muted small">{methods.find(m => m.value === method)?.hint}</p>
        <label>Belge / kanıt referansı (opsiyonel)
          <input value={ref} onChange={e => setRef(e.target.value)}
            placeholder="örn. barkod kodu — 'reject' yazarsan reddedilir" />
        </label>
        {error && <div className="error">{error}</div>}
        {status && (
          <div className={status === 'Verified' ? 'result ok' : status === 'Rejected' ? 'result bad' : 'result'}>
            Durum: <strong>{status}</strong>{reason ? ` — ${reason}` : ''}
          </div>
        )}
        <button className="btn btn-primary" type="submit" disabled={busy}>
          {busy ? 'İnceleniyor…' : 'Doğrulamayı gönder'}
        </button>
      </form>
    </div>
  );
}
