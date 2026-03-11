import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Search, MapPin, Star, Users, Fuel, Settings } from 'lucide-react';
import { listingsApi } from '@/services/apiServices';
import type { SearchListingsParams } from '@/types';

export default function SearchPage() {
  const [params, setParams] = useState<SearchListingsParams>({ page: 1, pageSize: 12 });
  const [city, setCity] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  const { data, isLoading } = useQuery({
    queryKey: ['listings', 'search', params],
    queryFn: () => listingsApi.search(params),
  });

  const handleSearch = () => {
    setParams(p => ({ ...p, city: city || undefined, startDate: startDate || undefined, endDate: endDate || undefined, page: 1 }));
  };

  return (
    <div>
      {/* Hero Search */}
      <div className="bg-gradient-to-r from-blue-600 to-indigo-700 rounded-2xl p-8 mb-8 text-white">
        <h1 className="text-3xl font-bold mb-2">Find your perfect car</h1>
        <p className="text-blue-100 mb-6">Browse from hundreds of cars listed by owners near you</p>

        <div className="bg-white rounded-xl p-4 flex flex-wrap gap-3 items-end">
          <div className="flex-1 min-w-48">
            <label className="block text-xs font-medium text-gray-500 mb-1">City</label>
            <div className="relative">
              <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input value={city} onChange={e => setCity(e.target.value)}
                placeholder="e.g. Cluj-Napoca" className="w-full pl-9 pr-3 py-2 border border-gray-200 rounded-lg text-sm text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Pickup Date</label>
            <input type="date" value={startDate} onChange={e => setStartDate(e.target.value)}
              className="border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Return Date</label>
            <input type="date" value={endDate} onChange={e => setEndDate(e.target.value)}
              className="border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <button onClick={handleSearch}
            className="bg-blue-600 text-white px-6 py-2 rounded-lg font-medium hover:bg-blue-700 flex items-center gap-2">
            <Search className="w-4 h-4" /> Search
          </button>
        </div>
      </div>

      {/* Results */}
      {isLoading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="bg-white rounded-xl shadow-sm animate-pulse">
              <div className="h-48 bg-gray-200 rounded-t-xl" />
              <div className="p-4 space-y-2">
                <div className="h-4 bg-gray-200 rounded w-3/4" />
                <div className="h-4 bg-gray-200 rounded w-1/2" />
              </div>
            </div>
          ))}
        </div>
      ) : (
        <>
          <p className="text-sm text-gray-500 mb-4">{data?.totalCount ?? 0} cars found</p>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {data?.items.map(car => (
              <Link key={car.id} to={`/cars/${car.id}`}
                className="bg-white rounded-xl shadow-sm hover:shadow-md transition-shadow overflow-hidden group">
                <div className="h-48 bg-gray-100 relative overflow-hidden">
                  {car.photos[0] ? (
                    <img src={car.photos[0]} alt={`${car.make} ${car.model}`} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-gray-400">
                      <span className="text-4xl">🚗</span>
                    </div>
                  )}
                  <div className="absolute top-3 right-3 bg-white/90 backdrop-blur-sm rounded-lg px-2 py-1 text-sm font-bold text-blue-600">
                    ${car.pricePerDay}/day
                  </div>
                </div>
                <div className="p-4">
                  <h3 className="font-semibold text-gray-900">{car.year} {car.make} {car.model}</h3>
                  <div className="flex items-center gap-1 text-gray-500 text-sm mt-1">
                    <MapPin className="w-3 h-3" /> {car.city}
                  </div>
                  <div className="flex items-center gap-3 mt-3 text-xs text-gray-500">
                    <span className="flex items-center gap-1"><Users className="w-3 h-3" /> {car.seats} seats</span>
                    <span className="flex items-center gap-1"><Settings className="w-3 h-3" /> {car.transmission}</span>
                    <span className="flex items-center gap-1"><Fuel className="w-3 h-3" /> {car.fuelType}</span>
                  </div>
                  {car.totalRatings > 0 && (
                    <div className="flex items-center gap-1 mt-2 text-sm">
                      <Star className="w-4 h-4 text-yellow-400 fill-yellow-400" />
                      <span className="font-medium">{car.averageRating.toFixed(1)}</span>
                      <span className="text-gray-400">({car.totalRatings})</span>
                    </div>
                  )}
                </div>
              </Link>
            ))}
          </div>

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="flex justify-center gap-2 mt-8">
              <button disabled={!data.hasPreviousPage}
                onClick={() => setParams(p => ({ ...p, page: (p.page ?? 1) - 1 }))}
                className="px-4 py-2 border rounded-lg text-sm disabled:opacity-50 hover:bg-gray-50">Previous</button>
              <span className="px-4 py-2 text-sm text-gray-600">Page {data.page} of {data.totalPages}</span>
              <button disabled={!data.hasNextPage}
                onClick={() => setParams(p => ({ ...p, page: (p.page ?? 1) + 1 }))}
                className="px-4 py-2 border rounded-lg text-sm disabled:opacity-50 hover:bg-gray-50">Next</button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
