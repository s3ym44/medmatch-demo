import { BrowserRouter, Routes, Route, Navigate, Link, useLocation } from 'react-router-dom';
import { AuthProvider, useAuth } from './auth/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Login from './pages/Login';
import Onboarding from './pages/Onboarding';
import Verify from './pages/Verify';
import Discover from './pages/Discover';
import Matches from './pages/Matches';
import Chat from './pages/Chat';

function TopBar() {
  const { userId, logout } = useAuth();
  const loc = useLocation();
  if (!userId || loc.pathname === '/login') return null;
  return (
    <header className="topbar">
      <Link to="/discover" className="topbar-brand"><span className="brand-mark">◐</span> MedMatch</Link>
      <nav>
        <Link to="/discover">Keşfet</Link>
        <Link to="/matches">Eşleşmeler</Link>
        <button className="linkbtn" onClick={logout}>Çıkış</button>
      </nav>
    </header>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <TopBar />
        <main className="app-main">
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/onboarding" element={<ProtectedRoute><Onboarding /></ProtectedRoute>} />
            <Route path="/verify" element={<ProtectedRoute><Verify /></ProtectedRoute>} />
            <Route path="/discover" element={<ProtectedRoute><Discover /></ProtectedRoute>} />
            <Route path="/matches" element={<ProtectedRoute><Matches /></ProtectedRoute>} />
            <Route path="/chat/:matchId" element={<ProtectedRoute><Chat /></ProtectedRoute>} />
            <Route path="*" element={<Navigate to="/discover" replace />} />
          </Routes>
        </main>
      </AuthProvider>
    </BrowserRouter>
  );
}
