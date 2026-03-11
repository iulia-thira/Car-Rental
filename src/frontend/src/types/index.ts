// ── Auth & Users ──────────────────────────────────────────────
export type UserRole = 'Renter' | 'Owner' | 'Admin';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phoneNumber?: string;
  role: UserRole;
  isVerified: boolean;
  profileImageUrl?: string;
  averageRating: number;
  totalRatings: number;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
  expiresAt: string;
}

export interface LoginRequest { email: string; password: string; }

export interface RegisterRequest {
  email: string; password: string;
  firstName: string; lastName: string;
  phoneNumber?: string; role: UserRole;
}

// ── Listings ──────────────────────────────────────────────────
export type TransmissionType = 'Manual' | 'Automatic';
export type FuelType = 'Petrol' | 'Diesel' | 'Electric' | 'Hybrid';
export type ListingStatus = 'Draft' | 'PendingReview' | 'Active' | 'Inactive' | 'Suspended';

export interface CarListing {
  id: string;
  ownerId: string;
  make: string;
  model: string;
  year: number;
  description: string;
  pricePerDay: number;
  deposit: number;
  latitude: number;
  longitude: number;
  address: string;
  city: string;
  country: string;
  photos: string[];
  features: string[];
  transmission: TransmissionType;
  fuelType: FuelType;
  seats: number;
  mileageLimit?: number;
  status: ListingStatus;
  averageRating: number;
  totalRatings: number;
  createdAt: string;
}

export interface SearchListingsParams {
  city?: string; latitude?: number; longitude?: number; radiusKm?: number;
  startDate?: string; endDate?: string;
  minPrice?: number; maxPrice?: number;
  transmission?: TransmissionType; fuelType?: FuelType; minSeats?: number;
  page?: number; pageSize?: number;
}

export interface CreateListingRequest {
  make: string; model: string; year: number; description: string;
  pricePerDay: number; deposit: number;
  latitude: number; longitude: number;
  address: string; city: string; country: string;
  features: string[];
  transmission: TransmissionType; fuelType: FuelType;
  seats: number; mileageLimit?: number;
}

// ── Bookings ──────────────────────────────────────────────────
export type BookingStatus =
  | 'Pending' | 'Confirmed' | 'Active' | 'Completed'
  | 'CancelledByRenter' | 'CancelledByOwner' | 'Disputed';

export interface Booking {
  id: string;
  carId: string;
  renterId: string;
  ownerId: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  pricePerDay: number;
  totalPrice: number;
  deposit: number;
  status: BookingStatus;
  specialRequests?: string;
  cancellationReason?: string;
  pickupConfirmedAt?: string;
  returnConfirmedAt?: string;
  createdAt: string;
}

export interface CreateBookingRequest {
  carId: string; ownerId: string;
  startDate: string; endDate: string;
  pricePerDay: number; deposit: number;
  specialRequests?: string;
  renterEmail: string; ownerEmail: string;
}

// ── Payments ──────────────────────────────────────────────────
export interface PaymentIntentResponse { clientSecret: string; paymentIntentId: string; amount: number; }
export interface Payment { id: string; bookingId: string; amount: number; deposit: number; platformFee: number; ownerPayout: number; status: string; stripePaymentIntentId?: string; createdAt: string; }

// ── Reviews ──────────────────────────────────────────────────
export type ReviewTargetType = 'Car' | 'User';
export interface Review { id: string; bookingId: string; authorId: string; targetId: string; targetType: ReviewTargetType; rating: number; comment: string; createdAt: string; }
export interface RatingSummary { targetId: string; averageRating: number; totalReviews: number; }

// ── API ───────────────────────────────────────────────────────
export interface ApiResponse<T> { success: boolean; data: T; message?: string; errors: string[]; }
export interface PagedResult<T> { items: T[]; totalCount: number; page: number; pageSize: number; totalPages: number; hasNextPage: boolean; hasPreviousPage: boolean; }
