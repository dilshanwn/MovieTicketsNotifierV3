# Product Requirements Document (PRD)
## Scope Ticket Notifier V3.0 Web Application

---

### **1. Project Overview**

**Product Name:** Scope Ticket Notifier V3.0  
**Project Type:** Next.js Web Application with Azure Functions Backend  
**Target Audience:** Movie enthusiasts who want to be notified when Scope Cinemas releases tickets for movies  
**Platform:** Web Application (Desktop & Mobile Responsive)

### **2. Product Vision & Goals**

**Vision:** Provide users with a seamless web interface to register for and manage movie ticket release notifications from Scope Cinemas.

**Primary Goals:**
- Enable users to register for notifications when Scope Cinemas releases tickets for specific movies
- Provide a user-friendly dashboard to manage registered alerts
- Display current movie information with posters and details
- Integrate with existing Azure Functions backend and Supabase database

**Success Metrics:**
- User registration and retention
- Number of active alerts registered
- User engagement with movie listings
- Successful notification delivery rates

---

### **3. Technical Architecture**

#### **3.1 Frontend Technology Stack**
- **Framework:** Next.js 14+ (App Router)
- **Styling:** Tailwind CSS with dark theme
- **UI Components:** Shadcn/ui components
- **Authentication:** Supabase Auth
- **State Management:** React Context/Zustand
- **HTTP Client:** Axios/fetch
- **Image Optimization:** Next.js Image component

#### **3.2 Backend Technology Stack**
- **Existing:** Azure Functions (.NET 8)
- **Database:** Supabase (PostgreSQL)
- **Authentication:** Supabase Auth
- **External API:** Scope Cinemas API

#### **3.3 Environment Variables Required**

**Supabase Configuration:**
- `NEXT_PUBLIC_SUPABASE_URL`
- `NEXT_PUBLIC_SUPABASE_ANON_KEY`
- `SUPABASE_SERVICE_ROLE_KEY`

**Scope API Configuration:**
- `SCOPE_BASE_URL`
- `SCOPE_CLIENT_ID`
- `SCOPE_CLIENT_SECRET`
- `SCOPE_USER_KEY`
- `SCOPE_TOKEN_URL`
- `SCOPE_NOW_SHOWING_MOVIES_URL`
- `SCOPE_UPCOMING_MOVIES_URL`
- `SCOPE_MOVIE_SHOW_TIMES_URL`

**Azure Functions:**
- `AZURE_FUNCTIONS_BASE_URL`

---

### **4. Database Schema Reference**

#### **4.1 Existing Supabase Tables**

**registered_alerts_by_name:**
```sql
- id: bigint (auto-generated)
- created_at: timestamp with time zone
- active: boolean (default: true)
- email: text (references authenticated user)
- movie_name: text
- location: locations enum (HCM, CCC, LIBERTY)
- experiance: experiance_types[] (IMAX, GOLD, ATMOS, Digital)
- date: date
```

**registered_alerts_by_id:**
```sql
- id: bigint (auto-generated)
- created_at: timestamp with time zone
- active: boolean (default: true)
- email: text (references authenticated user)
- movie_id: text
- location: locations enum (HCM, CCC, LIBERTY)
- experiance: experiance_types[] (IMAX, GOLD, ATMOS, Digital)
- date: date
```

#### **4.2 User Authentication & Data Isolation**
- Uses Supabase Auth with email/password authentication
- Row Level Security (RLS) enabled on alert tables
- **Data Isolation Rules:**
  - Users can only access alerts where `email` column matches their authenticated email
  - All queries automatically filter by user's email address
  - Create operations automatically set the email field to the authenticated user's email
  - Update/Delete operations validate ownership before execution
- **RLS Policies Required:**
  - `registered_alerts_by_name`: `auth.jwt() ->> 'email' = email`
  - `registered_alerts_by_id`: `auth.jwt() ->> 'email' = email`

#### **4.3 Required Database RLS Policies**

