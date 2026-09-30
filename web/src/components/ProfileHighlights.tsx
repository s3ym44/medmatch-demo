import type { CareerStage, PromptDto, RelocationOpenness, WorkSchedule } from '../api/types';
import { careerStageLabel, relocationLabel, workScheduleLabel } from '../api/types';

interface BadgeFields { careerStage: CareerStage; workSchedule: WorkSchedule; relocation: RelocationOpenness; }

/** Kariyer aşaması + çalışma düzeni + tayin açıklığı rozetleri. */
export function ProfileBadges({ profile }: { profile: BadgeFields }) {
  return (
    <div className="chips badges">
      <span className="chip badge">{careerStageLabel[profile.careerStage]}</span>
      <span className="chip badge">{workScheduleLabel[profile.workSchedule]}</span>
      <span className="chip badge">Tayin: {relocationLabel[profile.relocation]}</span>
    </div>
  );
}

/** Prompt cevapları kart olarak. */
export function PromptCards({ prompts }: { prompts: PromptDto[] }) {
  if (prompts.length === 0) return null;
  return (
    <div className="prompt-cards">
      {prompts.map(p => (
        <div className="prompt-card" key={p.promptKey}>
          <div className="prompt-q">{p.promptText}</div>
          <div className="prompt-a">{p.answer}</div>
        </div>
      ))}
    </div>
  );
}
