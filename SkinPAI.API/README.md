# SkinPAI API

A comprehensive .NET 9 Web API backend for the SkinPAI skin analysis mobile application.

## Features

- **User Authentication** - JWT-based auth with refresh tokens
- **Skin Analysis** - AI-powered skin scanning with detailed analysis
- **Product Recommendations** - Personalized product suggestions based on skin analysis
- **Skincare Routines** - Create, manage, and track daily skincare routines
- **Community Platform** - Social features with posts, likes, comments
- **Creator Stations** - Influencer profiles with followers and content
- **Subscription Management** - Member and Pro subscription tiers
- **Wallet System** - In-app wallet for payments
- **Achievements** - Gamification with unlockable achievements
- **Notifications** - Push notification system
- **Chat** - Direct messaging between users

## Tech Stack

- **.NET 9** - Latest .NET framework
- **Entity Framework Core 9** - ORM with migrations
- **SQL Server LocalDB** - Database
- **JWT Authentication** - Secure token-based auth
- **BCrypt** - Password hashing
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation

## Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server LocalDB
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd SkinPAI.API
```

2. Create the database:
```bash
cd SkinPAI.API
dotnet ef database update
```

3. Run the API:
```bash
dotnet run
```

4. Open Swagger UI:
```
http://localhost:5000/swagger
```

### Configuration

The API uses `appsettings.json` for configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(LocalDb)\\MSSQLLocalDB;Database=SkinPAI_Dev;..."
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "SkinPAI.API",
    "Audience": "SkinPAI.Mobile"
  }
}
```

## Project Structure

```
SkinPAI.API/
├── Controllers/           # API controllers
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── ScansController.cs
│   ├── ProductsController.cs
│   ├── CommunityController.cs
│   ├── RoutinesController.cs
│   ├── SubscriptionsController.cs
│   ├── NotificationsController.cs
│   └── ChatController.cs
├── Data/
│   └── SkinPAIDbContext.cs  # EF Core DbContext
├── Models/
│   ├── Entities/            # Database entities (35+ models)
│   └── DTOs/                # Data transfer objects
├── Repositories/            # Repository pattern
│   ├── IRepository.cs
│   ├── Repository.cs
│   ├── IUnitOfWork.cs
│   └── UnitOfWork.cs
├── Services/                # Business logic
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   ├── IUserService.cs
│   ├── UserService.cs
│   └── ... (9 services total)
├── Documentation/           # API documentation
│   ├── API_DOCUMENTATION.md
│   └── FRONTEND_INTEGRATION.md
├── Uploads/                 # File storage
│   ├── scans/
│   ├── profiles/
│   ├── posts/
│   └── products/
├── Program.cs               # Application entry point
├── appsettings.json         # Configuration
└── SkinPAI.API.csproj       # Project file
```

## API Endpoints

| Category | Endpoint | Description |
|----------|----------|-------------|
| **Auth** | POST /api/Auth/register | Register new user |
| | POST /api/Auth/login | Login user |
| | POST /api/Auth/guest | Guest login |
| | POST /api/Auth/refresh | Refresh token |
| **Users** | GET /api/Users/me | Get current user |
| | PUT /api/Users/me | Update profile |
| | GET /api/Users/dashboard/member | Member dashboard |
| | GET /api/Users/dashboard/pro | Pro dashboard |
| **Scans** | POST /api/Scans | Create scan |
| | GET /api/Scans | Get user scans |
| | GET /api/Scans/{id} | Get scan by ID |
| | GET /api/Scans/daily-usage | Get daily usage |
| **Products** | GET /api/Products | Get products |
| | GET /api/Products/categories | Get categories |
| | GET /api/Products/favorites | Get favorites |
| | GET /api/Products/recommendations | Get recommendations |
| **Routines** | GET /api/Routines | Get routines |
| | POST /api/Routines | Create routine |
| | POST /api/Routines/{id}/complete | Complete routine |
| **Community** | GET /api/Community/feed | Get feed |
| | POST /api/Community/posts | Create post |
| | POST /api/Community/posts/{id}/like | Like post |
| | GET /api/Community/stations | Get stations |
| **Subscriptions** | GET /api/Subscriptions/plans | Get plans |
| | POST /api/Subscriptions/subscribe | Subscribe |
| | GET /api/Subscriptions/wallet | Get wallet |
| **Notifications** | GET /api/Notifications | Get notifications |
| | GET /api/Notifications/achievements | Get achievements |
| **Chat** | GET /api/Chat/conversations | Get conversations |
| | POST /api/Chat/messages | Send message |

See [API_DOCUMENTATION.md](Documentation/API_DOCUMENTATION.md) for full details.

## Database

The API uses SQL Server LocalDB with 37 tables:

- **Users & Auth**: Users, RefreshTokens, Roles, UserRoles, SkinProfiles
- **Subscriptions**: SubscriptionPlans, UserSubscriptions, PaymentTransactions, WalletTransactions
- **Scans**: SkinScans, SkinAnalysisResults, DailyScanUsages
- **Products**: Products, Brands, Distributors, ProductCategories, ProductBundles, ProductRecommendations
- **Routines**: UserRoutines, RoutineSteps, RoutineCompletions, RoutineReminders
- **Community**: CommunityPosts, PostLikes, PostComments, CreatorStations, StationFollowers
- **Campaigns**: BrandCampaigns, CampaignParticipants
- **Achievements**: Achievements, UserAchievements
- **Notifications**: Notifications
- **Chat**: ChatMessages

### Seed Data

The database is pre-seeded with:
- 4 Roles (Admin, Moderator, User, Creator)
- 4 Subscription Plans (Member Monthly/Yearly, Pro Monthly/Yearly)
- 8 Product Categories
- 5 Achievements

## Frontend Integration

See [FRONTEND_INTEGRATION.md](Documentation/FRONTEND_INTEGRATION.md) for:
- TypeScript API service
- Type definitions
- Example component code
- Migration from mock data

## Development

### Adding Migrations

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Logging

Logs are written to:
- Console (development)
- `logs/skinpai-.txt` (rolling file)

### Testing

Use Swagger UI for API testing:
```
http://localhost:5000/swagger
```

## License

Copyright © 2024 SkinPAI. All rights reserved.
