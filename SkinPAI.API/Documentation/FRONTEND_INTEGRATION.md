# Frontend API Integration Guide

This guide shows how to integrate the SkinPAI mobile frontend with the .NET backend API.

## Setup

### 1. Create API Service

Create `src/services/api.ts`:

```typescript
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

interface ApiResponse<T> {
  data: T;
  error?: string;
}

class ApiService {
  private accessToken: string | null = null;
  private refreshToken: string | null = null;

  constructor() {
    // Load tokens from storage
    this.accessToken = localStorage.getItem('accessToken');
    this.refreshToken = localStorage.getItem('refreshToken');
  }

  setTokens(accessToken: string, refreshToken: string) {
    this.accessToken = accessToken;
    this.refreshToken = refreshToken;
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  }

  clearTokens() {
    this.accessToken = null;
    this.refreshToken = null;
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    const url = `${API_BASE_URL}${endpoint}`;
    
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    if (this.accessToken) {
      headers['Authorization'] = `Bearer ${this.accessToken}`;
    }

    try {
      const response = await fetch(url, {
        ...options,
        headers,
      });

      if (response.status === 401) {
        // Try to refresh token
        const refreshed = await this.refreshAccessToken();
        if (refreshed) {
          // Retry the request
          headers['Authorization'] = `Bearer ${this.accessToken}`;
          const retryResponse = await fetch(url, { ...options, headers });
          const data = await retryResponse.json();
          return { data };
        } else {
          this.clearTokens();
          window.location.href = '/login';
          return { data: null as T, error: 'Session expired' };
        }
      }

      if (!response.ok) {
        const errorData = await response.json();
        return { data: null as T, error: errorData.title || 'Request failed' };
      }

      const data = await response.json();
      return { data };
    } catch (error) {
      return { data: null as T, error: 'Network error' };
    }
  }

  private async refreshAccessToken(): Promise<boolean> {
    if (!this.refreshToken) return false;

    try {
      const response = await fetch(`${API_BASE_URL}/Auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: this.refreshToken }),
      });

      if (response.ok) {
        const data = await response.json();
        this.setTokens(data.accessToken, data.refreshToken);
        return true;
      }
      return false;
    } catch {
      return false;
    }
  }

  // Auth endpoints
  async register(data: RegisterRequest): Promise<ApiResponse<AuthResponse>> {
    const response = await this.request<AuthResponse>('/Auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (response.data?.accessToken) {
      this.setTokens(response.data.accessToken, response.data.refreshToken);
    }
    return response;
  }

  async login(email: string, password: string): Promise<ApiResponse<AuthResponse>> {
    const response = await this.request<AuthResponse>('/Auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
    if (response.data?.accessToken) {
      this.setTokens(response.data.accessToken, response.data.refreshToken);
    }
    return response;
  }

  async guestLogin(): Promise<ApiResponse<AuthResponse>> {
    const response = await this.request<AuthResponse>('/Auth/guest', {
      method: 'POST',
    });
    if (response.data?.accessToken) {
      this.setTokens(response.data.accessToken, response.data.refreshToken);
    }
    return response;
  }

  async logout(): Promise<void> {
    await this.request('/Auth/logout', { method: 'POST' });
    this.clearTokens();
  }

  // User endpoints
  async getCurrentUser(): Promise<ApiResponse<UserDto>> {
    return this.request<UserDto>('/Users/me');
  }

  async updateProfile(data: UpdateUserRequest): Promise<ApiResponse<UserDto>> {
    return this.request<UserDto>('/Users/me', {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  async getMemberDashboard(): Promise<ApiResponse<MemberDashboardDto>> {
    return this.request<MemberDashboardDto>('/Users/dashboard/member');
  }

  async getProDashboard(): Promise<ApiResponse<ProDashboardDto>> {
    return this.request<ProDashboardDto>('/Users/dashboard/pro');
  }

  // Scan endpoints
  async createScan(imageBase64: string, scanType: string, notes?: string): Promise<ApiResponse<SkinScanDto>> {
    return this.request<SkinScanDto>('/Scans', {
      method: 'POST',
      body: JSON.stringify({ imageBase64, scanType, notes }),
    });
  }

  async getScans(): Promise<ApiResponse<SkinScanDto[]>> {
    return this.request<SkinScanDto[]>('/Scans');
  }

  async getScan(scanId: string): Promise<ApiResponse<SkinScanDto>> {
    return this.request<SkinScanDto>(`/Scans/${scanId}`);
  }

  async getDailyUsage(): Promise<ApiResponse<DailyScanUsageDto>> {
    return this.request<DailyScanUsageDto>('/Scans/daily-usage');
  }

  async canScan(): Promise<ApiResponse<{ canScan: boolean; scansRemaining: number; reason?: string }>> {
    return this.request('/Scans/can-scan');
  }

  // Product endpoints
  async getProducts(params?: ProductFilterRequest): Promise<ApiResponse<PaginatedResponse<ProductDto>>> {
    const queryString = params ? '?' + new URLSearchParams(params as any).toString() : '';
    return this.request<PaginatedResponse<ProductDto>>(`/Products${queryString}`);
  }

  async getCategories(): Promise<ApiResponse<ProductCategoryDto[]>> {
    return this.request<ProductCategoryDto[]>('/Products/categories');
  }

  async getBrands(): Promise<ApiResponse<BrandDto[]>> {
    return this.request<BrandDto[]>('/Products/brands');
  }

  async getFavorites(): Promise<ApiResponse<ProductDto[]>> {
    return this.request<ProductDto[]>('/Products/favorites');
  }

  async addFavorite(productId: string): Promise<ApiResponse<void>> {
    return this.request<void>(`/Products/favorites/${productId}`, { method: 'POST' });
  }

  async removeFavorite(productId: string): Promise<ApiResponse<void>> {
    return this.request<void>(`/Products/favorites/${productId}`, { method: 'DELETE' });
  }

  async getBundles(): Promise<ApiResponse<ProductBundleDto[]>> {
    return this.request<ProductBundleDto[]>('/Products/bundles');
  }

  async getScanRecommendations(scanId: string): Promise<ApiResponse<ProductDto[]>> {
    return this.request<ProductDto[]>(`/Products/recommendations/scan/${scanId}`);
  }

  async getPersonalizedRecommendations(): Promise<ApiResponse<ProductDto[]>> {
    return this.request<ProductDto[]>('/Products/recommendations');
  }

  // Routine endpoints
  async getRoutines(): Promise<ApiResponse<RoutineDto[]>> {
    return this.request<RoutineDto[]>('/Routines');
  }

  async createRoutine(data: CreateRoutineRequest): Promise<ApiResponse<RoutineDto>> {
    return this.request<RoutineDto>('/Routines', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async completeRoutine(routineId: string, notes?: string): Promise<ApiResponse<void>> {
    return this.request<void>(`/Routines/${routineId}/complete`, {
      method: 'POST',
      body: JSON.stringify({ notes }),
    });
  }

  async getReminders(): Promise<ApiResponse<RoutineReminderDto[]>> {
    return this.request<RoutineReminderDto[]>('/Routines/reminders');
  }

  // Community endpoints
  async getCommunityFeed(page = 1, pageSize = 20, stationId?: string): Promise<ApiResponse<PaginatedResponse<CommunityPostDto>>> {
    let url = `/Community/feed?page=${page}&pageSize=${pageSize}`;
    if (stationId) url += `&stationId=${stationId}`;
    return this.request<PaginatedResponse<CommunityPostDto>>(url);
  }

  async createPost(data: CreatePostRequest): Promise<ApiResponse<CommunityPostDto>> {
    return this.request<CommunityPostDto>('/Community/posts', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async likePost(postId: string): Promise<ApiResponse<void>> {
    return this.request<void>(`/Community/posts/${postId}/like`, { method: 'POST' });
  }

  async commentOnPost(postId: string, content: string, parentCommentId?: string): Promise<ApiResponse<PostCommentDto>> {
    return this.request<PostCommentDto>(`/Community/posts/${postId}/comments`, {
      method: 'POST',
      body: JSON.stringify({ content, parentCommentId }),
    });
  }

  async getStations(): Promise<ApiResponse<CreatorStationDto[]>> {
    return this.request<CreatorStationDto[]>('/Community/stations');
  }

  async getStation(stationId: string): Promise<ApiResponse<CreatorStationDto>> {
    return this.request<CreatorStationDto>(`/Community/stations/${stationId}`);
  }

  async followStation(stationId: string): Promise<ApiResponse<void>> {
    return this.request<void>(`/Community/stations/${stationId}/follow`, { method: 'POST' });
  }

  // Subscription endpoints
  async getPlans(): Promise<ApiResponse<SubscriptionPlanDto[]>> {
    return this.request<SubscriptionPlanDto[]>('/Subscriptions/plans');
  }

  async subscribe(planId: string, paymentMethodId: string): Promise<ApiResponse<UserSubscriptionDto>> {
    return this.request<UserSubscriptionDto>('/Subscriptions/subscribe', {
      method: 'POST',
      body: JSON.stringify({ planId, paymentMethodId }),
    });
  }

  async getCurrentSubscription(): Promise<ApiResponse<UserSubscriptionDto>> {
    return this.request<UserSubscriptionDto>('/Subscriptions/current');
  }

  async cancelSubscription(): Promise<ApiResponse<void>> {
    return this.request<void>('/Subscriptions/cancel', { method: 'POST' });
  }

  async getWalletInfo(): Promise<ApiResponse<WalletInfoDto>> {
    return this.request<WalletInfoDto>('/Subscriptions/wallet');
  }

  async addFunds(amount: number, paymentMethodId: string): Promise<ApiResponse<void>> {
    return this.request<void>('/Subscriptions/wallet/add-funds', {
      method: 'POST',
      body: JSON.stringify({ amount, paymentMethodId }),
    });
  }

  // Notification endpoints
  async getNotifications(unreadOnly = false): Promise<ApiResponse<NotificationDto[]>> {
    return this.request<NotificationDto[]>(`/Notifications?unreadOnly=${unreadOnly}`);
  }

  async markNotificationRead(notificationId: string): Promise<ApiResponse<void>> {
    return this.request<void>(`/Notifications/${notificationId}/read`, { method: 'PUT' });
  }

  async markAllNotificationsRead(): Promise<ApiResponse<void>> {
    return this.request<void>('/Notifications/read-all', { method: 'PUT' });
  }

  async getAchievements(): Promise<ApiResponse<UserAchievementDto[]>> {
    return this.request<UserAchievementDto[]>('/Notifications/achievements');
  }

  // Chat endpoints
  async getConversations(): Promise<ApiResponse<ChatConversationDto[]>> {
    return this.request<ChatConversationDto[]>('/Chat/conversations');
  }

  async getMessages(conversationId: string): Promise<ApiResponse<ChatMessageDto[]>> {
    return this.request<ChatMessageDto[]>(`/Chat/conversations/${conversationId}/messages`);
  }

  async sendMessage(receiverId: string, content: string, mediaBase64?: string): Promise<ApiResponse<ChatMessageDto>> {
    return this.request<ChatMessageDto>('/Chat/messages', {
      method: 'POST',
      body: JSON.stringify({ receiverId, content, mediaBase64 }),
    });
  }
}

export const api = new ApiService();
export default api;
```

### 2. TypeScript Interfaces

Create `src/types/api.ts`:

```typescript
export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  dateOfBirth?: string;
  gender?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: UserDto;
}

export interface UserDto {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  profileImageUrl?: string;
  membershipType: string;
  membershipStatus: string;
  isCreator: boolean;
  isVerified: boolean;
  walletBalance: number;
  totalScansUsed: number;
}

export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  bio?: string;
  profileImageBase64?: string;
}

export interface SkinScanDto {
  scanId: string;
  userId: string;
  imageUrl: string;
  scanType: string;
  scanDate: string;
  aiProcessingStatus: string;
  analysisResult?: SkinAnalysisResultDto;
}

export interface SkinAnalysisResultDto {
  overallScore: number;
  hydrationLevel: number;
  oilinessLevel: number;
  elasticityScore: number;
  poresCondition: string;
  wrinkleLevel: number;
  darkSpotLevel: number;
  uvDamageLevel: number;
  skinTypeDetected: string;
  concerns: string[];
  recommendations: string[];
}

export interface DailyScanUsageDto {
  date: string;
  scansUsed: number;
  scansLimit: number;
  scansRemaining: number;
}

export interface ProductDto {
  productId: string;
  name: string;
  description?: string;
  price: number;
  discountPercent?: number;
  imageUrl?: string;
  averageRating?: number;
  reviewCount: number;
  categoryName: string;
  brandName: string;
}

export interface ProductFilterRequest {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  brandId?: string;
  minPrice?: number;
  maxPrice?: number;
  rating?: number;
  sortBy?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ProductCategoryDto {
  categoryId: string;
  categoryName: string;
  categoryIcon: string;
  displayOrder: number;
}

export interface BrandDto {
  brandId: string;
  brandName: string;
  logoUrl?: string;
}

export interface ProductBundleDto {
  bundleId: string;
  name: string;
  description?: string;
  bundlePrice: number;
  originalPrice: number;
  savings: number;
  imageUrl?: string;
  products: ProductSummaryDto[];
}

export interface ProductSummaryDto {
  productId: string;
  name: string;
  imageUrl?: string;
  price: number;
}

export interface RoutineDto {
  routineId: string;
  name: string;
  description?: string;
  routineType: string;
  isActive: boolean;
  steps: RoutineStepDto[];
}

export interface RoutineStepDto {
  stepId: number;
  stepNumber: number;
  actionType: string;
  instructions?: string;
  durationMinutes?: number;
  product?: ProductSummaryDto;
}

export interface CreateRoutineRequest {
  name: string;
  description?: string;
  routineType: string;
  steps: CreateRoutineStepRequest[];
}

export interface CreateRoutineStepRequest {
  stepNumber: number;
  actionType: string;
  instructions?: string;
  durationMinutes?: number;
  productId?: string;
}

export interface RoutineReminderDto {
  reminderId: number;
  reminderTime: string;
  daysOfWeek: string[];
  isEnabled: boolean;
}

export interface CommunityPostDto {
  postId: string;
  userId: string;
  authorName: string;
  authorImageUrl?: string;
  title: string;
  content?: string;
  contentType: string;
  imageUrl?: string;
  videoUrl?: string;
  likeCount: number;
  commentCount: number;
  shareCount: number;
  isLiked: boolean;
  publishedAt: string;
  hashtags: string[];
}

export interface CreatePostRequest {
  title: string;
  content?: string;
  contentType: string;
  imageBase64?: string;
  hashtags?: string[];
}

export interface PostCommentDto {
  commentId: number;
  userId: string;
  authorName: string;
  authorImageUrl?: string;
  content: string;
  createdAt: string;
  replies?: PostCommentDto[];
}

export interface CreatorStationDto {
  stationId: string;
  userId: string;
  stationName: string;
  stationSlug: string;
  description?: string;
  bannerImageUrl?: string;
  profileImageUrl?: string;
  followersCount: number;
  isVerified: boolean;
  isFollowing: boolean;
  categories: string[];
  recentPosts?: CommunityPostDto[];
}

export interface SubscriptionPlanDto {
  planId: string;
  planName: string;
  billingPeriod: string;
  priceAmount: number;
  dailyScansLimit?: number;
  features: string[];
}

export interface UserSubscriptionDto {
  subscriptionId: string;
  planName: string;
  status: string;
  startDate: string;
  endDate?: string;
  billingPeriod: string;
  priceAmount: number;
}

export interface WalletInfoDto {
  balance: number;
  transactions: WalletTransactionDto[];
}

export interface WalletTransactionDto {
  transactionId: string;
  amount: number;
  type: string;
  description?: string;
  createdAt: string;
}

export interface NotificationDto {
  notificationId: number;
  title: string;
  body?: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  relatedEntityId?: string;
}

export interface UserAchievementDto {
  achievementId: string;
  title: string;
  description?: string;
  icon: string;
  achievementType: string;
  points: number;
  unlockedAt: string;
  progress: number;
  requiredProgress: number;
}

export interface MemberDashboardDto {
  user: UserDto;
  currentSubscription?: UserSubscriptionDto;
  todaysScanUsage: DailyScanUsageDto;
  achievements: UserAchievementDto[];
  recentScans: SkinScanDto[];
  routines: RoutineDto[];
  skinProgress?: SkinProgressDto;
}

export interface ProDashboardDto extends MemberDashboardDto {
  station?: CreatorStationDto;
  earnings: number;
  audienceGrowth: number;
  topPosts: CommunityPostDto[];
  activeCampaigns: BrandCampaignDto[];
}

export interface SkinProgressDto {
  currentScore: number;
  previousScore: number;
  improvement: number;
  weeklyScores: number[];
}

export interface BrandCampaignDto {
  campaignId: string;
  brandName: string;
  title: string;
  description?: string;
  reward: number;
  status: string;
}

export interface ChatConversationDto {
  conversationId: string;
  otherUserId: string;
  otherUserName: string;
  otherUserImageUrl?: string;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount: number;
}

export interface ChatMessageDto {
  messageId: number;
  senderId: string;
  receiverId: string;
  content: string;
  mediaUrl?: string;
  sentAt: string;
  isRead: boolean;
}
```

### 3. Environment Configuration

Create `.env`:

```env
VITE_API_URL=http://localhost:5000/api
```

For production, create `.env.production`:

```env
VITE_API_URL=https://api.skinpai.app/api
```

### 4. Example Usage in Components

#### AuthScreen.tsx

```tsx
import { api } from '../services/api';
import { useState } from 'react';

export function AuthScreen() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async () => {
    setLoading(true);
    setError('');
    
    const { data, error } = await api.login(email, password);
    
    if (error) {
      setError(error);
    } else if (data) {
      // Navigate to dashboard
      window.location.href = '/dashboard';
    }
    
    setLoading(false);
  };

  const handleGuestLogin = async () => {
    setLoading(true);
    const { data, error } = await api.guestLogin();
    
    if (data) {
      window.location.href = '/dashboard';
    }
    setLoading(false);
  };

  return (
    <div>
      <input 
        type="email" 
        value={email} 
        onChange={(e) => setEmail(e.target.value)} 
        placeholder="Email"
      />
      <input 
        type="password" 
        value={password} 
        onChange={(e) => setPassword(e.target.value)} 
        placeholder="Password"
      />
      {error && <p className="error">{error}</p>}
      <button onClick={handleLogin} disabled={loading}>
        {loading ? 'Loading...' : 'Login'}
      </button>
      <button onClick={handleGuestLogin} disabled={loading}>
        Continue as Guest
      </button>
    </div>
  );
}
```

#### CameraInterface.tsx

```tsx
import { api } from '../services/api';
import { useState } from 'react';

export function CameraInterface() {
  const [canScan, setCanScan] = useState(true);
  const [scanning, setScanning] = useState(false);

  const checkScanAvailability = async () => {
    const { data } = await api.canScan();
    if (data) {
      setCanScan(data.canScan);
    }
  };

  const handleCapture = async (imageBase64: string) => {
    setScanning(true);
    
    const { data, error } = await api.createScan(imageBase64, 'Full');
    
    if (data) {
      // Navigate to results
      window.location.href = `/scan-results/${data.scanId}`;
    }
    
    setScanning(false);
  };

  return (
    <div>
      {/* Camera UI */}
      <button 
        onClick={() => handleCapture(/* captured image */)} 
        disabled={!canScan || scanning}
      >
        {scanning ? 'Analyzing...' : 'Capture'}
      </button>
    </div>
  );
}
```

#### ScanResults.tsx

```tsx
import { api } from '../services/api';
import { useState, useEffect } from 'react';
import { SkinScanDto, ProductDto } from '../types/api';

export function ScanResults({ scanId }: { scanId: string }) {
  const [scan, setScan] = useState<SkinScanDto | null>(null);
  const [recommendations, setRecommendations] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadData = async () => {
      const [scanResponse, recsResponse] = await Promise.all([
        api.getScan(scanId),
        api.getScanRecommendations(scanId),
      ]);
      
      if (scanResponse.data) setScan(scanResponse.data);
      if (recsResponse.data) setRecommendations(recsResponse.data);
      
      setLoading(false);
    };
    
    loadData();
  }, [scanId]);

  if (loading) return <div>Loading...</div>;

  return (
    <div>
      {scan?.analysisResult && (
        <div>
          <h2>Overall Score: {scan.analysisResult.overallScore}</h2>
          <p>Skin Type: {scan.analysisResult.skinTypeDetected}</p>
          <p>Hydration: {scan.analysisResult.hydrationLevel}%</p>
          
          <h3>Concerns</h3>
          <ul>
            {scan.analysisResult.concerns.map((concern, i) => (
              <li key={i}>{concern}</li>
            ))}
          </ul>
        </div>
      )}
      
      <h3>Recommended Products</h3>
      <div className="products-grid">
        {recommendations.map((product) => (
          <div key={product.productId} className="product-card">
            <img src={product.imageUrl} alt={product.name} />
            <h4>{product.name}</h4>
            <p>${product.price}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
```

---

## Migrating from Mock Data

To replace mock data with API calls:

1. **Remove mock data files** (`src/data/mockData.ts`, etc.)
2. **Replace hooks** to use API service instead of local state
3. **Add loading states** for async operations
4. **Handle errors** gracefully with user feedback

Example - Replace mock products:

```tsx
// Before (mock data)
import { mockProducts } from '../data/mockData';
const products = mockProducts;

// After (API)
import { api } from '../services/api';
import { useEffect, useState } from 'react';

const [products, setProducts] = useState([]);
const [loading, setLoading] = useState(true);

useEffect(() => {
  api.getProducts().then(({ data }) => {
    if (data) setProducts(data.items);
    setLoading(false);
  });
}, []);
```

---

## CORS Configuration

The API is configured to accept requests from:
- `http://localhost:5173` (Vite dev server)
- `http://localhost:3000` (alternative dev server)
- `http://127.0.0.1:5173`
- `https://skinpai.app` (production)

If you need to add more origins, update `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:3000",
            "https://your-domain.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
```

---

## Testing

Test the integration using the API documentation and Swagger UI:

1. Start the API: `dotnet run` (in SkinPAI.API directory)
2. Open Swagger: http://localhost:5000/swagger
3. Start the frontend: `npm run dev` (in Skinpaimobile-main directory)
4. Test endpoints through the UI

## Troubleshooting

### Common Issues

**401 Unauthorized**
- Token expired - will auto-refresh
- Token missing - redirect to login

**CORS Error**
- Check API is running
- Verify origin is in allowed list

**Network Error**
- Check API URL in `.env`
- Ensure API is running on correct port

**500 Server Error**
- Check API logs
- Verify database connection
