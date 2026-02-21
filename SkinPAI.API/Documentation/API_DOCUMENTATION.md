# SkinPAI API Documentation

## Overview

SkinPAI is a comprehensive .NET 9 Web API designed to power the SkinPAI mobile application, providing skin analysis, product recommendations, community features, and subscription management.

**Base URL:** `http://localhost:5000/api`  
**Swagger UI:** `http://localhost:5000/swagger`

## Authentication

The API uses JWT Bearer token authentication. Include the token in the Authorization header:

```
Authorization: Bearer {your_jwt_token}
```

### Token Lifecycle
- **Access Token:** 60 minutes expiration
- **Refresh Token:** 30 days expiration

---

## API Endpoints

### Authentication (`/api/Auth`)

#### Register
**POST** `/api/Auth/register`

Creates a new user account.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1990-01-15",
  "gender": "Male"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "random-refresh-token",
  "expiresIn": 3600,
  "user": {
    "userId": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "membershipType": "Guest",
    "isCreator": false,
    "isVerified": false,
    "walletBalance": 0,
    "totalScansUsed": 0
  }
}
```

#### Login
**POST** `/api/Auth/login`

Authenticates an existing user.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** Same as Register

#### Guest Login
**POST** `/api/Auth/guest`

Creates a temporary guest account for limited access.

**Response:** Same as Register (with guest membership)

#### Refresh Token
**POST** `/api/Auth/refresh`

Exchanges a refresh token for a new access token.

**Request Body:**
```json
{
  "refreshToken": "your-refresh-token"
}
```

**Response:** Same as Register

#### Logout
**POST** `/api/Auth/logout`

🔒 **Requires Authentication**

Invalidates the current refresh token.

---

### Users (`/api/Users`)

#### Get Current User
**GET** `/api/Users/me`

🔒 **Requires Authentication**

Returns the current user's profile.

**Response (200 OK):**
```json
{
  "userId": "guid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "profileImageUrl": "/uploads/profiles/user.jpg",
  "membershipType": "Member",
  "membershipStatus": "Active",
  "isCreator": false,
  "isVerified": false,
  "walletBalance": 50.00,
  "totalScansUsed": 15
}
```

#### Update Profile
**PUT** `/api/Users/me`

🔒 **Requires Authentication**

Updates the current user's profile.

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "bio": "Skincare enthusiast",
  "profileImageBase64": "data:image/jpeg;base64,/9j/4AAQ..."
}
```

#### Get Member Dashboard
**GET** `/api/Users/dashboard/member`

🔒 **Requires Authentication**

Returns member dashboard data including stats, achievements, and progress.

#### Get Pro Dashboard
**GET** `/api/Users/dashboard/pro`

🔒 **Requires Authentication** (Pro subscription required)

Returns pro dashboard data including analytics, earnings, and audience insights.

---

### Skin Scans (`/api/Scans`)

#### Create Scan
**POST** `/api/Scans`

🔒 **Requires Authentication**

Creates a new skin scan and triggers AI analysis.

**Request Body:**
```json
{
  "imageBase64": "data:image/jpeg;base64,/9j/4AAQ...",
  "scanType": "Full",
  "notes": "Morning scan"
}
```

**Response (200 OK):**
```json
{
  "scanId": "guid",
  "userId": "guid",
  "imageUrl": "/uploads/scans/scan-123.jpg",
  "scanType": "Full",
  "scanDate": "2024-01-15T10:30:00Z",
  "aiProcessingStatus": "Processing",
  "analysisResult": null
}
```

#### Get User Scans
**GET** `/api/Scans`

🔒 **Requires Authentication**

Returns all scans for the current user.

#### Get Scan by ID
**GET** `/api/Scans/{scanId}`

🔒 **Requires Authentication**

Returns a specific scan with analysis results.

**Response (200 OK):**
```json
{
  "scanId": "guid",
  "userId": "guid",
  "imageUrl": "/uploads/scans/scan-123.jpg",
  "scanType": "Full",
  "scanDate": "2024-01-15T10:30:00Z",
  "aiProcessingStatus": "Completed",
  "analysisResult": {
    "overallScore": 85,
    "hydrationLevel": 78,
    "oilinessLevel": 45,
    "elasticityScore": 82,
    "poresCondition": "Normal",
    "wrinkleLevel": 15,
    "darkSpotLevel": 20,
    "uvDamageLevel": 10,
    "skinTypeDetected": "Combination",
    "concerns": ["Mild dehydration", "T-zone oiliness"],
    "recommendations": ["Hydrating serum", "Oil-free moisturizer"]
  }
}
```

#### Get Daily Usage
**GET** `/api/Scans/daily-usage`

🔒 **Requires Authentication**

Returns today's scan usage and limits.

**Response (200 OK):**
```json
{
  "date": "2024-01-15",
  "scansUsed": 3,
  "scansLimit": 5,
  "scansRemaining": 2
}
```

#### Check Scan Availability
**GET** `/api/Scans/can-scan`

