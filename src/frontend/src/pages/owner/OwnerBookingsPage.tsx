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
};

export default function OwnerBookingsPage() {
  const qc = useQueryClient();
  const { data: bookings, isLoading } = useQuery({
    queryKey: ['bookings', 'owner'],
    queryFn: bookingsApi.getOwnerBookings,
  });

  const confirmMutation = useMutation({
    mutationFn: bookingsApi.confirm,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['bookings', 'owner'] }),
  });

  const pickupMutation = useMutation({
    mutationFn: bookingsApi.confirmPickup,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['bookings', 'owner'] }),
  });

  const returnMutation = useMutation({
    mutationFn: bookingsApi.confirmReturn,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['bookings', 'owner'] }),
  });

  const cancelMutation = useMutation({
    mutationFn: (id: string) => bookingsApi.cancel(id, 'Cancelled by owner'),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['bookings', 'owner'] }),
  });

  if (isLoading) return <div className="animate-pulse space-y-4">{Array.from({ length: 3 }).map((_, i) => <div key={i} className="h-40 bg-gray-200 rounded-xl" />)}</div>;

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Incoming Bookings</h1>
      {!bookings?.length ? (
        <div className="text-center py-16 text-gray-400">
          <p className="text-4xl mb-3">📅</p>
          <p className="font-medium">No bookings yet</p>
        </div>
      ) : (
        <div className="space-y-4">
          {bookings.map((b: Booking) => (
            <div key={b.id} className="bg-white border border-gray-200 rounded-xl p-5 shadow-sm">
              <div className="flex justify-between items-start mb-3">
                <div>
                  <p className="font-semibold text-gray-900">Booking #{b.id.slice(0, 8)}</p>
                  <p className="text-sm text-gray-500">{format(new Date(b.startDate), 'MMM d')} – {format(new Date(b.endDate), 'MMM d, yyyy')}</p>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-blue-600 font-semibold">${b.totalPrice}</span>
                  <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColors[b.status] ?? 'bg-gray-100'}`}>{b.status}</span>
                </div>
              </div>

              <div className="flex flex-wrap gap-2">
                {b.status === 'Pending' && (
                  <>
                    <button onClick={() => confirmMutation.mutate(b.id)}
                      className="text-sm bg-green-600 text-white px-3 py-1.5 rounded-lg hover:bg-green-700">
                      Confirm
                    </button>
                    <button onClick={() => cancelMutation.mutate(b.id)}
                      className="text-sm border border-red-200 text-red-600 px-3 py-1.5 rounded-lg hover:bg-red-50">
                      Decline
                    </button>
                  </>
                )}
                {b.status === 'Confirmed' && (
                  <button onClick={() => pickupMutation.mutate(b.id)}
                    className="text-sm bg-blue-600 text-white px-3 py-1.5 rounded-lg hover:bg-blue-700">
                    Confirm Pickup
                  </button>
                )}
                {b.status === 'Active' && (
                  <button onClick={() => returnMutation.mutate(b.id)}
                    className="text-sm bg-purple-600 text-white px-3 py-1.5 rounded-lg hover:bg-purple-700">
                    Confirm Return
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
