import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Plus, Eye, Trash2, MapPin, Star } from 'lucide-react';
import { listingsApi } from '@/services/apiServices';

export default function OwnerDashboardPage() {
  const qc = useQueryClient();
  const { data: listings, isLoading } = useQuery({
    queryKey: ['listings', 'my'],
    queryFn: listingsApi.getMyListings,
  });

  const deleteMutation = useMutation({
    mutationFn: listingsApi.delete,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['listings', 'my'] }),
  });

  if (isLoading) return <div className="animate-pulse space-y-4">{Array.from({ length: 3 }).map((_, i) => <div key={i} className="h-32 bg-gray-200 rounded-xl" />)}</div>;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-900">My Listings</h1>
        <Link to="/owner/listings/new"
          className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700">
          <Plus className="w-4 h-4" /> Add Car
        </Link>
      </div>

      {!listings?.length ? (
        <div className="text-center py-16 bg-gray-50 rounded-2xl">
          <p className="text-4xl mb-3">🚗</p>
          <p className="font-semibold text-gray-700">No cars listed yet</p>
          <p className="text-sm text-gray-400 mt-1 mb-6">List your first car and start earning</p>
          <Link to="/owner/listings/new" className="bg-blue-600 text-white px-6 py-2.5 rounded-lg font-medium hover:bg-blue-700">
            List a Car
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {listings.map(car => (
            <div key={car.id} className="bg-white border border-gray-200 rounded-xl overflow-hidden shadow-sm">
              <div className="h-40 bg-gray-100">
                {car.photos[0] ? (
                  <img src={car.photos[0]} alt={`${car.make} ${car.model}`} className="w-full h-full object-cover" />
                ) : (
                  <div className="w-full h-full flex items-center justify-center text-3xl">🚗</div>
                )}
              </div>
              <div className="p-4">
                <div className="flex justify-between items-start mb-1">
                  <h3 className="font-semibold text-gray-900">{car.year} {car.make} {car.model}</h3>
                  <span className={`text-xs px-2 py-0.5 rounded-full ${car.status === 'Active' ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'}`}>
                    {car.status}
                  </span>
                </div>
                <div className="flex items-center gap-1 text-gray-400 text-xs mb-2">
                  <MapPin className="w-3 h-3" /> {car.city}
                </div>
                <p className="text-blue-600 font-semibold text-sm mb-3">${car.pricePerDay}/day</p>
                <div className="flex gap-2">
                  <Link to={`/cars/${car.id}`}
                    className="flex items-center gap-1 text-xs border border-gray-200 px-3 py-1.5 rounded-lg hover:bg-gray-50">
                    <Eye className="w-3 h-3" /> View
                  </Link>
                  <button onClick={() => { if (confirm('Delete this listing?')) deleteMutation.mutate(car.id); }}
                    className="flex items-center gap-1 text-xs border border-red-200 text-red-600 px-3 py-1.5 rounded-lg hover:bg-red-50">
                    <Trash2 className="w-3 h-3" /> Delete
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