🔒 **Requires Authentication**

Checks if the user can perform a scan based on their plan.

**Response (200 OK):**
```json
{
  "canScan": true,
  "scansRemaining": 2,
  "reason": null
}
```

---

### Products (`/api/Products`)

#### Get All Products
**GET** `/api/Products`

Returns a paginated list of products with optional filters.

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20)
- `categoryId` (guid): Filter by category
- `brandId` (guid): Filter by brand
- `minPrice` (decimal): Minimum price
- `maxPrice` (decimal): Maximum price
- `rating` (decimal): Minimum rating
- `sortBy` (string): Sort field (price, rating, name)

**Response (200 OK):**
```json
{
  "items": [
    {
      "productId": "guid",
      "name": "Hydrating Face Serum",
      "description": "Deeply hydrating formula",
      "price": 29.99,
      "discountPercent": 15,
      "imageUrl": "/uploads/products/serum.jpg",
      "averageRating": 4.5,
      "reviewCount": 128,
      "categoryName": "Serum",
      "brandName": "SkinCare Co"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

#### Get Categories
**GET** `/api/Products/categories`

Returns all product categories.

#### Get Brands
**GET** `/api/Products/brands`

Returns all brands.

#### Get Favorites
**GET** `/api/Products/favorites`

🔒 **Requires Authentication**

Returns user's favorite products.

#### Add to Favorites
**POST** `/api/Products/favorites/{productId}`

🔒 **Requires Authentication**

#### Remove from Favorites
**DELETE** `/api/Products/favorites/{productId}`

🔒 **Requires Authentication**

#### Get Product Bundles
**GET** `/api/Products/bundles`

Returns all product bundles.

#### Get Scan Recommendations
**GET** `/api/Products/recommendations/scan/{scanId}`

🔒 **Requires Authentication**

Returns product recommendations based on a specific scan analysis.

#### Get Personalized Recommendations
**GET** `/api/Products/recommendations`

🔒 **Requires Authentication**

Returns personalized product recommendations based on user's skin profile.

---

### Routines (`/api/Routines`)

#### Get User Routines
**GET** `/api/Routines`

🔒 **Requires Authentication**

Returns all routines for the current user.

#### Create Routine
**POST** `/api/Routines`

🔒 **Requires Authentication**

**Request Body:**
```json
{
  "name": "Morning Routine",
  "description": "My daily morning skincare routine",
  "routineType": "Morning",
  "steps": [
    {
      "stepNumber": 1,
      "actionType": "Cleanse",
      "instructions": "Use gentle cleanser",
      "durationMinutes": 2,
      "productId": "guid"
    }
  ]
}
```

#### Complete Routine
**POST** `/api/Routines/{routineId}/complete`

🔒 **Requires Authentication**

Marks a routine as completed for today.

#### Get Reminders
**GET** `/api/Routines/reminders`

🔒 **Requires Authentication**

Returns routine reminders for the current user.

---

### Community (`/api/Community`)

#### Get Feed
**GET** `/api/Community/feed`

Returns the community feed with posts.

**Query Parameters:**
- `page` (int): Page number
- `pageSize` (int): Items per page
- `stationId` (guid): Filter by creator station

#### Create Post
**POST** `/api/Community/posts`

🔒 **Requires Authentication**

**Request Body:**
```json
{
  "title": "My Skincare Journey",
  "content": "Here's my experience...",
  "contentType": "Article",
  "imageBase64": "data:image/jpeg;base64,/9j/4AAQ...",
  "hashtags": ["skincare", "routine"]
}
```

#### Like Post
**POST** `/api/Community/posts/{postId}/like`

🔒 **Requires Authentication**

#### Comment on Post
**POST** `/api/Community/posts/{postId}/comments`

🔒 **Requires Authentication**

**Request Body:**
```json
{
  "content": "Great post!",
  "parentCommentId": null
}
```

#### Get Creator Stations
**GET** `/api/Community/stations`

Returns all creator stations.

#### Get Station by ID
**GET** `/api/Community/stations/{stationId}`

Returns a specific creator station with posts.

#### Follow Station
**POST** `/api/Community/stations/{stationId}/follow`

🔒 **Requires Authentication**

---

### Subscriptions (`/api/Subscriptions`)

#### Get Plans
**GET** `/api/Subscriptions/plans`

Returns all available subscription plans.

**Response (200 OK):**
```json
[
  {
    "planId": "guid",
    "planName": "Member",
    "billingPeriod": "Monthly",
    "priceAmount": 9.99,
    "dailyScansLimit": 5,
    "features": ["5 scans per day", "Progress tracking", "Routine reminders"]
  },
  {
    "planId": "guid",
    "planName": "Pro",
    "billingPeriod": "Monthly",
    "priceAmount": 29.99,
    "dailyScansLimit": null,
    "features": ["Unlimited scans", "Creator Station", "Advanced Analytics"]
  }
]
```

#### Subscribe
**POST** `/api/Subscriptions/subscribe`

🔒 **Requires Authentication**

**Request Body:**
```json
{
  "planId": "guid",
  "paymentMethodId": "pm_xxx"
}
```

#### Get Current Subscription
**GET** `/api/Subscriptions/current`

🔒 **Requires Authentication**

#### Cancel Subscription
**POST** `/api/Subscriptions/cancel`

🔒 **Requires Authentication**

#### Get Wallet Info
**GET** `/api/Subscriptions/wallet`

🔒 **Requires Authentication**

**Response (200 OK):**
```json
{
  "balance": 50.00,
  "transactions": [
    {
      "transactionId": "guid",
      "amount": 50.00,
      "type": "Credit",
      "description": "Added funds",
      "createdAt": "2024-01-15T10:00:00Z"
    }
  ]
}
```

#### Add Funds
**POST** `/api/Subscriptions/wallet/add-funds`

🔒 **Requires Authentication**

**Request Body:**
```json
{
  "amount": 50.00,
  "paymentMethodId": "pm_xxx"
}
```

---

### Notifications (`/api/Notifications`)

#### Get Notifications
**GET** `/api/Notifications`

🔒 **Requires Authentication**

Returns user notifications.

**Query Parameters:**
- `unreadOnly` (bool): Filter to unread only

#### Mark as Read
**PUT** `/api/Notifications/{notificationId}/read`

🔒 **Requires Authentication**

#### Mark All as Read
**PUT** `/api/Notifications/read-all`

🔒 **Requires Authentication**

#### Get Achievements
**GET** `/api/Notifications/achievements`

🔒 **Requires Authentication**

Returns user's earned achievements.

---

### Chat (`/api/Chat`)

#### Get Conversations
**GET** `/api/Chat/conversations`

🔒 **Requires Authentication**

Returns user's chat conversations.

#### Get Messages
**GET** `/api/Chat/conversations/{conversationId}/messages`

🔒 **Requires Authentication**

#### Send Message
**POST** `/api/Chat/messages`

🔒 **Requires Authentication**

**Request Body:**
```json
{
  "receiverId": "guid",
  "content": "Hello!",
  "mediaBase64": null
}
```

---

## Error Responses

All endpoints may return the following error responses:

### 400 Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "email": ["The Email field is required."]
  }
}
```

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### 403 Forbidden
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Pro subscription required"
}
```

### 404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

### 500 Internal Server Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500
}
```

