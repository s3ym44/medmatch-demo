import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api, ApiError, type CreateProfileInput } from '../api/client';
import type {
  CareerStage, Gender, MandatoryServiceStatus, NightShiftLoad, Profession, RelocationOpenness, WorkSchedule,
} from '../api/types';
import {
  careerStageLabel, mandatoryServiceLabel, nightShiftLoadLabel, relocationLabel, workScheduleLabel,
} from '../api/types';
import PromptPicker from '../components/PromptPicker';

function options<T extends string>(labels: Record<T, string>) {
  return (Object.keys(labels) as T[]).map(k => <option key={k} value={k}>{labels[k]}</option>);
}

export default function Onboarding() {
  const nav = useNavigate();
  const [form, setForm] = useState<CreateProfileInput>({
    displayName: '', profession: 'Physician', gender: 'Female',
    birthDate: '1995-01-01', city: 'Ankara', bio: '',
    interestedIn: 'Male', ageMin: 25, ageMax: 40,
    workSchedule: 'Daytime', nightShiftLoad: 'None', mandatoryService: 'Completed',
    relocation: 'Open', careerStage: 'Specialist', prompts: [],
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
        <label>Kısa motto (isteğe bağlı)
          <input value={form.bio ?? ''} onChange={e => set('bio', e.target.value)} maxLength={100}
            placeholder="Tek satırda kendini anlat" />
        </label>
        <fieldset>
          <legend>Takvim ve coğrafya</legend>
          <div className="row">
            <label>Kariyer aşaması
              <select value={form.careerStage} onChange={e => set('careerStage', e.target.value as CareerStage)}>
                {options(careerStageLabel)}
              </select>
            </label>
            <label>Çalışma düzeni
              <select value={form.workSchedule} onChange={e => set('workSchedule', e.target.value as WorkSchedule)}>
                {options(workScheduleLabel)}
              </select>
            </label>
          </div>
          <div className="row">
            <label>Nöbet yoğunluğu (aylık)
              <select value={form.nightShiftLoad} onChange={e => set('nightShiftLoad', e.target.value as NightShiftLoad)}>
                {options(nightShiftLoadLabel)}
              </select>
            </label>
            <label>Mecburi hizmet
              <select value={form.mandatoryService}
                onChange={e => set('mandatoryService', e.target.value as MandatoryServiceStatus)}>
                {options(mandatoryServiceLabel)}
              </select>
            </label>
          </div>
          <label>Tayin / şehir değiştirme
            <select value={form.relocation} onChange={e => set('relocation', e.target.value as RelocationOpenness)}>
              {options(relocationLabel)}
            </select>
          </label>
        </fieldset>
        <PromptPicker value={form.prompts} onChange={v => set('prompts', v)} />
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
