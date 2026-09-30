export type Profession = 'Physician' | 'Dentist';
export type Gender = 'Male' | 'Female' | 'Other';
export type VerificationStatus = 'Unverified' | 'Pending' | 'Verified' | 'Rejected';
export type VerificationMethod = 'EDevletDocument' | 'InstitutionalEmail' | 'RegistryNumber';
export type SwipeDecision = 'Pass' | 'Like';
export type WorkSchedule = 'Daytime' | 'Shifts' | 'Mixed';
export type NightShiftLoad = 'None' | 'Light' | 'Moderate' | 'Heavy';
export type MandatoryServiceStatus = 'Completed' | 'InProgress' | 'Pending' | 'NotApplicable';
export type RelocationOpenness = 'Open' | 'Depends' | 'Closed';
export type CareerStage = 'Intern' | 'Resident' | 'Specialist' | 'Academic' | 'GeneralPractitioner';
export type PromptKey =
  | 'NightShiftSurvival' | 'CantTellPatients' | 'TusWinDay' | 'IncompatibleSpecialty'
  | 'FreeWeekend' | 'HowToLoseMe' | 'OffDutyDifferent';

export interface PromptDto { promptKey: PromptKey; promptText: string; answer: string; }
export interface PromptCatalogItem { key: PromptKey; text: string; }
export interface PromptAnswerInput { promptKey: PromptKey; answer: string; }

export interface ScheduleFields {
  workSchedule: WorkSchedule; nightShiftLoad: NightShiftLoad;
  mandatoryService: MandatoryServiceStatus; relocation: RelocationOpenness; careerStage: CareerStage;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  userId: string;
  hasProfile: boolean;
}

export interface Photo { id: string; url: string; isPrimary: boolean; order: number; }

export interface Profile extends ScheduleFields {
  id: string; userId: string; displayName: string;
  profession: Profession; gender: Gender; age: number; city: string;
  bio?: string | null; verificationStatus: VerificationStatus;
  interestedIn: Gender; ageMin: number; ageMax: number; photos: Photo[];
  prompts: PromptDto[];
}


export interface Candidate extends ScheduleFields {
  profileId: string; userId: string; displayName: string;
  profession: Profession; gender: Gender; age: number; city: string;
  bio?: string | null; photos: Photo[]; prompts: PromptDto[];
}

export interface SwipeResult { matched: boolean; matchId?: string | null; }

export interface Match {
  matchId: string; otherUserId: string; otherDisplayName: string;
  otherPhotoUrl?: string | null; createdAt: string;
  lastMessage?: string | null; lastMessageAt?: string | null;
}

export interface Message {
  id: string; matchId: string; senderId: string;
  content: string; sentAt: string; readAt?: string | null;
}

export interface VerificationResult {
  status: VerificationStatus; method: VerificationMethod;
  reviewedAt?: string | null; reason?: string | null;
}

export const professionLabel: Record<Profession, string> = {
  Physician: 'Hekim',
  Dentist: 'Diş Hekimi',
};

export const workScheduleLabel: Record<WorkSchedule, string> = {
  Daytime: 'Gündüz', Shifts: 'Vardiya / nöbet', Mixed: 'Karışık',
};
export const nightShiftLoadLabel: Record<NightShiftLoad, string> = {
  None: 'Yok', Light: 'Az (1-3)', Moderate: 'Orta (4-7)', Heavy: 'Yoğun (8+)',
};
export const mandatoryServiceLabel: Record<MandatoryServiceStatus, string> = {
  Completed: 'Yaptım', InProgress: 'Yapıyorum', Pending: 'Yapacağım', NotApplicable: 'Muaf',
};
export const relocationLabel: Record<RelocationOpenness, string> = {
  Open: 'Açığım', Depends: 'Duruma göre', Closed: 'Kapalıyım',
};
export const careerStageLabel: Record<CareerStage, string> = {
  Intern: 'İntörn', Resident: 'Asistan', Specialist: 'Uzman',
  Academic: 'Öğretim üyesi', GeneralPractitioner: 'Pratisyen',
};
