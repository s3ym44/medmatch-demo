import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { PromptAnswerInput, PromptCatalogItem, PromptKey } from '../api/types';

export const MAX_PROMPTS = 3;
export const MAX_ANSWER = 200;

interface Props { value: PromptAnswerInput[]; onChange: (v: PromptAnswerInput[]) => void; }

/** Katalogtan en fazla 3 prompt seçtirir; her biri için sayaçlı textarea gösterir. */
export default function PromptPicker({ value, onChange }: Props) {
  const [catalog, setCatalog] = useState<PromptCatalogItem[]>([]);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    api.promptCatalog().then(setCatalog).catch(() => setFailed(true));
  }, []);

  const textOf = (k: PromptKey) => catalog.find(c => c.key === k)?.text ?? k;
  const chosen = new Set(value.map(v => v.promptKey));
  const available = catalog.filter(c => !chosen.has(c.key));

  const add = (key: PromptKey) => onChange([...value, { promptKey: key, answer: '' }]);
  const remove = (key: PromptKey) => onChange(value.filter(v => v.promptKey !== key));
  const setAnswer = (key: PromptKey, answer: string) =>
    onChange(value.map(v => (v.promptKey === key ? { ...v, answer } : v)));

  return (
    <fieldset className="prompt-picker">
      <legend>Kendini anlat (isteğe bağlı)</legend>
      <p className="muted small">En fazla {MAX_PROMPTS} prompt seç ve cevapla.</p>
      {failed && <div className="muted small">Prompt listesi yüklenemedi.</div>}

      {value.map(v => (
        <label key={v.promptKey} className="prompt-field">
          <span className="prompt-field-head">
            <span>{textOf(v.promptKey)}</span>
            <button type="button" className="linkbtn" onClick={() => remove(v.promptKey)}>Kaldır</button>
          </span>
          <textarea rows={3} maxLength={MAX_ANSWER} value={v.answer}
            onChange={e => setAnswer(v.promptKey, e.target.value)}
            placeholder="Hasta bilgisi paylaşmadan yaz." required />
          <span className="muted small counter">{v.answer.length}/{MAX_ANSWER}</span>
        </label>
      ))}

      {value.length < MAX_PROMPTS && available.length > 0 && (
        <select value="" onChange={e => e.target.value && add(e.target.value as PromptKey)}
          aria-label="Prompt ekle">
          <option value="">+ Prompt ekle…</option>
          {available.map(c => <option key={c.key} value={c.key}>{c.text}</option>)}
        </select>
      )}
    </fieldset>
  );
}
