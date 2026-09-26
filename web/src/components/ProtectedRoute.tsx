import { Navigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import type { ReactNode } from 'react';

export default function ProtectedRoute({ children }: { children: ReactNode }) {
  const { userId, ready } = useAuth();
  if (!ready) return null;
  if (!userId) return <Navigate to="/login" replace />;
  return <>{children}</>;
}
