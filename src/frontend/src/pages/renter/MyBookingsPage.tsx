import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { bookingsApi } from '@/services/apiServices';
import type { Booking } from '@/types';

const statusColors: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-800',
  Confirmed: 'bg-green-100 text-green-800',
  Active: 'bg-blue-100 text-blue-800',
  Completed: 'bg-gray-100 text-gray-800',
  CancelledByRenter: 'bg-red-100 text-red-800',
  CancelledByOwner: 'bg-red-100 text-red-800',
  Disputed: 'bg-orange-100 text-orange-800',
};

function BookingCard({ booking, onCancel }: { booking: Booking; onCancel: (id: string) => void }) {
  return (
    <div className="bg-white border border-gray-200 rounded-xl p-5 shadow-sm">
      <div className="flex justify-between items-start mb-3">
        <div>
          <p className="font-semibold text-gray-900">Car: {booking.carId.slice(0, 8)}...</p>
          <p className="text-sm text-gray-500">Booked {format(new Date(booking.createdAt), 'MMM d, yyyy')}</p>
        </div>
        <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColors[booking.status] ?? 'bg-gray-100'}`}>
          {booking.status}
        </span>
      </div>

      <div className="grid grid-cols-3 gap-3 text-sm mb-4">
        <div><p className="text-gray-400 text-xs">Pickup</p><p className="font-medium">{format(new Date(booking.startDate), 'MMM d, yyyy')}</p></div>
        <div><p className="text-gray-400 text-xs">Return</p><p className="font-medium">{format(new Date(booking.endDate), 'MMM d, yyyy')}</p></div>
        <div><p className="text-gray-400 text-xs">Total</p><p className="font-medium text-blue-600">${booking.totalPrice}</p></div>
      </div>

      {(booking.status === 'Pending' || booking.status === 'Confirmed') && (
        <button onClick={() => onCancel(booking.id)}
          className="text-sm text-red-600 hover:text-red-700 font-medium">
          Cancel Booking
        </button>
      )}
    </div>
  );
}

export default function MyBookingsPage() {
  const qc = useQueryClient();
  const { data: bookings, isLoading } = useQuery({
    queryKey: ['bookings', 'my'],
    queryFn: bookingsApi.getMyBookings,
  });

  const cancelMutation = useMutation({
    mutationFn: (id: string) => bookingsApi.cancel(id, 'Cancelled by renter'),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['bookings', 'my'] }),
  });

  if (isLoading) return <div className="animate-pulse space-y-4">{Array.from({ length: 3 }).map((_, i) => <div key={i} className="h-40 bg-gray-200 rounded-xl" />)}</div>;

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">My Bookings</h1>
      {!bookings?.length ? (
        <div className="text-center py-16 text-gray-400">
          <p className="text-4xl mb-3">🚗</p>
          <p className="font-medium">No bookings yet</p>
          <p className="text-sm mt-1">Find a car to get started</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {bookings.map(b => (
            <BookingCard key={b.id} booking={b} onCancel={(id) => cancelMutation.mutate(id)} />
          ))}
        </div>
      )}
    </div>
  );
}