**For registered_alerts_by_name table:**
```sql
-- Enable RLS
ALTER TABLE registered_alerts_by_name ENABLE ROW LEVEL SECURITY;

-- Policy for SELECT (users can only see their own alerts)
CREATE POLICY "Users can view own alerts by name" ON registered_alerts_by_name
FOR SELECT USING (auth.jwt() ->> 'email' = email);

-- Policy for INSERT (users can only create alerts with their email)
CREATE POLICY "Users can insert own alerts by name" ON registered_alerts_by_name
FOR INSERT WITH CHECK (auth.jwt() ->> 'email' = email);

-- Policy for UPDATE (users can only update their own alerts)
CREATE POLICY "Users can update own alerts by name" ON registered_alerts_by_name
FOR UPDATE USING (auth.jwt() ->> 'email' = email);

-- Policy for DELETE (users can only delete their own alerts)
CREATE POLICY "Users can delete own alerts by name" ON registered_alerts_by_name
FOR DELETE USING (auth.jwt() ->> 'email' = email);
```

**For registered_alerts_by_id table:**
```sql
-- Enable RLS
ALTER TABLE registered_alerts_by_id ENABLE ROW LEVEL SECURITY;

-- Policy for SELECT (users can only see their own alerts)
CREATE POLICY "Users can view own alerts by id" ON registered_alerts_by_id
FOR SELECT USING (auth.jwt() ->> 'email' = email);

-- Policy for INSERT (users can only create alerts with their email)
CREATE POLICY "Users can insert own alerts by id" ON registered_alerts_by_id
FOR INSERT WITH CHECK (auth.jwt() ->> 'email' = email);

-- Policy for UPDATE (users can only update their own alerts)
CREATE POLICY "Users can update own alerts by id" ON registered_alerts_by_id
FOR UPDATE USING (auth.jwt() ->> 'email' = email);

-- Policy for DELETE (users can only delete their own alerts)
CREATE POLICY "Users can delete own alerts by id" ON registered_alerts_by_id
FOR DELETE USING (auth.jwt() ->> 'email' = email);
```

---

### **5. Feature Requirements**

#### **5.1 Authentication System**
**Requirements:**
- Email/password login form with validation
- User registration with email verification
- Password reset functionality
- Session management with automatic logout
- Protected routes - all application features require authentication
- Persistent login state across browser sessions
- Secure logout functionality

**User Stories:**
- As a user, I want to create an account so I can register for movie notifications
- As a user, I want to log in securely to access my alerts
- As a user, I want to reset my password if I forget it
- As a user, I want the app to remember my login across sessions
- As a visitor, I should not be able to access any app features without logging in

#### **5.2 Dashboard - Registered Alerts**
**Requirements:**
- Display all user's registered alerts (both by name and ID) filtered by user's email
- Show alert status (active/inactive)
- Allow editing/deleting alerts with confirmation dialogs
- Allow bulk deletion of multiple alerts
- Display movie information with posters when available
- Filter and search functionality within user's own alerts
- Real-time updates when alerts are modified or deleted

**User Stories:**
- As a user, I want to see only my registered alerts in one place
- As a user, I want to edit or delete my alerts with confirmation
- As a user, I want to see movie posters and details for my alerts
- As a user, I want to delete multiple alerts at once
- As a user, I want immediate feedback when I delete an alert

#### **5.3 Movie Browsing**
**Requirements:**
- Display now showing movies from Scope API
- Display upcoming movies from Scope API
- Show movie posters, titles, genres, runtime, IMDB ratings
- Movie detail view with synopsis, cast, trailer
- Responsive grid layout

**User Stories:**
- As a user, I want to browse current and upcoming movies
- As a user, I want to see detailed information about movies
- As a user, I want to register for alerts directly from movie listings

#### **5.4 Alert Registration**
**Requirements:**
- Register alerts by movie name or specific movie ID
- Select cinema location (HCM, CCC, LIBERTY)
- Select movie experience types (IMAX, GOLD, ATMOS, Digital)
- Select preferred dates
- Form validation and error handling
- Auto-populate user email from authenticated session
- Prevent duplicate alerts for same movie/date/location/experience combination

**User Stories:**
- As a user, I want to register for notifications for specific movies
- As a user, I want to choose my preferred cinema location and experience
- As a user, I want to select specific dates for notifications
- As a user, I want to be prevented from creating duplicate alerts

#### **5.5 Alert Deletion & Management**
**Requirements:**
- Delete individual alerts with confirmation dialog
- Bulk delete multiple alerts with confirmation
- Soft delete option (mark as inactive) vs hard delete
- Undo functionality for accidental deletions (5-second window)
- Visual feedback during delete operations
- Optimistic UI updates with rollback on failure

