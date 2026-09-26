export type Profession = 'Physician' | 'Dentist';
export type Gender = 'Male' | 'Female' | 'Other';
export type VerificationStatus = 'Unverified' | 'Pending' | 'Verified' | 'Rejected';
export type VerificationMethod = 'EDevletDocument' | 'InstitutionalEmail' | 'RegistryNumber';
export type SwipeDecision = 'Pass' | 'Like';

export interface AuthResponse {
  token: string;
  expiresAt: string;
  userId: string;
  hasProfile: boolean;
}

export interface Photo { id: string; url: string; isPrimary: boolean; order: number; }

export interface Profile {
  id: string; userId: string; displayName: string;
  profession: Profession; gender: Gender; age: number; city: string;
  bio?: string | null; verificationStatus: VerificationStatus;
  interestedIn: Gender; ageMin: number; ageMax: number; photos: Photo[];
}

export interface Candidate {
  profileId: string; userId: string; displayName: string;
  profession: Profession; gender: Gender; age: number; city: string;
  bio?: string | null; photos: Photo[];
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
