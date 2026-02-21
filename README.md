# SkinPAI - Complete Technical Documentation

**Version:** 1.0  
**Last Updated:** February 21, 2026  
**Projects:** SkinPAI.API (Backend) | Skinpaimobile (Frontend)

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture Overview](#2-architecture-overview)
3. [Backend Technical Documentation](#3-backend-technical-documentation)
4. [Frontend Technical Documentation](#4-frontend-technical-documentation)
5. [API Reference](#5-api-reference)
6. [Database Schema](#6-database-schema)
7. [Business Rules & Logic](#7-business-rules--logic)
8. [How-To Guides](#8-how-to-guides)
9. [Deployment & Configuration](#9-deployment--configuration)
10. [Security Considerations](#10-security-considerations)

---

## 1. Project Overview

### 1.1 Introduction

**SkinPAI** is a comprehensive AI-powered skincare analysis platform that enables users to scan their skin, receive personalized analysis, get product recommendations, and connect with a skincare community.

### 1.2 Core Features

| Feature | Description | User Tiers |
|---------|-------------|------------|
| **Skin Scanning** | AI-powered facial skin analysis using camera | All users |
| **Analysis Results** | Detailed skin health metrics (hydration, texture, concerns) | All users |
| **Product Recommendations** | Personalized product suggestions based on scan results | Member, Pro |
| **Progress Tracking** | Historical scan comparisons and progress charts | Member, Pro |
| **Community Feed** | Social features, posts, influencer stations | Member, Pro |
| **Creator Studio** | Influencer/creator station management | Pro only |
| **Routines & Reminders** | Skincare routine tracking with notifications | Member, Pro |
| **Wallet System** | In-app balance for subscriptions and purchases | All users |

### 1.3 Technology Stack

#### Backend (SkinPAI.API)
- **Framework:** .NET 9 Web API
- **Database:** SQL Server (LocalDB for dev, Azure SQL for prod)
- **ORM:** Entity Framework Core 9
- **Authentication:** JWT Bearer Tokens
- **Logging:** Serilog (Console + File)
- **AI Integration:** Hugging Face API (skin analysis)
- **File Storage:** Local filesystem (configurable for Azure Blob)

#### Frontend (Skinpaimobile)
- **Framework:** React 18 + TypeScript
- **Build Tool:** Vite 6.x
- **UI Components:** shadcn/ui + Tailwind CSS
- **State Management:** React Context + Local Storage
- **Internationalization:** i18next (Arabic RTL support)
- **Camera:** Web Camera API + face-api.js

### 1.4 Target Market

- **Primary Market:** Iraq/Basra region
- **Currency:** Iraqi Dinar (IQD)
- **Languages:** Arabic (RTL), English
- **Distributors:** Local pharmacies and beauty centers

---

## 2. Architecture Overview

### 2.1 System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                          CLIENT LAYER                                │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │                    React + Vite Frontend                       │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐   │  │
│  │  │  Components │  │   Services  │  │    State/Context    │   │  │
│  │  │  (UI/UX)    │  │   (api.ts)  │  │  (User, Theme, i18n)│   │  │
│  │  └─────────────┘  └─────────────┘  └─────────────────────┘   │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ HTTP/REST (JSON)
                                    │ JWT Authentication
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                          API LAYER                                   │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │                    .NET 9 Web API                              │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐   │  │
│  │  │ Controllers │  │  Services   │  │    Middleware       │   │  │
│  │  │  (REST)     │  │  (Business) │  │  (Auth, Logging)    │   │  │
│  │  └─────────────┘  └─────────────┘  └─────────────────────┘   │  │
│  │                                                                │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐   │  │
│  │  │ Repositories│  │    DTOs     │  │      Entities       │   │  │
│  │  │ (Data)      │  │  (Transfer) │  │    (Domain)         │   │  │
│  │  └─────────────┘  └─────────────┘  └─────────────────────┘   │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ Entity Framework Core
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        DATA LAYER                                    │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  SQL Server Database          │    File Storage               │  │
│  │  ┌────────────────────────┐   │    ┌─────────────────────┐   │  │
│  │  │ Users, Scans, Products │   │    │ /Uploads/           │   │  │
│  │  │ Subscriptions, Posts   │   │    │   /profiles/        │   │  │
│  │  │ Routines, Chat, etc.   │   │    │   /scans/           │   │  │
│  │  └────────────────────────┘   │    │   /posts/           │   │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ HTTP API
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      EXTERNAL SERVICES                               │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │  Hugging Face     │  Payment Gateway   │  Email Service     │    │
│  │  (Skin Analysis)  │  (Future)          │  (Future)          │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Project Structure

#### Backend Structure
```
SkinPAI.API/
├── Controllers/           # API endpoints (AuthController, ScansController, etc.)
├── Services/              # Business logic layer
├── Repositories/          # Data access layer (Unit of Work pattern)
├── Models/
│   ├── Entities/          # Database entity classes
│   └── DTOs/              # Data transfer objects
├── Data/
│   ├── SkinPAIDbContext   # EF Core DbContext
│   └── DataSeeder.cs      # Initial data seeding
├── Middleware/            # Custom middleware (logging, error handling)
├── Migrations/            # EF Core migrations
├── Uploads/               # File storage directory
├── Logs/                  # Application logs
└── Program.cs             # Application entry point
```

#### Frontend Structure
```
Skinpaimobile/src/
├── components/            # React components
│   ├── ui/                # Base UI components (shadcn/ui)
│   ├── figma/             # Figma-based design components
│   ├── navigation/        # Navigation components
│   ├── community/         # Community-specific components
│   └── shared/            # Shared utility components
├── services/
│   └── api.ts             # API service layer
├── hooks/                 # Custom React hooks
├── contexts/              # React context providers
├── types/                 # TypeScript type definitions
├── data/                  # Static data (legacy mock data)
├── i18n/                  # Internationalization files
├── styles/                # CSS/styling files
└── utils/                 # Utility functions
```

### 2.3 Data Flow

```
User Action → Component → api.ts → HTTP Request → Controller → Service → Repository → Database
                                                       ↓
                    UI Update ← State Update ← Response ← DTO ← Entity Mapping
```

---

## 3. Backend Technical Documentation

### 3.1 Controllers

| Controller | Base Route | Description |
|------------|------------|-------------|
| `AuthController` | `/api/Auth` | Authentication (login, register, tokens) |
| `UsersController` | `/api/Users` | User profile management |
| `ScansController` | `/api/Scans` | Skin scan operations |
| `ProductsController` | `/api/Products` | Product catalog and favorites |
| `CommunityController` | `/api/Community` | Posts, stations, campaigns |
| `RoutinesController` | `/api/Routines` | Skincare routines and reminders |
| `SubscriptionsController` | `/api/Subscriptions` | Plans, wallet, payments |
| `NotificationsController` | `/api/Notifications` | User notifications |
| `ChatController` | `/api/Chat` | Direct messaging |

### 3.2 Services Layer

```csharp
// Service interfaces implemented:
IAuthService          // Authentication, JWT token management
IUserService          // User CRUD, profile updates
IScanService          // Skin scan processing, analysis
IProductService       // Product catalog, recommendations
ICommunityService     // Posts, stations, campaigns
IRoutineService       // Routines, reminders
ISubscriptionService  // Plans, subscriptions, wallet
INotificationService  // Push notifications, achievements
IChatService          // Real-time messaging
IFileStorageService   // File upload/download
ISkinAnalysisAIService // Hugging Face AI integration
```

### 3.3 Repository Pattern

The backend uses the **Unit of Work** pattern for data access:

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<SkinScan> Scans { get; }
    IRepository<Product> Products { get; }
    IRepository<CommunityPost> CommunityPosts { get; }
    // ... all repositories
    
    Task<int> SaveChangesAsync();
}
```

### 3.4 Entity Relationships

```
User (1) ─────────────< (N) SkinScan
User (1) ─────────────< (N) UserSubscription
User (1) ─────────────< (N) UserRoutine
User (1) ─────────────< (N) CommunityPost
User (1) ─────────────< (N) UserProductFavorite
User (1) ─────────────< (N) Notification
User (1) ────────────── (1) SkinProfile
User (1) ────────────── (1) CreatorStation [optional]

SkinScan (1) ─────────── (1) SkinAnalysisResult
SkinScan (1) ─────────< (N) ProductRecommendation

Product (N) ─────────── (1) Brand
Product (N) ─────────── (1) Distributor
Product (N) ─────────── (1) ProductCategory

CreatorStation (1) ───< (N) StationFollower
CreatorStation (1) ───< (N) CommunityPost

CommunityPost (1) ────< (N) PostComment
CommunityPost (1) ────< (N) PostLike
```

### 3.5 Middleware

```csharp
// Custom middleware pipeline:
app.UseMiddleware<RequestLoggingMiddleware>();  // Logs all requests
app.UseMiddleware<ErrorHandlingMiddleware>();   // Global error handling

// Built-in middleware:
app.UseAuthentication();  // JWT validation
app.UseAuthorization();   // Role-based access
app.UseCors();            // Cross-origin requests
```

### 3.6 Configuration

**appsettings.json structure:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SkinPAI;..."
  },
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "SkinPAI.API",
    "Audience": "SkinPAI.Client",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  },
  "HuggingFace": {
    "ApiKey": "...",
    "TimeoutSeconds": 60
  },
  "FileStorage": {
    "BasePath": "./Uploads",
    "MaxFileSizeMB": 10
  }
}
```

---

## 4. Frontend Technical Documentation

### 4.1 Component Architecture

#### Core Components

| Component | Purpose | Key Features |
|-----------|---------|--------------|
| `App.tsx` | Root component | State management, routing, user session |
| `AuthScreen` | Authentication | Login, register, social auth |
| `MemberDashboard` | Main dashboard | Scan history, progress, quick actions |
| `ProDashboard` | Pro user dashboard | Creator stats, analytics |
| `CameraInterface` | Skin scanning | Camera capture, face detection |
| `ScanResults` | Analysis display | Metrics, recommendations, comparison |
| `ProductRecommendations` | Product catalog | Filters, search, favorites |
| `CommunityFeed` | Social features | Posts, influencers, campaigns |
| `ProfileSettings` | User settings | Profile, preferences, subscription |

#### UI Component Library

Based on **shadcn/ui** with custom styling:

```typescript
// Core UI components
Button, Card, Badge, Avatar, Input, Select,
Tabs, Dialog, Sheet, Checkbox, Slider, Progress,
DropdownMenu, ScrollArea, Separator, etc.
```

### 4.2 State Management

**User State (Global):**
```typescript
interface User {
  id: string;
  type: 'guest' | 'member' | 'pro';
  scansToday: number;
  maxScans: number;
  name?: string;
  email?: string;
  walletBalance?: number;
  questionnaireCompleted?: boolean;
  skinProfile?: SkinProfile;
}
```

**State Persistence:**
- `localStorage` for user session and preferences
- API sync for critical data
- React Context for global state distribution

### 4.3 API Integration Layer

**api.ts Service Class:**

```typescript
class ApiService {
  private baseUrl: string;
  private accessToken: string | null;
  
  // Authentication
  async login(data: LoginRequest): Promise<ApiResponse<AuthResponse>>
  async register(data: RegisterRequest): Promise<ApiResponse<AuthResponse>>
  async socialLogin(data: SocialLoginRequest): Promise<ApiResponse<SocialAuthResponse>>
  async refreshToken(token: string): Promise<ApiResponse<AuthResponse>>
  
  // User Management
  async getProfile(): Promise<ApiResponse<UserDto>>
  async updateProfile(data: UpdateProfileRequest): Promise<ApiResponse<UserDto>>
  
  // Scans
  async uploadScan(imageBase64: string): Promise<ApiResponse<ScanResultDto>>
  async getScanHistory(page: number): Promise<ApiResponse<PaginatedResponse<ScanResultDto>>>
  async getSkinProgress(): Promise<ApiResponse<SkinProgressDto>>
  
  // Products
  async getProducts(params: ProductSearchParams): Promise<ApiResponse<PaginatedResponse<ProductDto>>>
  async getFavorites(): Promise<ApiResponse<ProductDto[]>>
  async addFavorite(productId: string): Promise<ApiResponse<void>>
  
  // Community
  async getCommunityFeed(page: number): Promise<ApiResponse<PaginatedResponse<CommunityPostDto>>>
  async createPost(data: CreatePostRequest): Promise<ApiResponse<CommunityPostDto>>
  async likePost(postId: string): Promise<ApiResponse<void>>
  
  // ...more endpoints
}

export const api = new ApiService();
export default api;
```

### 4.4 Internationalization (i18n)

**Supported Languages:**
- English (en)
- Arabic (ar) - with RTL support

**Implementation:**
```typescript
// Using react-i18next
const { t, i18n } = useTranslation();

// RTL Context
const { isRTL, direction, flexDir } = useAppTranslation();

// Usage in components
<div className={`flex ${flexDir}`} style={{ direction }}>
  <span>{t('dashboard.welcome')}</span>
</div>
```

### 4.5 Hooks

| Hook | Purpose |
|------|---------|
| `useAppTranslation` | i18n + RTL handling |
| `useLocalStorage` | Persisted state |
| `useNavigation` | Screen navigation |
| `useScanProgress` | Scan progress tracking |

---

## 5. API Reference

### 5.1 Authentication Endpoints

#### POST /api/Auth/register
Register a new user account.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "phoneNumber": "+9647701234567",
  "dateOfBirth": "1990-05-15",
  "gender": "Male"
}
```

**Response (200):**
```json
{
  "userId": "guid",
  "email": "user@example.com",
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "membershipType": "Guest",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-refresh-token",
  "expiresAt": "2026-02-21T15:00:00Z"
}
```

#### POST /api/Auth/login
Authenticate existing user.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

#### POST /api/Auth/social-login
Authenticate via social provider.

**Request:**
```json
{
  "provider": "google",
  "idToken": "google-id-token",
  "firstName": "Ahmed",
  "lastName": "Hassan"
}
```

#### POST /api/Auth/guest-login
Create temporary guest session.

**Response:**
```json
{
  "userId": "guid",
  "membershipType": "Guest",
  "maxScans": 3,
  "accessToken": "...",
  "expiresAt": "..."
}
```

#### POST /api/Auth/refresh-token
Refresh access token.

**Request:**
```json
{
  "refreshToken": "current-refresh-token"
}
```

### 5.2 User Endpoints

#### GET /api/Users/profile
Get current user profile.

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "userId": "guid",
  "email": "user@example.com",
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "profileImageUrl": "/uploads/profiles/img.jpg",
  "membershipType": "Member",
  "membershipStatus": "Active",
  "walletBalance": 50000.00,
  "totalScansUsed": 15,
  "isVerified": false,
  "isCreator": false,
  "questionnaireCompleted": true,
  "skinProfile": {
    "skinType": "Combination",
    "skinConcerns": "Acne, Dark Spots",
    "currentRoutine": "Basic cleanser and moisturizer",
    "sunExposure": "Moderate",
    "lifestyle": "Office work, occasional outdoor"
  }
}
```

#### PUT /api/Users/profile
Update user profile.

#### PUT /api/Users/skin-profile
Update skin questionnaire.

**Request:**
```json
{
  "skinType": "Oily",
  "skinConcernsJson": "[\"Acne\", \"Large Pores\"]",
  "currentRoutine": "Full 10-step routine",
  "sunExposure": "High",
  "lifestyle": "Active outdoor"
}
```

#### POST /api/Users/profile-image
Upload profile image (multipart/form-data or base64).

### 5.3 Scan Endpoints

#### POST /api/Scans
Upload and analyze a skin scan.

**Request:**
```json
{
  "imageBase64": "data:image/jpeg;base64,/9j/4AAQSkZ...",
  "scanType": "Face"
}
```

**Response:**
```json
{
  "scanId": "guid",
  "userId": "guid",
  "scanImageUrl": "/uploads/scans/scan_xxx.jpg",
  "overlayImageUrl": "/uploads/scans/scan_xxx_overlay.jpg",
  "scanType": "Face",
  "scanDate": "2026-02-21T12:00:00Z",
  "aiProcessingStatus": "Completed",
  "overallScore": 78.5,
  "estimatedSkinAge": 28,
  "analysisResult": {
    "hydrationLevel": 72,
    "oilLevel": 45,
    "textureScore": 68,
    "poreVisibility": 35,
    "wrinkleScore": 15,
    "acneSeverity": 25,
    "pigmentationScore": 30,
    "sensitivityLevel": 20,
    "skinTypeDetected": "Combination",
    "recommendedIngredients": ["Niacinamide", "Salicylic Acid", "Hyaluronic Acid"],
    "concerns": ["Moderate acne", "Visible pores"]
  },
  "recommendations": [/*product recommendations*/]
}
```

#### GET /api/Scans
Get scan history (paginated).

**Query Parameters:**
- `page` (default: 1)
- `pageSize` (default: 10)

#### GET /api/Scans/{scanId}
Get specific scan details.

#### GET /api/Scans/latest
Get most recent scan.

#### GET /api/Scans/progress
Get skin progress over time.

**Response:**
```json
{
  "currentScore": 78.5,
  "scoreChange": +5.2,
  "totalScans": 15,
  "progressData": [
    {"date": "2026-01-21", "value": 73.3},
    {"date": "2026-02-21", "value": 78.5}
  ],
  "improvements": ["Hydration +8%", "Acne -12%"],
  "areasNeedingAttention": ["Sun protection"]
}
```

### 5.4 Product Endpoints

#### GET /api/Products
Get product catalog (paginated, filterable).

**Query Parameters:**
- `page`, `pageSize`
- `brandId`, `categoryId`, `distributorId`
- `minPrice`, `maxPrice`
- `skinType`, `skinConcern`
- `inStock`, `onSale`
- `search`
- `sortBy` (price, rating, name)

**Response:**
```json
{
  "items": [
    {
      "productId": "guid",
      "productName": "Niacinamide 10% + Zinc 1%",
      "description": "Targets blemishes and congestion",
      "price": 15000.00,
      "originalPrice": 18000.00,
      "discountPercent": 17,
      "averageRating": 4.8,
      "totalReviews": 1250,
      "productImageUrl": "/images/products/niacinamide.jpg",
      "brand": {
        "brandId": "guid",
        "brandName": "The Ordinary",
        "logoUrl": "/images/brands/ordinary.png",
        "isVerified": true
      },
      "category": {
        "categoryId": "guid",
        "categoryName": "Serums",
        "iconName": "droplet"
      },
      "distributor": {
        "distributorId": "guid",
        "name": "Basra Pharmacy",
        "isPartner": true
      },
      "skinTypes": ["Oily", "Combination"],
      "skinConcerns": ["Acne", "Blemishes"],
      "keyIngredients": ["Niacinamide", "Zinc PCA"],
      "inStock": true,
      "shopUrl": "https://..."
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 156,
  "totalPages": 8
}
```

#### GET /api/Products/{productId}
Get single product details.

#### GET /api/Products/brands
Get all brands.

#### GET /api/Products/categories
Get all product categories.

#### GET /api/Products/distributors
Get all distributors.

#### GET /api/Products/favorites
Get user's favorite products.

#### POST /api/Products/favorites/{productId}
Add product to favorites.

#### DELETE /api/Products/favorites/{productId}
Remove product from favorites.

#### GET /api/Products/bundles
Get product bundles.

### 5.5 Community Endpoints

#### GET /api/Community/feed
Get community posts (paginated).

**Query Parameters:**
- `page`, `pageSize`
- `stationId` (filter by creator station)

#### POST /api/Community/posts
Create a new post.

**Request:**
```json
{
  "content": "Just finished my evening routine! ✨",
  "postType": "image",
  "title": "Evening Skincare",
  "mediaBase64": ["data:image/jpeg;base64,..."],
  "tags": ["routine", "evening"],
  "hashtags": ["#skincare", "#glowup"]
}
```

#### POST /api/Community/posts/{postId}/like
Like a post.

#### POST /api/Community/posts/{postId}/unlike
Unlike a post.

#### GET /api/Community/posts/{postId}/comments
Get post comments.

#### POST /api/Community/posts/{postId}/comments
Add a comment.

#### GET /api/Community/stations
Get all creator stations.

#### GET /api/Community/stations/{stationId}
Get specific station.

#### POST /api/Community/stations/{stationId}/follow
Follow a station.

#### GET /api/Community/influencers/top
Get top influencers.

#### GET /api/Community/campaigns
Get active brand campaigns.

#### POST /api/Community/campaigns/{campaignId}/join
Join a campaign.

#### GET /api/Community/stats
Get community statistics.

### 5.6 Routine Endpoints

#### GET /api/Routines
Get user's routines.

#### POST /api/Routines
Create a new routine.

**Request:**
```json
{
  "routineName": "Morning Routine",
  "routineType": "AM",
  "description": "My daily morning skincare",
  "steps": [
    {"stepNumber": 1, "productId": "guid", "productName": "Cleanser", "instructions": "Apply to wet face"},
    {"stepNumber": 2, "productId": "guid", "productName": "Toner", "instructions": "Pat into skin"}
  ]
}
```

#### GET /api/Routines/{routineId}
Get specific routine.

#### PUT /api/Routines/{routineId}
Update routine.

#### DELETE /api/Routines/{routineId}
Delete routine.

#### POST /api/Routines/{routineId}/complete
Mark routine as completed for today.

#### GET /api/Routines/reminders
Get routine reminders.

#### POST /api/Routines/reminders
Create a reminder.

#### PUT /api/Routines/reminders/{reminderId}
Update reminder.

#### DELETE /api/Routines/reminders/{reminderId}
Delete reminder.

### 5.7 Subscription Endpoints

#### GET /api/Subscriptions/plans
Get available subscription plans.

**Response:**
```json
[
  {
    "planId": "guid",
    "planCode": "MEMBER",
    "planName": "Member",
    "description": "Essential skincare features",
    "priceMonthly": 15000.00,
    "priceYearly": 150000.00,
    "scansPerDay": 5,
    "hasAdvancedAnalysis": true,
    "hasProductRecommendations": true,
    "hasProgressTracking": true,
    "hasCommunityAccess": true,
    "hasCreatorStudio": false,
    "hasPrioritySupport": false,
    "adFree": false
  },
  {
    "planId": "guid",
    "planCode": "PRO",
    "planName": "Pro",
    "description": "Full creator features",
    "priceMonthly": 35000.00,
    "priceYearly": 350000.00,
    "scansPerDay": -1,
    "hasCreatorStudio": true,
    "hasPrioritySupport": true,
    "adFree": true
  }
]
```

#### POST /api/Subscriptions/subscribe
Subscribe to a plan.

**Request:**
```json
{
  "planId": "guid",
  "paymentMethodId": "wallet"
}
```

#### GET /api/Subscriptions/current
Get current subscription.

#### POST /api/Subscriptions/cancel
Cancel subscription.

#### GET /api/Subscriptions/wallet
Get wallet balance.

#### POST /api/Subscriptions/wallet/add-funds
Add funds to wallet.

**Request:**
```json
{
  "amount": 50000.00,
  "paymentMethodId": "card_xxx"
}
```

### 5.8 Notification Endpoints

#### GET /api/Notifications
Get user notifications.

**Query Parameters:**
- `unreadOnly` (boolean)

#### PUT /api/Notifications/{notificationId}/read
Mark notification as read.

#### PUT /api/Notifications/read-all
Mark all notifications as read.

#### GET /api/Notifications/achievements
Get user achievements.

### 5.9 Chat Endpoints

#### GET /api/Chat/conversations
Get all conversations.

#### GET /api/Chat/conversations/{otherUserId}/messages
Get messages with a user.

#### POST /api/Chat/messages
Send a message.

**Request:**
```json
{
  "receiverId": "guid",
  "content": "Hello!",
  "mediaBase64": null
}
```

#### PUT /api/Chat/conversations/{otherUserId}/read
Mark conversation as read.

---

## 6. Database Schema

### 6.1 Core Tables

#### Users
| Column | Type | Description |
|--------|------|-------------|
| UserId | GUID (PK) | Primary key |
| Email | NVARCHAR(256) | Unique email |
| PasswordHash | NVARCHAR(MAX) | BCrypt hash |
| FirstName | NVARCHAR(100) | First name |
| LastName | NVARCHAR(100) | Last name |
| MembershipType | NVARCHAR(20) | Guest/Member/Pro |
| MembershipStatus | NVARCHAR(20) | Active/Suspended/Cancelled |
| WalletBalance | DECIMAL(18,2) | In-app balance |
| TotalScansUsed | INT | Lifetime scan count |
| IsCreator | BIT | Has creator station |
| IsVerified | BIT | Verified badge |
| CreatedAt | DATETIME2 | Registration date |
| LastLoginAt | DATETIME2 | Last login |

#### SkinScans
| Column | Type | Description |
|--------|------|-------------|
| ScanId | GUID (PK) | Primary key |
| UserId | GUID (FK) | User reference |
| ScanImageUrl | NVARCHAR(500) | Image path |
| OverlayImageUrl | NVARCHAR(500) | AI overlay path |
| ScanType | NVARCHAR(50) | Face/Forehead/etc |
| AIProcessingStatus | NVARCHAR(20) | Pending/Completed/Failed |
| OverallScore | DECIMAL(5,2) | 0-100 score |
| EstimatedSkinAge | INT | AI-estimated age |
| ScanDate | DATETIME2 | Scan timestamp |

#### SkinAnalysisResults
| Column | Type | Description |
|--------|------|-------------|
| ResultId | GUID (PK) | Primary key |
| ScanId | GUID (FK) | Scan reference |
| HydrationLevel | DECIMAL(5,2) | 0-100 |
| OilLevel | DECIMAL(5,2) | 0-100 |
| TextureScore | DECIMAL(5,2) | 0-100 |
| AcneSeverity | DECIMAL(5,2) | 0-100 |
| WrinkleScore | DECIMAL(5,2) | 0-100 |
| PigmentationScore | DECIMAL(5,2) | 0-100 |
| SkinTypeDetected | NVARCHAR(50) | Detected skin type |
| ConcernsJson | NVARCHAR(MAX) | JSON array |
| RecommendedIngredientsJson | NVARCHAR(MAX) | JSON array |

#### Products
| Column | Type | Description |
|--------|------|-------------|
| ProductId | GUID (PK) | Primary key |
| ProductName | NVARCHAR(200) | Product name |
| BrandId | GUID (FK) | Brand reference |
| CategoryId | GUID (FK) | Category reference |
| DistributorId | GUID (FK) | Distributor reference |
| Price | DECIMAL(18,2) | Current price (IQD) |
| OriginalPrice | DECIMAL(18,2) | Original price |
| ProductImageUrl | NVARCHAR(500) | Image URL |
| ShopUrl | NVARCHAR(500) | Purchase URL |
| InStock | BIT | Availability |
| SkinTypesJson | NVARCHAR(MAX) | Target skin types |
| SkinConcernsJson | NVARCHAR(MAX) | Target concerns |
| KeyIngredientsJson | NVARCHAR(MAX) | Ingredients list |

#### SubscriptionPlans
| Column | Type | Description |
|--------|------|-------------|
| PlanId | GUID (PK) | Primary key |
| PlanCode | NVARCHAR(50) | GUEST/MEMBER/PRO |
| PlanName | NVARCHAR(50) | Display name |
| PriceMonthly | DECIMAL(18,2) | Monthly price |
| PriceYearly | DECIMAL(18,2) | Yearly price |
| DailyScansLimit | INT | Scans per day (null=unlimited) |
| HasAdvancedAnalysis | BIT | Feature flag |
| HasProductRecommendations | BIT | Feature flag |
| HasProgressTracking | BIT | Feature flag |
| HasCommunityAccess | BIT | Feature flag |
| HasCreatorStudio | BIT | Feature flag |

### 6.2 Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        SkinPAI Database                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐       ┌──────────────┐       ┌──────────────┐ │
│  │    Users     │───────│  SkinScans   │───────│ SkinAnalysis │ │
│  └──────────────┘       └──────────────┘       │   Results    │ │
│         │                      │               └──────────────┘ │
│         │                      │                                 │
│         ▼                      ▼                                 │
│  ┌──────────────┐       ┌──────────────┐                        │
│  │ UserRoutines │       │   Product    │                        │
│  └──────────────┘       │Recommendations│                       │
│         │               └──────────────┘                        │
│         │                      │                                 │
│         ▼                      ▼                                 │
│  ┌──────────────┐       ┌──────────────┐       ┌──────────────┐ │
│  │RoutineSteps  │       │   Products   │◄──────│   Brands     │ │
│  └──────────────┘       └──────────────┘       └──────────────┘ │
│                                │                      │          │
│                                │                      ▼          │
│                                │               ┌──────────────┐ │
│                                └───────────────│ Distributors │ │
│                                                └──────────────┘ │
│                                                                  │
│  ┌──────────────┐       ┌──────────────┐       ┌──────────────┐ │
│  │   Users      │───────│CommunityPosts│───────│ PostComments │ │
│  └──────────────┘       └──────────────┘       └──────────────┘ │
│         │                      │                                 │
│         │                      ▼                                 │
│         │               ┌──────────────┐                        │
│         │               │  PostLikes   │                        │
│         │               └──────────────┘                        │
│         │                                                        │
│         ▼                                                        │
│  ┌──────────────┐       ┌──────────────┐       ┌──────────────┐ │
│  │CreatorStation│───────│StationFollows │      │BrandCampaigns│ │
│  └──────────────┘       └──────────────┘       └──────────────┘ │
│                                                                  │
│  ┌──────────────┐       ┌──────────────┐       ┌──────────────┐ │
│  │    Users     │───────│ Subscriptions│───────│    Plans     │ │
│  └──────────────┘       └──────────────┘       └──────────────┘ │
│         │                                                        │
│         ▼                                                        │
│  ┌──────────────┐       ┌──────────────┐                        │
│  │WalletTransact│       │Notifications │                        │
│  └──────────────┘       └──────────────┘                        │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 7. Business Rules & Logic

### 7.1 User Membership Tiers

#### Guest
| Rule | Value |
|------|-------|
| Scans per day | 3 |
| Scan history retention | 1 scan (latest only) |
| Product recommendations | Limited (3 products) |
| Community access | View only |
| Account persistence | 24 hours (unless registered) |

#### Member (Paid)
| Rule | Value |
|------|-------|
| Scans per day | 5 |
| Scan history | Unlimited |
| Product recommendations | Full personalized |
| Progress tracking | Full access |
| Community access | Full (post, comment, like) |
| Routines | Up to 10 routines |
| Monthly price | 15,000 IQD |
| Yearly price | 150,000 IQD (17% savings) |

#### Pro (Premium)
| Rule | Value |
|------|-------|
| Scans per day | Unlimited |
| All Member features | Yes |
| Creator Studio | Full access |
| Station followers | Unlimited |
| Campaign participation | Priority |
| Priority support | Yes |
| Ad-free experience | Yes |
| Monthly price | 35,000 IQD |
| Yearly price | 350,000 IQD (17% savings) |

### 7.2 Scan Processing Rules

1. **Daily Scan Limit Check**
   ```
   IF user.scansToday >= plan.dailyScansLimit THEN
     REJECT "Daily scan limit reached"
   ```

2. **Image Validation**
   - Minimum resolution: 640x480
   - Maximum file size: 10MB
   - Accepted formats: JPEG, PNG, WebP
   - Face must be detected in image

3. **AI Processing Flow**
   ```
   UPLOAD → VALIDATE → STORE → QUEUE_AI → PROCESS → STORE_RESULTS → NOTIFY
   ```

4. **Score Calculation**
   ```
   overallScore = (hydration * 0.15) + (texture * 0.15) + (clarity * 0.15) +
                  (100 - acne * 0.2) + (100 - wrinkles * 0.15) + (elasticity * 0.1) +
                  (100 - pigmentation * 0.1)
   ```

### 7.3 Product Recommendation Logic

1. **Match by Skin Type**
   - Products matching detected skin type get +30 relevance score

2. **Match by Concerns**
   - Each matching concern adds +20 relevance score

3. **Match by Ingredients**
   - Products with recommended ingredients get +15 per match

4. **Sort Order**
   - Primary: Relevance score (DESC)
   - Secondary: Average rating (DESC)
   - Tertiary: In stock first

5. **Filtering Rules**
   - Guest: Show top 3 recommendations
   - Member/Pro: Show all with full details

### 7.4 Subscription & Billing Rules

1. **Subscription Activation**
   - Immediate on successful payment
   - Grace period: 3 days for payment failures

2. **Wallet Transactions**
   - Minimum add: 5,000 IQD
   - Maximum wallet balance: 500,000 IQD
   - Refund policy: Within 14 days, unused scans only

3. **Cancellation**
   - Access continues until period end
   - No proration or refunds
   - Downgrade to Guest after expiry

4. **Upgrade/Downgrade**
   - Immediate effect
   - Proration calculated for remaining days
   - Downgrade: Features removed at next billing

### 7.5 Community Rules

1. **Posting Requirements**
   - Member: Can post text and images
   - Pro: Can post videos and create campaigns

2. **Creator Station Rules**
   - Pro only feature
   - One station per user
   - Minimum 10 posts to appear in "Featured"
   - Verified badge: 1000+ followers + manual review

3. **Content Moderation**
   - Auto-flag: profanity, spam patterns
   - Manual review queue for flagged content
   - 3 strikes = temporary suspension

4. **Engagement Limits**
   - Comments: 100/day per user
   - Likes: 500/day per user
   - Posts: 10/day for Members, 50/day for Pro

### 7.6 Routine & Reminder Rules

1. **Routine Limits**
   - Guest: View only (demo routines)
   - Member: 10 custom routines
   - Pro: Unlimited routines

2. **Reminder Scheduling**
   - Minimum interval: 1 hour apart
   - Maximum reminders: 20 per user
   - Time zones: Stored in UTC, displayed in local

3. **Completion Tracking**
   - Streak counting: Consecutive days
   - Statistics: Weekly/monthly completion rates

---

## 8. How-To Guides

### 8.1 How to Set Up Development Environment

#### Prerequisites
- .NET 9 SDK
- Node.js 18+
- SQL Server LocalDB (or Docker SQL Server)
- VS Code or Visual Studio 2022

#### Backend Setup
```powershell
# Clone and navigate
cd C:\Projects\SkinPAI.API

# Restore packages
dotnet restore

# Apply database migrations
cd SkinPAI.API
dotnet ef database update

# Run the API
dotnet run --urls="http://localhost:5001"
```

#### Frontend Setup
```powershell
# Navigate to frontend
cd C:\Projects\Skinpaimobile-main\Skinpaimobile-main

# Install dependencies
npm install

# Start development server
npm run dev
```

### 8.2 How to Add a New API Endpoint

1. **Create/Update DTO** (`Models/DTOs/`)
   ```csharp
   public record NewFeatureDto(
       Guid Id,
       string Name,
       DateTime CreatedAt
   );
   ```

2. **Add Service Method** (`Services/`)
   ```csharp
   public interface IFeatureService
   {
       Task<NewFeatureDto> GetFeatureAsync(Guid id);
   }
   ```

3. **Add Controller Endpoint** (`Controllers/`)
   ```csharp
   [HttpGet("{id}")]
   public async Task<ActionResult<NewFeatureDto>> GetFeature(Guid id)
   {
       var result = await _featureService.GetFeatureAsync(id);
       return Ok(result);
   }
   ```

4. **Add Frontend API Method** (`services/api.ts`)
   ```typescript
   async getFeature(id: string): Promise<ApiResponse<NewFeatureDto>> {
     return this.request<NewFeatureDto>(`/Feature/${id}`);
   }
   ```

### 8.3 How to Add a New Entity

1. **Create Entity Class** (`Models/Entities/NewEntity.cs`)
   ```csharp
   public class NewEntity
   {
       [Key]
       public Guid Id { get; set; } = Guid.NewGuid();
       
       [Required, MaxLength(200)]
       public string Name { get; set; } = string.Empty;
       
       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
   }
   ```

2. **Add to DbContext** (`Data/SkinPAIDbContext.cs`)
   ```csharp
   public DbSet<NewEntity> NewEntities { get; set; }
   ```

3. **Add Repository** (`Repositories/IUnitOfWork.cs`)
   ```csharp
   IRepository<NewEntity> NewEntities { get; }
   ```

4. **Create Migration**
   ```powershell
   dotnet ef migrations add AddNewEntity
   dotnet ef database update
   ```

### 8.4 How to Add a New Component

1. **Create Component File** (`src/components/NewComponent.tsx`)
   ```tsx
   import React from 'react';
   import { Card, CardContent } from './ui/card';
   import { useAppTranslation } from '../hooks/useAppTranslation';
   
   interface NewComponentProps {
     data: SomeType;
     onAction: () => void;
   }
   
   export default function NewComponent({ data, onAction }: NewComponentProps) {
     const { t, isRTL } = useAppTranslation();
     
     return (
       <Card>
         <CardContent>
           {/* Component content */}
         </CardContent>
       </Card>
     );
   }
   ```

2. **Add to App.tsx** (if needed for routing)
   ```tsx
   {currentScreen === 'new-feature' && (
     <NewComponent
       data={someData}
       onAction={handleAction}
     />
   )}
   ```

### 8.5 How to Add Translations

1. **Add English Translation** (`src/i18n/en.json`)
   ```json
   {
     "newFeature": {
       "title": "New Feature",
       "description": "Description of feature"
     }
   }
   ```

2. **Add Arabic Translation** (`src/i18n/ar.json`)
   ```json
   {
     "newFeature": {
       "title": "ميزة جديدة",
       "description": "وصف الميزة"
     }
   }
   ```

3. **Use in Component**
   ```tsx
   const { t } = useAppTranslation();
   return <h1>{t('newFeature.title')}</h1>;
   ```

### 8.6 How to Debug API Issues

1. **Check Logs**
   ```powershell
   # View recent logs
   Get-Content C:\Projects\SkinPAI.API\SkinPAI.API\Logs\SkinPAI-*.log -Tail 100
   ```

2. **Test API Directly**
   ```powershell
   # Health check
   Invoke-RestMethod -Uri "http://localhost:5001/health"
   
   # Test endpoint
   Invoke-RestMethod -Uri "http://localhost:5001/api/Products?pageSize=5" `
     -Headers @{Authorization = "Bearer $token"}
   ```

3. **Check Swagger**
   - Navigate to `http://localhost:5001/swagger`
   - Test endpoints interactively

4. **Debug Frontend API Calls**
   - Open browser DevTools (F12)
   - Network tab → Filter by XHR
   - Check request/response details

---

## 9. Deployment & Configuration

### 9.1 Environment Configuration

#### Development (.env / appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SkinPAI;..."
  },
  "JwtSettings": {
    "SecretKey": "dev-secret-key-minimum-32-characters"
  },
  "HuggingFace": {
    "ApiKey": "hf_xxxxxxxx"
  }
}
```

#### Production Considerations
- Use Azure Key Vault for secrets
- Use Azure SQL Database
- Use Azure Blob Storage for files
- Configure HTTPS with valid SSL
- Set up Azure CDN for static assets

### 9.2 Build Commands

#### Backend
```powershell
# Development
dotnet run

# Production build
dotnet publish -c Release -o ./publish

# With Docker
docker build -t skinpai-api .
docker run -p 5001:80 skinpai-api
```

#### Frontend
```powershell
# Development
npm run dev

# Production build
npm run build

# Preview production build
npm run preview
```

### 9.3 Database Migrations

```powershell
# Create migration
dotnet ef migrations add MigrationName -p SkinPAI.API

# Apply migrations
dotnet ef database update -p SkinPAI.API

# Rollback
dotnet ef database update PreviousMigrationName -p SkinPAI.API

# Generate SQL script
dotnet ef migrations script -o migration.sql -p SkinPAI.API
```

---

## 10. Security Considerations

### 10.1 Authentication & Authorization

- **JWT Tokens**: Access tokens expire in 60 minutes
- **Refresh Tokens**: Expire in 30 days, stored securely
- **Password Requirements**: Minimum 8 characters, mixed case, numbers
- **Failed Login Lockout**: 5 attempts, 15-minute lockout

### 10.2 Data Protection

- **Passwords**: BCrypt hashing with salt
- **Sensitive Data**: Not logged (filtered by middleware)
- **File Uploads**: Validated, sanitized filenames
- **SQL Injection**: Prevented via EF Core parameterization

### 10.3 API Security

- **CORS**: Configured for specific origins only
- **Rate Limiting**: Recommended for production (100 req/min)
- **Input Validation**: DTOs with validation attributes
- **HTTPS**: Required in production

### 10.4 Privacy Compliance

- **Data Retention**: Scan images stored for account lifetime
- **Right to Delete**: User can delete all data
- **Data Export**: User can request data export
- **Consent**: Explicit consent for AI analysis

---

## Appendix A: Error Codes

| Code | Description |
|------|-------------|
| AUTH001 | Invalid credentials |
| AUTH002 | Account locked |
| AUTH003 | Token expired |
| AUTH004 | Refresh token invalid |
| SCAN001 | Daily limit reached |
| SCAN002 | Image validation failed |
| SCAN003 | AI processing failed |
| SUB001 | Insufficient wallet balance |
| SUB002 | Plan not found |
| COM001 | Content flagged |
| COM002 | Rate limit exceeded |

## Appendix B: Status Codes

| HTTP Code | Usage |
|-----------|-------|
| 200 | Success |
| 201 | Created |
| 204 | No Content (delete success) |
| 400 | Bad Request (validation) |
| 401 | Unauthorized |
| 403 | Forbidden (insufficient tier) |
| 404 | Not Found |
| 409 | Conflict (duplicate) |
| 429 | Rate Limited |
| 500 | Internal Server Error |

## Appendix C: Glossary

| Term | Definition |
|------|------------|
| **Scan** | AI analysis of user's skin photo |
| **Station** | Creator's profile/channel (Pro only) |
| **Routine** | Ordered list of skincare products |
| **Bundle** | Curated product package |
| **Campaign** | Brand promotional event |
| **IQD** | Iraqi Dinar (currency) |

---

**Document maintained by:** SkinPAI Development Team  
**For questions:** Contact development@skinpai.com