**User Stories:**
- As a user, I want to delete alerts I no longer need
- As a user, I want confirmation before deleting alerts to prevent accidents
- As a user, I want to delete multiple alerts at once to save time
- As a user, I want to undo accidental deletions immediately
- As a user, I want clear feedback when alerts are being deleted

---

### **6. Page Structure & Navigation**

#### **6.1 Public Pages**
- `/` - Landing page with app information and login prompt
- `/login` - Authentication page (redirects to dashboard if already logged in)
- `/register` - User registration page (redirects to dashboard if already logged in)
- `/forgot-password` - Password reset page

**Note:** All other pages require authentication. Unauthenticated users will be redirected to `/login`.

#### **6.2 Protected Pages**
- `/dashboard` - Main dashboard showing registered alerts
- `/movies` - Browse now showing movies
- `/movies/upcoming` - Browse upcoming movies
- `/movies/[id]` - Movie detail page
- `/alerts/new` - Register new alert
- `/alerts/[id]/edit` - Edit existing alert
- `/profile` - User profile and settings

---

### **7. Azure Functions API Endpoints (To Be Created)**

#### **7.1 Movie Data Endpoints**
- `GET /api/movies/now-showing` - Get current movies from Scope API
- `GET /api/movies/upcoming` - Get upcoming movies from Scope API
- `GET /api/movies/{id}` - Get specific movie details
- `GET /api/movies/search?name={name}` - Search movies by name

#### **7.2 Alert Management Endpoints**
- `GET /api/alerts` - Get user's alerts filtered by email (requires auth)
- `POST /api/alerts` - Create new alert with user's email (requires auth)
- `PUT /api/alerts/{id}` - Update alert (requires auth + ownership validation)
- `DELETE /api/alerts/{id}` - Delete single alert (requires auth + ownership validation)
- `DELETE /api/alerts/bulk` - Delete multiple alerts by IDs (requires auth + ownership validation)
- `PATCH /api/alerts/{id}/toggle` - Toggle alert active status (requires auth + ownership validation)

#### **7.3 Scope API Integration Endpoints**
- `GET /api/scope/access-token` - Get Scope API access token
- `GET /api/scope/movie-screenings/{movieId}?date={date}` - Get movie screenings

---

### **8. UI/UX Requirements**

