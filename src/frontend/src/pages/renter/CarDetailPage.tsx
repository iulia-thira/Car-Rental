import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { MapPin, Star, Users, Fuel, Settings, Shield, ChevronLeft } from 'lucide-react';
import { listingsApi, bookingsApi, reviewsApi } from '@/services/apiServices';
import { useAuthStore } from '@/store/authStore';
import { differenceInDays, format } from 'date-fns';

export default function CarDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuthStore();

  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [specialRequests, setSpecialRequests] = useState('');
  const [photoIdx, setPhotoIdx] = useState(0);

  const { data: car, isLoading } = useQuery({
    queryKey: ['listing', id],
    queryFn: () => listingsApi.getById(id!),
    enabled: !!id,
  });

  const { data: reviews } = useQuery({
    queryKey: ['reviews', 'car', id],
    queryFn: () => reviewsApi.getCarReviews(id!),
    enabled: !!id,
  });

  const bookMutation = useMutation({
    mutationFn: bookingsApi.create,
    onSuccess: () => navigate('/my-bookings'),
  });

  const days = startDate && endDate ? differenceInDays(new Date(endDate), new Date(startDate)) : 0;
  const totalPrice = days > 0 && car ? days * car.pricePerDay : 0;

  const handleBook = () => {
    if (!isAuthenticated) { navigate('/login'); return; }
    if (!car || days <= 0) return;
    bookMutation.mutate({
      carId: car.id, ownerId: car.ownerId,
      startDate, endDate,
      pricePerDay: car.pricePerDay, deposit: car.deposit,
      specialRequests,
      renterEmail: user!.email, ownerEmail: '',
    });
  };

  if (isLoading) return <div className="animate-pulse h-96 bg-gray-200 rounded-xl" />;
  if (!car) return <div>Car not found.</div>;

  return (
    <div>
      <button onClick={() => navigate(-1)} className="flex items-center gap-1 text-gray-600 hover:text-blue-600 mb-6 text-sm">
        <ChevronLeft className="w-4 h-4" /> Back to search
      </button>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left: Car details */}
        <div className="lg:col-span-2">
          {/* Photos */}
          <div className="rounded-2xl overflow-hidden mb-6 bg-gray-100 h-80 relative">
            {car.photos.length > 0 ? (
              <>
                <img src={car.photos[photoIdx]} alt={`${car.make} ${car.model}`} className="w-full h-full object-cover" />
                {car.photos.length > 1 && (
                  <div className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2">
                    {car.photos.map((_, i) => (
                      <button key={i} onClick={() => setPhotoIdx(i)}
                        className={`w-2 h-2 rounded-full ${i === photoIdx ? 'bg-white' : 'bg-white/50'}`} />
                    ))}
                  </div>
                )}
              </>
            ) : (
              <div className="w-full h-full flex items-center justify-center text-gray-400 text-6xl">🚗</div>
            )}
          </div>

          <h1 className="text-2xl font-bold text-gray-900">{car.year} {car.make} {car.model}</h1>
          <div className="flex items-center gap-2 text-gray-500 mt-1 mb-4">
            <MapPin className="w-4 h-4" />
            <span>{car.address}, {car.city}, {car.country}</span>
          </div>

          {car.totalRatings > 0 && (
            <div className="flex items-center gap-1 mb-4">
              <Star className="w-5 h-5 text-yellow-400 fill-yellow-400" />
              <span className="font-semibold">{car.averageRating.toFixed(1)}</span>
              <span className="text-gray-400 text-sm">({car.totalRatings} reviews)</span>
            </div>
          )}

          <div className="grid grid-cols-4 gap-4 bg-gray-50 rounded-xl p-4 mb-6">
            <div className="text-center"><Users className="w-5 h-5 mx-auto text-blue-600 mb-1" /><p className="text-xs text-gray-500">Seats</p><p className="font-semibold">{car.seats}</p></div>
            <div className="text-center"><Settings className="w-5 h-5 mx-auto text-blue-600 mb-1" /><p className="text-xs text-gray-500">Transmission</p><p className="font-semibold">{car.transmission}</p></div>
            <div className="text-center"><Fuel className="w-5 h-5 mx-auto text-blue-600 mb-1" /><p className="text-xs text-gray-500">Fuel</p><p className="font-semibold">{car.fuelType}</p></div>
            <div className="text-center"><Shield className="w-5 h-5 mx-auto text-blue-600 mb-1" /><p className="text-xs text-gray-500">Deposit</p><p className="font-semibold">${car.deposit}</p></div>
          </div>

          <p className="text-gray-700 mb-6">{car.description}</p>

          {car.features.length > 0 && (
            <div className="mb-6">
              <h3 className="font-semibold text-gray-900 mb-3">Features</h3>
              <div className="flex flex-wrap gap-2">
                {car.features.map(f => (
                  <span key={f} className="bg-blue-50 text-blue-700 px-3 py-1 rounded-full text-sm">{f}</span>
                ))}
              </div>
            </div>
          )}

          {reviews && reviews.length > 0 && (
            <div>
              <h3 className="font-semibold text-gray-900 mb-3">Reviews</h3>
              <div className="space-y-4">
                {reviews.map(r => (
                  <div key={r.id} className="border border-gray-200 rounded-xl p-4">
                    <div className="flex items-center gap-2 mb-2">
                      {Array.from({ length: 5 }).map((_, i) => (
                        <Star key={i} className={`w-4 h-4 ${i < r.rating ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'}`} />
                      ))}
                      <span className="text-sm text-gray-400">{format(new Date(r.createdAt), 'MMM yyyy')}</span>
                    </div>
                    <p className="text-gray-700 text-sm">{r.comment}</p>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Right: Booking card */}
        <div>
          <div className="bg-white border border-gray-200 rounded-2xl p-6 sticky top-24 shadow-sm">
            <div className="text-2xl font-bold text-gray-900 mb-1">
              ${car.pricePerDay}<span className="text-base font-normal text-gray-500">/day</span>
            </div>

            <div className="space-y-3 my-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Pickup Date</label>
                <input type="date" value={startDate} onChange={e => setStartDate(e.target.value)}
                  min={new Date().toISOString().split('T')[0]}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Return Date</label>
                <input type="date" value={endDate} onChange={e => setEndDate(e.target.value)}
                  min={startDate || new Date().toISOString().split('T')[0]}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Special Requests</label>
                <textarea value={specialRequests} onChange={e => setSpecialRequests(e.target.value)}
                  rows={2} placeholder="Any special requests..."
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none" />
              </div>
            </div>

            {days > 0 && (
              <div className="border-t border-gray-100 pt-4 mb-4 space-y-2 text-sm">
                <div className="flex justify-between text-gray-600">
                  <span>${car.pricePerDay} × {days} days</span>
                  <span>${car.pricePerDay * days}</span>
                </div>
                <div className="flex justify-between text-gray-600">
                  <span>Deposit (refundable)</span>
                  <span>${car.deposit}</span>
                </div>
                <div className="flex justify-between font-semibold text-gray-900 border-t pt-2">
                  <span>Total</span>
                  <span>${totalPrice + car.deposit}</span>
                </div>
              </div>
            )}

            <button onClick={handleBook}
              disabled={days <= 0 || bookMutation.isPending}
              className="w-full bg-blue-600 text-white py-3 rounded-xl font-semibold hover:bg-blue-700 disabled:opacity-50 transition-colors">
              {bookMutation.isPending ? 'Booking...' : isAuthenticated ? 'Book Now' : 'Sign in to Book'}
            </button>

            {bookMutation.isError && (
              <p className="text-red-600 text-xs mt-2 text-center">Booking failed. Dates may be unavailable.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
