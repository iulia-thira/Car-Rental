# 🚗 DriveShare — Car Rental Platform

A full-stack car rental marketplace built with **.NET 8 microservices** and **React 18**, deployed on **Azure**.

## Architecture

```
Frontend (React 18 + TypeScript + Vite)
    │
    ▼ (REST API via Nginx proxy)
┌─────────────────────────────────────────────────────────┐
│               Backend Microservices (.NET 8)            │
├──────────────┬────────────────┬────────────────────────┤
│ UserService  │ ListingService │  BookingService        │
│ (Azure SQL)  │ (Cosmos DB)    │  (Azure SQL)           │
├──────────────┼────────────────┼────────────────────────┤
│PaymentService│NotificationSvc │  ReviewService         │
│ (Stripe)     │ (SendGrid)     │  (Azure SQL)           │
└──────────────┴────────────────┴────────────────────────┘
                        │
              Azure Service Bus (async events)
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18, TypeScript, Vite, TanStack Query, Zustand, Tailwind CSS |
| Backend | .NET 8 ASP.NET Core, Entity Framework Core 8, MediatR |
| Auth | JWT Tokens (Azure AD B2C in production) |
| Databases | Azure SQL (Users/Bookings/Payments/Reviews), Cosmos DB (Listings) |
| Messaging | Azure Service Bus |
| Storage | Azure Blob Storage (car photos) |
| Payments | Stripe + Stripe Connect |
| Email | SendGrid |
| Infra | Azure Container Apps, Terraform |
| CI/CD | GitHub Actions |

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)
- [Docker Desktop](https://docker.com/products/docker-desktop)

### 1. Clone and set up

```bash
git clone https://github.com/iulia-thira/Car-Rental.git
cd Car-Rental
```

### 2. Run with Docker Compose (recommended)

```bash
docker-compose up -d
```

This starts:
- All 6 .NET microservices
- SQL Server (port 1433)
- Cosmos DB Emulator (port 8081)
- Azure Blob Storage Emulator (Azurite, port 10000)
- Frontend (port 3000)

Open: http://localhost:3000

### 3. Run services individually (development)

**Backend services:**
```bash
# UserService
cd src/services/UserService
dotnet run  # runs on http://localhost:5001

# ListingService
cd src/services/ListingService
dotnet run  # runs on http://localhost:5002

# BookingService
cd src/services/BookingService
dotnet run  # runs on http://localhost:5003

# PaymentService
cd src/services/PaymentService
dotnet run  # runs on http://localhost:5004

# NotificationService
cd src/services/NotificationService
dotnet run  # runs on http://localhost:5005

# ReviewService
cd src/services/ReviewService
dotnet run  # runs on http://localhost:5006
```

**Frontend:**
```bash
cd src/frontend
npm install
npm run dev  # runs on http://localhost:3000
```

## Configuration

Each service has an `appsettings.json`. For production, set these environment variables:

| Service | Key Variables |
|---------|--------------|
| All | `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience` |
| User/Booking/Payment/Review | `ConnectionStrings__DefaultConnection` |
| Listing | `Cosmos__ConnectionString`, `Azure__StorageConnectionString` |
| Booking/Notification | `ServiceBus__ConnectionString` |
| Payment | `Stripe__SecretKey` |
| Notification | `SendGrid__ApiKey` |

## API Endpoints

### User Service (port 5001)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/users/register | Register new user |
| POST | /api/users/login | Login |
| GET | /api/users/me | Get current user |
| PUT | /api/users/me | Update profile |

### Listing Service (port 5002)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/listings/search | Search listings |
| GET | /api/listings/{id} | Get listing by ID |
| POST | /api/listings | Create listing (Owner) |
| PUT | /api/listings/{id} | Update listing (Owner) |
| DELETE | /api/listings/{id} | Delete listing (Owner) |
| POST | /api/listings/{id}/photos | Upload photo |

### Booking Service (port 5003)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/bookings | Create booking |
| GET | /api/bookings/my | Get renter's bookings |
| GET | /api/bookings/owner | Get owner's bookings |
| POST | /api/bookings/{id}/confirm | Owner confirms |
| POST | /api/bookings/{id}/cancel | Cancel booking |
| POST | /api/bookings/{id}/confirm-pickup | Confirm pickup |
| POST | /api/bookings/{id}/confirm-return | Confirm return |

### Payment Service (port 5004)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/payments/intent | Create Stripe payment intent |
| POST | /api/payments/confirm | Confirm payment |
| POST | /api/payments/refund | Refund payment |

### Review Service (port 5006)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/reviews | Create review |
| GET | /api/reviews/car/{carId} | Get car reviews |
| GET | /api/reviews/summary/car/{carId} | Get rating summary |

## Deploy to Azure

1. **Set up Terraform:**
```bash
cd infra/terraform
terraform init
terraform plan -var="sql_admin_password=YourSecurePassword!"
terraform apply
```

2. **Configure GitHub Secrets:**
   - `AZURE_CREDENTIALS` — service principal JSON
   - `AZURE_RESOURCE_GROUP` — resource group name

3. **Push to main** — GitHub Actions will build, push Docker images, and deploy.

## Project Structure

```
Car-Rental/
├── CarRental.sln
├── docker-compose.yml
├── .github/workflows/ci-cd.yml
├── infra/terraform/main.tf
└── src/
    ├── frontend/                    # React 18 app
    │   ├── src/
    │   │   ├── pages/               # Route pages
    │   │   ├── components/          # Shared components
    │   │   ├── services/            # API clients
    │   │   ├── store/               # Zustand stores
    │   │   └── types/               # TypeScript types
    └── services/
        ├── SharedKernel/            # Shared models & events
        ├── UserService/             # Auth & user management
        ├── ListingService/          # Car listings (Cosmos DB)
        ├── BookingService/          # Reservations & state machine
        ├── PaymentService/          # Stripe integration
        ├── NotificationService/     # Email via SendGrid
        └── ReviewService/           # Ratings & reviews
```

## User Flows

### Renter
1. Register as **Renter** → Browse cars → Select dates → Book
2. Wait for Owner confirmation → Pay via Stripe → Pickup
3. Return car → Leave review

### Owner
1. Register as **Owner** → List your car → Set availability
2. Review incoming bookings → Confirm/Decline
3. Confirm pickup → Confirm return → Receive payout

## License

MIT
