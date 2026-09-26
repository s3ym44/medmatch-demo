import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { api, getToken, setToken } from '../api/client';

interface AuthState {
  userId: string | null;
  token: string | null;
  ready: boolean;
  login: (email: string, password: string) => Promise<boolean>;
  register: (email: string, password: string) => Promise<boolean>;
  loginDemo: () => Promise<boolean>;
  logout: () => void;
}

const AuthContext = createContext<AuthState | null>(null);

function decodeUserId(token: string): string | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
    return payload.sub ?? null;
  } catch { return null; }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setTok] = useState<string | null>(getToken());
  const [userId, setUserId] = useState<string | null>(token ? decodeUserId(token) : null);
  const [ready, setReady] = useState(false);

  useEffect(() => { setReady(true); }, []);

  function applyToken(t: string) {
    setToken(t);
    setTok(t);
    setUserId(decodeUserId(t));
  }

  const value = useMemo<AuthState>(() => ({
    userId, token, ready,
    login: async (email, password) => {
      const r = await api.login(email, password);
      applyToken(r.token);
      return r.hasProfile;
    },
    register: async (email, password) => {
      const r = await api.register(email, password);
      applyToken(r.token);
      return r.hasProfile;
    },
    loginDemo: async () => {
      const c = await api.demoCredentials();
      const r = await api.login(c.email, c.password);
      applyToken(r.token);
      return r.hasProfile;
    },
    logout: () => { setToken(null); setTok(null); setUserId(null); },
  }), [userId, token, ready]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth AuthProvider içinde kullanılmalı');
  return ctx;
}
