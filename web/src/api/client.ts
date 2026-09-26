import type {
  AuthResponse, Profile, Candidate, SwipeResult, Match, Message,
  VerificationResult, VerificationMethod, Gender, Profession, SwipeDecision,
} from './types';

const TOKEN_KEY = 'medmatch.token';

export function getToken(): string | null {
  try { return localStorage.getItem(TOKEN_KEY); } catch { return null; }
}
export function setToken(token: string | null) {
  try {
    if (token) localStorage.setItem(TOKEN_KEY, token);
    else localStorage.removeItem(TOKEN_KEY);
  } catch { /* yoksay */ }
}

class ApiError extends Error {
  status: number;
  constructor(status: number, message: string) { super(message); this.status = status; }
}

async function req<T>(method: string, path: string, body?: unknown): Promise<T> {
  const headers: Record<string, string> = {};
  const token = getToken();
  if (token) headers['Authorization'] = `Bearer ${token}`;
  if (body !== undefined) headers['Content-Type'] = 'application/json';

  const res = await fetch(path, {
    method, headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!res.ok) {
    let msg = `Hata ${res.status}`;
    try { const j = await res.json(); if (j?.error) msg = j.error; } catch { /* */ }
    throw new ApiError(res.status, msg);
  }
  if (res.status === 204) return undefined as T;
  const text = await res.text();
  return text ? (JSON.parse(text) as T) : (undefined as T);
}

export interface CreateProfileInput {
  displayName: string; profession: Profession; gender: Gender;
  birthDate: string; city: string; bio?: string;
  interestedIn: Gender; ageMin: number; ageMax: number;
}

export const api = {
  register: (email: string, password: string) =>
    req<AuthResponse>('POST', '/api/auth/register', { email, password }),
  login: (email: string, password: string) =>
    req<AuthResponse>('POST', '/api/auth/login', { email, password }),
  demoCredentials: () =>
    req<{ email: string; password: string }>('GET', '/api/auth/demo-credentials'),

  myProfile: () => req<Profile>('GET', '/api/profiles/me'),
  createProfile: (input: CreateProfileInput) => req<Profile>('POST', '/api/profiles', input),

  submitVerification: (method: VerificationMethod, documentRef?: string) =>
    req<VerificationResult>('POST', '/api/verification/submit', { method, documentRef }),

  candidates: (take = 20) => req<Candidate[]>('GET', `/api/discovery/candidates?take=${take}`),
  swipe: (targetUserId: string, decision: SwipeDecision) =>
    req<SwipeResult>('POST', '/api/matches/swipe', { targetUserId, decision }),

  matches: () => req<Match[]>('GET', '/api/matches'),
  messages: (matchId: string) => req<Message[]>('GET', `/api/matches/${matchId}/messages`),
  sendMessage: (matchId: string, content: string) =>
    req<Message>('POST', `/api/matches/${matchId}/messages`, { content }),
};

export { ApiError };