#### **8.1 Design System**
- **Theme:** Dark theme as primary (with optional light mode)
- **Color Palette:** 
  - Primary: Cinema red (#DC2626)
  - Secondary: Deep blue (#1E40AF)
  - Background: Dark gray (#111827)
  - Text: Light gray (#F9FAFB)
  - Danger: Red (#EF4444) for delete operations
  - Success: Green (#10B981) for confirmations
  - Warning: Amber (#F59E0B) for cautions
- **Typography:** Inter or similar modern font
- **Components:** Consistent with Shadcn/ui design system
- **Key UI Components Required:**
  - Confirmation modals for delete operations
  - Toast notifications for user feedback
  - Loading spinners and skeleton loaders
  - Bulk selection checkboxes
  - Undo action buttons
  - Protected route wrappers
  - Authentication forms (login, register, forgot password)

#### **8.2 Responsive Design**
- Mobile-first approach
- Breakpoints: sm (640px), md (768px), lg (1024px), xl (1280px)
- Touch-friendly interface for mobile devices
- Optimized movie poster grid layouts

#### **8.3 User Experience**
- Fast loading times with Next.js optimizations
- Optimistic UI updates with rollback capability
- Loading states and skeleton screens
- Error handling with user-friendly messages
- Accessibility compliance (WCAG 2.1 AA)
- **Delete Operation UX:**
  - Clear confirmation dialogs for single and bulk deletes
  - Visual feedback during delete operations (loading states)
  - Toast notifications for successful deletions
  - Undo button with 5-second timeout for accidental deletions
  - Bulk selection with clear visual indicators
- **Authentication UX:**
  - Seamless redirect to login for unauthenticated users
  - Persistent login state across browser sessions
  - Clear logout functionality
  - Session timeout warnings

---

### **9. Development Phases**

#### **Phase 1: Foundation & Security (Week 1)**
- Set up Next.js project with dark theme
- Implement Supabase authentication with email/password
- Configure Row Level Security (RLS) policies in Supabase
- Create basic page structure and navigation with protected routes
- Set up Azure Functions HTTP triggers with authentication middleware
- Implement email-based data isolation

#### **Phase 2: Core Features (Week 2)**
- Implement dashboard with registered alerts (filtered by user email)
- Create movie browsing pages (authenticated access only)
- Integrate Scope API for movie data
- Implement alert registration forms with user email auto-population
- Add basic alert deletion functionality with confirmation

#### **Phase 3: Enhanced Alert Management (Week 3)**
- Implement bulk alert deletion with confirmation dialogs
- Add movie detail pages with alert registration integration
- Implement alert editing functionality
- Add search and filtering functionality within user's alerts
- Implement soft delete with undo functionality

#### **Phase 4: Polish & Security Hardening (Week 4)**
- UI/UX refinements for delete operations
- Enhanced error handling and user feedback
- Security testing and validation of data isolation
- Performance optimization
- Testing and bug fixes
- Documentation updates

---

### **10. Security & Performance Considerations**

#### **10.1 Security**
- Row Level Security (RLS) on Supabase tables with email-based filtering
- Environment variables for sensitive data
- Input validation and sanitization
- Rate limiting on API endpoints
- HTTPS enforcement
- **Authentication & Authorization:**
  - JWT token validation on all protected endpoints
  - Email-based data isolation enforced at database level
  - Ownership validation for all CRUD operations
  - Automatic logout on token expiration
- **Data Protection:**
  - User emails are used as the primary data isolation mechanism
  - No user can access another user's alerts or data
  - Audit logging for delete operations (recommended)

#### **10.2 Performance**
- Next.js Image optimization for movie posters
- Caching strategies for movie data
- Pagination for large datasets
- Code splitting and lazy loading
- CDN deployment

---

### **11. Deployment & DevOps**

#### **11.1 Frontend Deployment**
- **Platform:** Vercel (recommended for Next.js)
- **Alternative:** Azure Static Web Apps
- **Domain:** Custom domain configuration
- **Environment:** Staging and production environments

#### **11.2 Backend Deployment**
- **Existing:** Azure Functions (already deployed)
- **New Functions:** Deploy additional HTTP triggers
- **Configuration:** Azure App Configuration or Key Vault

---

### **12. Success Criteria**

#### **12.1 Functional Requirements**
- [ ] Users can register and authenticate (login required for all features)
- [ ] Users can view and manage only their own alerts (filtered by email)
- [ ] Users can delete single alerts with confirmation
- [ ] Users can delete multiple alerts at once
- [ ] Users can browse current and upcoming movies (after login)
- [ ] Users can register new alerts with all required fields (email auto-populated)
- [ ] Movie posters and details display correctly
- [ ] Real-time sync with Supabase database
- [ ] Proper data isolation - users cannot see other users' data
- [ ] Automatic logout and redirect to login on session expiry

#### **12.2 Performance Requirements**
- Page load times < 3 seconds
- Time to Interactive (TTI) < 5 seconds
- 95%+ uptime for web application
- Responsive design works on all device sizes

#### **12.3 User Experience Requirements**
- Intuitive navigation and user flows
- Consistent dark theme throughout
- Error states provide clear guidance
- Mobile experience matches desktop functionality

---

### **13. Future Enhancements (Post-MVP)**

- Push notifications for mobile browsers
- Social media integration for movie sharing
- Movie recommendations based on user preferences
- Advanced filtering (genre, rating, language)
- User reviews and ratings
- Movie watchlist functionality
- Calendar integration for alert dates
- Multiple notification channels (SMS, Email)

---

### **14. Risk Assessment**

#### **14.1 Technical Risks**
- **Scope API Changes:** Risk of unofficial API endpoints changing
- **Rate Limiting:** Potential throttling from Scope API
- **Data Consistency:** Sync between name-based and ID-based alerts

#### **14.2 Mitigation Strategies**
- Implement robust error handling and fallbacks
- Cache movie data to reduce API calls
- Monitor API response patterns for changes
- Implement retry mechanisms with exponential backoff

---

This PRD provides a comprehensive roadmap for developing the Scope Ticket Notifier V3.0 web application. The document should be reviewed and approved by stakeholders before beginning development.