---

## Database Schema

The API uses SQL Server LocalDB with the following main entities:

### Users & Authentication
- **Users** - User accounts and profiles
- **RefreshTokens** - JWT refresh tokens
- **Roles** - User roles (Admin, Moderator, User, Creator)
- **UserRoles** - User-role mappings
- **SkinProfiles** - User skin profiles

### Subscriptions & Payments
- **SubscriptionPlans** - Available subscription plans
- **UserSubscriptions** - User subscriptions
- **PaymentTransactions** - Payment history
- **WalletTransactions** - Wallet transactions

### Skin Scanning
- **SkinScans** - Skin scan records
- **SkinAnalysisResults** - AI analysis results
- **DailyScanUsages** - Daily scan usage tracking

### Products
- **Brands** - Product brands
- **Distributors** - Product distributors
- **ProductCategories** - Product categories
- **Products** - Product catalog
- **ProductBundles** - Product bundles
- **ProductRecommendations** - Scan-based recommendations
- **UserProductFavorites** - User favorites

### Routines
- **UserRoutines** - User skincare routines
- **RoutineSteps** - Routine steps
- **RoutineCompletions** - Routine completion records
- **RoutineReminders** - Routine reminders

### Community
- **CreatorStations** - Creator stations
- **StationFollowers** - Station followers
- **CommunityPosts** - Community posts
- **PostLikes** - Post likes
- **PostComments** - Post comments
- **BrandCampaigns** - Brand campaigns
- **CampaignParticipants** - Campaign participants

### Achievements & Notifications
- **Achievements** - Available achievements
- **UserAchievements** - User earned achievements
- **Notifications** - User notifications

### Chat
- **ChatMessages** - Direct messages

---

## File Storage

Files are stored in the `Uploads` folder:

```
Uploads/
├── scans/          # Skin scan images
├── profiles/       # Profile pictures
├── posts/          # Community post images
└── products/       # Product images
```

Images are accessible via static file serving at `/uploads/{path}`.

---

## Configuration

### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(LocalDb)\\MSSQLLocalDB;Database=SkinPAI_Dev;..."
  }
}
```

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "SkinPAI.API",
    "Audience": "SkinPAI.Mobile"
  }
}
```

### CORS Origins
The API allows requests from:
- `http://localhost:5173`
- `http://localhost:3000`
- `http://127.0.0.1:5173`
- `https://skinpai.app`

---

## Running the API

### Development
```bash
cd SkinPAI.API
dotnet run
```

### Production
```bash
dotnet publish -c Release
dotnet SkinPAI.API.dll
```

### Migrations
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

---

## License

Copyright © 2024 SkinPAI. All rights reserved.
