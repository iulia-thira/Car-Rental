import api from './api';
import type {
  LoginRequest, RegisterRequest, AuthResponse,
  CarListing, CreateListingRequest, SearchListingsParams,
  Booking, CreateBookingRequest,
  PaymentIntentResponse, Payment,
  Review, RatingSummary, ReviewTargetType,
  ApiResponse, PagedResult, User
} from '@/types';

// ── Auth ──────────────────────────────────────────────────────
export const authApi = {
  login: (data: LoginRequest) =>
    api.post<ApiResponse<AuthResponse>>('/users/login', data).then(r => r.data.data),
  register: (data: RegisterRequest) =>
    api.post<ApiResponse<AuthResponse>>('/users/register', data).then(r => r.data.data),
  getMe: () =>
    api.get<ApiResponse<User>>('/users/me').then(r => r.data.data),
  updateProfile: (data: Partial<User>) =>
    api.put<ApiResponse<User>>('/users/me', data).then(r => r.data.data),
};

// ── Listings ──────────────────────────────────────────────────
export const listingsApi = {
  search: (params: SearchListingsParams) =>
    api.get<ApiResponse<PagedResult<CarListing>>>('/listings/search', { params }).then(r => r.data.data),
  getById: (id: string) =>
    api.get<ApiResponse<CarListing>>(`/listings/${id}`).then(r => r.data.data),
  getMyListings: () =>
    api.get<ApiResponse<CarListing[]>>('/listings/my').then(r => r.data.data),
  create: (data: CreateListingRequest) =>
    api.post<ApiResponse<CarListing>>('/listings', data).then(r => r.data.data),
  update: (id: string, data: Partial<CreateListingRequest>) =>
    api.put<ApiResponse<CarListing>>(`/listings/${id}`, data).then(r => r.data.data),
  delete: (id: string) =>
    api.delete(`/listings/${id}`),
  uploadPhoto: (id: string, file: File) => {
    const formData = new FormData();
    formData.append('photo', file);
    return api.post<ApiResponse<string>>(`/listings/${id}/photos`, formData).then(r => r.data.data);
  },
  blockDates: (id: string, start: string, end: string, reason?: string) =>
    api.post(`/listings/${id}/block-dates`, { start, end, reason }),
};

// ── Bookings ──────────────────────────────────────────────────
export const bookingsApi = {
  create: (data: CreateBookingRequest) =>
    api.post<ApiResponse<Booking>>('/bookings', data).then(r => r.data.data),
  getById: (id: string) =>
    api.get<ApiResponse<Booking>>(`/bookings/${id}`).then(r => r.data.data),
  getMyBookings: () =>
    api.get<ApiResponse<Booking[]>>('/bookings/my').then(r => r.data.data),
  getOwnerBookings: () =>
    api.get<ApiResponse<Booking[]>>('/bookings/owner').then(r => r.data.data),
  confirm: (id: string) =>
    api.post<ApiResponse<Booking>>(`/bookings/${id}/confirm`).then(r => r.data.data),
  cancel: (id: string, reason: string) =>
    api.post<ApiResponse<Booking>>(`/bookings/${id}/cancel`, { reason }).then(r => r.data.data),
  confirmPickup: (id: string) =>
    api.post<ApiResponse<Booking>>(`/bookings/${id}/confirm-pickup`).then(r => r.data.data),
  confirmReturn: (id: string) =>
    api.post<ApiResponse<Booking>>(`/bookings/${id}/confirm-return`).then(r => r.data.data),
};

// ── Payments ──────────────────────────────────────────────────
export const paymentsApi = {
  createIntent: (bookingId: string, ownerId: string, amount: number, deposit: number) =>
    api.post<ApiResponse<PaymentIntentResponse>>('/payments/intent', { bookingId, ownerId, amount, deposit })
      .then(r => r.data.data),
  confirmPayment: (paymentIntentId: string) =>
    api.post<ApiResponse<Payment>>('/payments/confirm', { paymentIntentId }).then(r => r.data.data),
  refund: (bookingId: string, amount?: number, reason?: string) =>
    api.post<ApiResponse<Payment>>('/payments/refund', { bookingId, amount, reason }).then(r => r.data.data),
  getByBooking: (bookingId: string) =>
    api.get<ApiResponse<Payment>>(`/payments/booking/${bookingId}`).then(r => r.data.data),
};

// ── Reviews ──────────────────────────────────────────────────
export const reviewsApi = {
  create: (data: { bookingId: string; targetId: string; targetType: ReviewTargetType; rating: number; comment: string }) =>
    api.post<ApiResponse<Review>>('/reviews', data).then(r => r.data.data),
  getCarReviews: (carId: string) =>
    api.get<ApiResponse<Review[]>>(`/reviews/car/${carId}`).then(r => r.data.data),
  getCarRatingSummary: (carId: string) =>
    api.get<ApiResponse<RatingSummary>>(`/reviews/summary/car/${carId}`).then(r => r.data.data),
};
