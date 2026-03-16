# 🎓 Student Management System

A full-stack Student Management System built with **Angular 17** (frontend) and **ASP.NET Core 8** (backend), featuring JWT authentication, role-based authorization, CRUD operations, photo uploads, Excel/PDF exports, and a live analytics dashboard.

---

## 📁 Project Structure

```
StudentMS/
├── backend/
│   └── StudentManagement/          ← ASP.NET Core 8 Web API
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── StudentsController.cs
│       │   ├── DashboardController.cs
│       │   ├── ExportController.cs
│       │   └── CoursesController.cs
│       ├── Models/
│       │   ├── Student.cs
│       │   └── User.cs             (User, Role, Course)
│       ├── DTOs/
│       │   ├── Requests/RequestDTOs.cs
│       │   └── Responses/ResponseDTOs.cs
│       ├── Services/
│       │   ├── Interfaces/
│       │   └── Implementations/
│       ├── Repositories/
│       │   ├── Interfaces/IGenericRepository.cs
│       │   └── Implementations/GenericRepository.cs
│       ├── Data/AppDbContext.cs
│       ├── Helpers/JwtHelper.cs
│       ├── Middleware/GlobalExceptionMiddleware.cs
│       ├── Validators/RequestValidators.cs
│       ├── Migrations/
│       ├── Uploads/                ← Student photos stored here
│       ├── Program.cs
│       └── appsettings.json
│
└── frontend/
    └── src/app/
        ├── auth/
        │   ├── login/
        │   └── register/
        ├── dashboard/
        ├── students/
        │   ├── list/
        │   ├── add-edit/
        │   └── detail/
        ├── core/
        │   ├── services/           (auth, student, dashboard, course, export)
        │   ├── interceptors/       (jwt, error)
        │   ├── guards/             (auth, guest, role)
        │   └── directives/         (has-role)
        ├── shared/
        │   └── components/
        │       ├── layout/         (sidenav + toolbar shell)
        │       └── confirm-dialog/
        └── models/models.ts
```

---

## ⚙️ Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ |
| SQL Server | 2019+ (or LocalDB) |
| Node.js | 18+ |
| Angular CLI | 17+ |

---

## 🚀 Backend Setup

### 1. Install NuGet Packages

```bash
cd backend/StudentManagement

dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 7.3.1
dotnet add package FluentValidation.AspNetCore --version 11.3.0
dotnet add package ClosedXML --version 0.102.2
dotnet add package QuestPDF --version 2024.3.4
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
```

### 2. Configure Database Connection

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**For SQL Server Express / LocalDB**, use:
```
Server=(localdb)\\mssqllocaldb;Database=StudentManagementDB;Trusted_Connection=True;
```

### 3. Run EF Core Migrations

```bash
# Install EF tools globally (once)
dotnet tool install --global dotnet-ef

# Create initial migration (already provided, but if you need to recreate)
dotnet ef migrations add InitialCreate

# Apply migration and create database
dotnet ef database update
```

> ℹ️ The app also **auto-migrates on startup** via `db.Database.Migrate()` in `Program.cs`.

### 4. Run the Backend

```bash
dotnet run
```

Backend runs at:
- HTTP:  `http://localhost:5001`
- HTTPS: `https://localhost:7001`
- Swagger UI: `https://localhost:7001/swagger`

---

## 🌐 Frontend Setup

### 1. Install Dependencies

```bash
cd frontend
npm install
```

Key NPM packages installed:
```
@angular/material @angular/cdk @angular/animations
chart.js ng2-charts
```

### 2. Configure API URL

Edit `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001/api',
  uploadsUrl: 'https://localhost:7001/uploads'
};
```

### 3. Run the Frontend

```bash
ng serve
```

Frontend runs at: `http://localhost:4200`

---

## 🔐 Default Credentials

| Username | Password | Role |
|----------|----------|------|
| `admin` | `Admin@123` | Admin |

---

## 🔑 Authentication Flow

1. POST `/api/auth/login` → returns `{ token, refreshToken, user }`
2. Frontend stores token in `localStorage`
3. `JwtInterceptor` attaches `Authorization: Bearer <token>` to all requests
4. On 401, `JwtInterceptor` auto-calls `/api/auth/refresh-token`
5. On refresh failure → redirects to `/auth/login`

