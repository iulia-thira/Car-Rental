import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';

// Pages
import LoginPage from '@/pages/auth/LoginPage';
import RegisterPage from '@/pages/auth/RegisterPage';
import SearchPage from '@/pages/renter/SearchPage';
import CarDetailPage from '@/pages/renter/CarDetailPage';
import MyBookingsPage from '@/pages/renter/MyBookingsPage';
import OwnerDashboardPage from '@/pages/owner/OwnerDashboardPage';
import CreateListingPage from '@/pages/owner/CreateListingPage';
import OwnerBookingsPage from '@/pages/owner/OwnerBookingsPage';
import Layout from '@/components/layout/Layout';

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, staleTime: 30_000 } }
});

function PrivateRoute({ children, roles }: { children: React.ReactNode; roles?: string[] }) {
  const { isAuthenticated, user } = useAuthStore();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (roles && user && !roles.includes(user.role)) return <Navigate to="/" replace />;
  return <>{children}</>;
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          {/* Auth */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Public */}
          <Route path="/" element={<Layout />}>
            <Route index element={<SearchPage />} />
            <Route path="cars/:id" element={<CarDetailPage />} />

            {/* Renter */}
            <Route path="my-bookings" element={
              <PrivateRoute roles={['Renter']}><MyBookingsPage /></PrivateRoute>
            } />

            {/* Owner */}
            <Route path="owner" element={
              <PrivateRoute roles={['Owner']}><OwnerDashboardPage /></PrivateRoute>
            } />
            <Route path="owner/listings/new" element={
              <PrivateRoute roles={['Owner']}><CreateListingPage /></PrivateRoute>
            } />
            <Route path="owner/bookings" element={
              <PrivateRoute roles={['Owner']}><OwnerBookingsPage /></PrivateRoute>
            } />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
