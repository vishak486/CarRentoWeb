# CarRentoWeb

A full-stack car rental web application built with **ASP.NET Core MVC (.NET 10)** and **SQL Server**. Customers can browse cars by brand, book them for specific dates, and pay online through Razorpay. Admins manage brands, cars, customers and orders from a dashboard.

## Features

### Customer
- Register / log in with ASP.NET Core Identity
- Complete profile (full name, address, driving license, phone) before booking
- Browse active brands, then available cars by brand, then car details with an image carousel
- Pick rental start and end dates and add to cart
- Double-booking protection: a car cannot be booked for dates that overlap an existing cart item or a paid order
- Checkout with **Razorpay** (server-side signature verification)
- Order history, order details and **PDF receipt download**
- Contact form

### Admin
- Dashboard with revenue, order, customer and car stats, plus Chart.js charts (revenue for the last 6 months, cars by status, top 5 most booked cars)
- Manage Brands: create with logo upload, activate / deactivate (soft delete)
- Manage Cars: create with 3 images, edit, update status (Available / Rented / Maintenance)
- Manage Customers: lock / unlock accounts
- Manage Orders: view paid and cancelled orders, mark car as returned, cancel an order (payment marked as refunded), download PDF receipt
- Manage Contacts: view and delete customer messages

## Performance: In-Memory Caching

Read-heavy pages use `IMemoryCache` to reduce database load:

| Data | Strategy |
|---|---|
| Customer brand list | Cached for 10 minutes, cleared when an admin creates or toggles a brand |
| Cars by brand (per-brand cache key) | Cached for 10 minutes, cleared when an admin creates, edits or changes the status of a car |
| Admin dashboard stats | Cached for 2 minutes (short expiry, no manual invalidation) |

Cart, orders, payments and the booking-overlap check are intentionally **not** cached, so availability is always accurate.

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC + Razor Pages (.NET 10) |
| Database | SQL Server with Entity Framework Core (code-first migrations) |
| Authentication | ASP.NET Core Identity with role-based authorization (Admin / Customer) |
| Payments | Razorpay |
| PDF generation | QuestPDF |
| Frontend | Bootstrap 5, jQuery, DataTables, Chart.js |
| Caching | `IMemoryCache` |

## Project Structure

```
CarRentoWeb/
├── Areas/
│   ├── Admin/       # Controllers + Views for dashboard, brands, cars, users, orders, contacts
│   ├── Customer/    # Controllers + Views (browse, cart) and Razor Pages (checkout, orders)
│   └── Identity/    # Scaffolded Identity UI (login, register, profile)
├── Data/            # ApplicationDbContext
├── Migrations/      # EF Core migrations
├── Models/          # Entities and view models
├── Services/        # Order, Razorpay and PDF services
├── Utility/         # Constants (roles)
├── ViewComponents/  # Cart count badge
└── wwwroot/         # Static files and uploaded images
```

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server Express
- A [Razorpay](https://razorpay.com/) account (test keys are enough)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/vishak486/carrentoweb.git
   cd carrentoweb/CarRentoWeb
   ```

2. **Configure the database.** Update the `DefaultConnection` string in `appsettings.json` to point to your SQL Server instance.

3. **Apply migrations**
   ```bash
   dotnet ef database update
   ```

4. **Add your Razorpay keys using User Secrets** (never commit real keys)
   ```bash
   dotnet user-secrets set "Razorpay:KeyId" "your_key_id"
   dotnet user-secrets set "Razorpay:KeySecret" "your_key_secret"
   ```

5. **Run the app**
   ```bash
   dotnet run
   ```

### Creating the first Admin

The `Admin` and `Customer` roles are created automatically the first time the Register page is opened. Public registration only assigns the `Customer` role, so to create an admin:

Admin: admin@gmail.com  Password: Admin123

1. Register a normal account.
2. In the database, change that user's role to `Admin` in the `AspNetUserRoles` table.

Admins are redirected to the dashboard after login.

## Notes

- Uploaded brand logos and car images are stored in `wwwroot/images/`.
- Razorpay is configured for INR.
- Email sending is a no-op by default (`NoOpEmailSender`), so email confirmation links are not actually sent.

## Screenshots

_Add screenshots here (home page, car details, cart, checkout, admin dashboard)._

## Author

**Vishak**
GitHub: [vishak486](https://github.com/vishak486)