---

## 📡 API Endpoints Reference

### Auth
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/login` | ❌ | Login |
| POST | `/api/auth/register` | ❌ | Register |
| POST | `/api/auth/refresh-token` | ❌ | Refresh JWT |
| POST | `/api/auth/revoke` | ✅ | Revoke token |

### Students
| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| GET | `/api/students?page=1&pageSize=10&search=xyz` | All | Paginated list |
| GET | `/api/students/{id}` | All | Get by ID |
| POST | `/api/students` | Admin | Create (multipart/form-data) |
| PUT | `/api/students/{id}` | Admin, Teacher | Update |
| DELETE | `/api/students/{id}` | Admin | Delete |
| POST | `/api/students/{id}/photo` | Admin, Teacher | Upload photo |

### Dashboard
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/dashboard/stats` | Summary stats |
| GET | `/api/dashboard/enrollments-by-month` | Chart data (12 months) |
| GET | `/api/dashboard/students-by-course` | Pie chart data |
| GET | `/api/dashboard/recent-enrollments` | Last 5 students |

### Export
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/export/students/excel` | Download .xlsx |
| GET | `/api/export/students/pdf` | Download .pdf |

### Courses
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/courses` | List all courses |

### Static Files
```
GET /uploads/{filename}   → serves student photo
```

---

## 🗄️ Database Schema

```
Roles           (Id, Name)
Users           (Id, Username, Email, PasswordHash, RoleId, RefreshToken, RefreshTokenExpiry, CreatedAt)
Courses         (Id, Name, Description)
Students        (Id, FirstName, LastName, Email, Phone, DateOfBirth, EnrollmentDate,
                 CourseId, IsActive, PhotoPath, CreatedAt)
```

**Seed Data** (auto-inserted on first migration):
- Roles: Admin, Teacher, Student
- Courses: Computer Science, Business Administration, Data Science
- Admin user: `admin` / `Admin@123`

---

## 🎨 Frontend Features

| Feature | Implementation |
|---------|---------------|
| JWT Auth | `AuthService` + `localStorage` |
| Auto Token Refresh | `JwtInterceptor` |
| Global Error Handling | `ErrorInterceptor` + `MatSnackBar` |
| Route Protection | `authGuard`, `guestGuard` |
| Role-based UI | `*appHasRole` structural directive |
| Dashboard Charts | `chart.js` + `ng2-charts` `BaseChartDirective` |
| Data Table | `MatTable` + `MatPaginator` |
| Real-time Search | `debounceTime` + `distinctUntilChanged` |
| Photo Upload | `FileReader` API preview + `FormData` |
| File Export | `responseType: 'blob'` + programmatic download |
| Responsive Layout | Angular Material `BreakpointObserver` + CSS Grid |
| Loading States | `MatProgressSpinner` throughout |
| Confirmation Dialogs | `MatDialog` + `ConfirmDialogComponent` |

---

## 🏗️ Running Both Projects Together

**Terminal 1 – Backend:**
```bash
cd backend/StudentManagement
dotnet run
```

**Terminal 2 – Frontend:**
```bash
cd frontend
ng serve --open
```

Navigate to `http://localhost:4200` and log in with `admin` / `Admin@123`.

---

## 📦 Build for Production

**Backend:**
```bash
dotnet publish -c Release -o ./publish
```

**Frontend:**
```bash
ng build --configuration production
# Output: dist/student-management-frontend/
```

---

## 🔧 Troubleshooting

| Issue | Fix |
|-------|-----|
| CORS error | Ensure backend CORS allows `http://localhost:4200` |
| SSL cert error | Run `dotnet dev-certs https --trust` |
| DB connection failed | Check SQL Server is running; verify connection string |
| `dotnet-ef` not found | Run `dotnet tool install --global dotnet-ef` |
| ng2-charts type errors | Ensure `chart.js` version matches peer dependency |
| Photos not loading | Verify `uploadsUrl` in `environment.ts` matches backend port |

---

## 📝 License

MIT — free to use, modify and distribute.
