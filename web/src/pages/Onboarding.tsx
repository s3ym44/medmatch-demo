import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api, ApiError, type CreateProfileInput } from '../api/client';
import type { Gender, Profession } from '../api/types';

export default function Onboarding() {
  const nav = useNavigate();
  const [form, setForm] = useState<CreateProfileInput>({
    displayName: '', profession: 'Physician', gender: 'Female',
    birthDate: '1995-01-01', city: 'Ankara', bio: '',
    interestedIn: 'Male', ageMin: 25, ageMax: 40,
  });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  function set<K extends keyof CreateProfileInput>(k: K, v: CreateProfileInput[K]) {
    setForm(f => ({ ...f, [k]: v }));
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null); setBusy(true);
    try {
      await api.createProfile(form);
      nav('/verify', { replace: true });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Kayıt başarısız.');
    } finally { setBusy(false); }
  }

  return (
    <div className="page narrow">
      <h2>Profilini oluştur</h2>
      <p className="muted">Bu bilgiler eşleşme ve keşif için kullanılır.</p>
      <form onSubmit={submit} className="form-grid">
        <label>Görünen ad
          <input value={form.displayName} onChange={e => set('displayName', e.target.value)} required />
        </label>
        <div className="row">
          <label>Meslek
            <select value={form.profession} onChange={e => set('profession', e.target.value as Profession)}>
              <option value="Physician">Hekim</option>
              <option value="Dentist">Diş Hekimi</option>
            </select>
          </label>
          <label>Cinsiyet
            <select value={form.gender} onChange={e => set('gender', e.target.value as Gender)}>
              <option value="Female">Kadın</option>
              <option value="Male">Erkek</option>
              <option value="Other">Diğer</option>
            </select>
          </label>
        </div>
        <div className="row">
          <label>Doğum tarihi
            <input type="date" value={form.birthDate} onChange={e => set('birthDate', e.target.value)} required />
          </label>
          <label>Şehir
            <input value={form.city} onChange={e => set('city', e.target.value)} required />
          </label>
        </div>
        <label>Hakkında
          <textarea value={form.bio ?? ''} onChange={e => set('bio', e.target.value)} rows={3}
            placeholder="Kısaca kendinden bahset" />
        </label>
        <fieldset>
          <legend>Tercihler</legend>
          <div className="row">
            <label>İlgilendiğin cinsiyet
              <select value={form.interestedIn} onChange={e => set('interestedIn', e.target.value as Gender)}>
                <option value="Male">Erkek</option>
                <option value="Female">Kadın</option>
                <option value="Other">Farketmez</option>
              </select>
            </label>
            <label>Yaş aralığı
              <div className="age-range">
                <input type="number" min={18} max={99} value={form.ageMin}
                  onChange={e => set('ageMin', Number(e.target.value))} />
                <span>–</span>
                <input type="number" min={18} max={99} value={form.ageMax}
                  onChange={e => set('ageMax', Number(e.target.value))} />
              </div>
            </label>
          </div>
        </fieldset>
        {error && <div className="error">{error}</div>}
        <button className="btn btn-primary" type="submit" disabled={busy}>
          {busy ? 'Kaydediliyor…' : 'Devam et'}
        </button>
      </form>
    </div>
  );
}
