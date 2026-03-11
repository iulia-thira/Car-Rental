import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { Car, Search, Calendar, LayoutDashboard, LogOut, LogIn, PlusCircle } from 'lucide-react';

export default function Layout() {
  const { isAuthenticated, user, logout } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => { logout(); navigate('/login'); };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white border-b border-gray-200 sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <Link to="/" className="flex items-center gap-2 text-xl font-bold text-blue-600">
              <Car className="w-6 h-6" />
              DriveShare
            </Link>

            <div className="flex items-center gap-4">
              <Link to="/" className="flex items-center gap-1 text-gray-600 hover:text-blue-600 text-sm font-medium">
                <Search className="w-4 h-4" /> Find Cars
              </Link>

              {isAuthenticated && user?.role === 'Renter' && (
                <Link to="/my-bookings" className="flex items-center gap-1 text-gray-600 hover:text-blue-600 text-sm font-medium">
                  <Calendar className="w-4 h-4" /> My Bookings
                </Link>
              )}

              {isAuthenticated && user?.role === 'Owner' && (
                <>
                  <Link to="/owner" className="flex items-center gap-1 text-gray-600 hover:text-blue-600 text-sm font-medium">
                    <LayoutDashboard className="w-4 h-4" /> Dashboard
                  </Link>
                  <Link to="/owner/listings/new" className="flex items-center gap-1 text-gray-600 hover:text-blue-600 text-sm font-medium">
                    <PlusCircle className="w-4 h-4" /> List a Car
                  </Link>
                  <Link to="/owner/bookings" className="flex items-center gap-1 text-gray-600 hover:text-blue-600 text-sm font-medium">
                    <Calendar className="w-4 h-4" /> Bookings
                  </Link>
                </>
              )}

              {isAuthenticated ? (
                <div className="flex items-center gap-3">
                  <span className="text-sm text-gray-700">{user?.firstName}</span>
                  <button onClick={handleLogout} className="flex items-center gap-1 text-gray-500 hover:text-red-600 text-sm">
                    <LogOut className="w-4 h-4" />
                  </button>
                </div>
              ) : (
                <Link to="/login" className="flex items-center gap-1 bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700">
                  <LogIn className="w-4 h-4" /> Sign In
                </Link>
              )}
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Outlet />
      </main>
    </div>
  );
}
