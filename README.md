# Student Onboarding Platform

A full-stack platform for student onboarding, course management, and administrative approval. The repository contains three components:

| Component | Path | Stack |
|-----------|------|-------|
| Backend API | `Student_Onboarding_Backend/` | .NET 8, ASP.NET Core, Dapper, Npgsql, Serilog |
| Web admin portal | `student_onboard/` | React 18, react-scripts 5, Axios, Recharts |
| Mobile app | `StudentOnboardingApp/` | .NET MAUI |

Database: PostgreSQL (Supabase in development). / use your own local database

---

## 1. Prerequisites

Install the following before running anything:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18 LTS or newer](https://nodejs.org/) (includes npm)
- [Git](https://git-scm.com/)
- A Supabase project, or a local PostgreSQL 14+ instance
- (Optional) An SMTP account (Gmail App Password or SendGrid) for email and OTP flows
- (Optional, mobile only) Visual Studio 2022 with the .NET MAUI workload

Verify the toolchain:

```powershell
dotnet --version    # expect 8.0.x
node --version      # expect v18.x or higher
npm --version
```

---

## 2. Repository layout

```
D:\SOP\
├── Student_Onboarding_Backend\
│   └── Student Onboarding Platform\
│       ├── Program.cs
│       ├── appsettings.json                  # committed, used as base
│       ├── appsettings.Development.json      # local overrides (do not commit secrets)
│       └── ...
│   └── sql\postgres\                         # numbered migration scripts
├── student_onboard\                          # React admin portal
│   ├── .env                                  # frontend environment file
│   └── package.json
├── StudentOnboardingApp\                     # MAUI mobile app (optional)
└── README.md                                 # this file
```

---

## 3. Database setup (PostgreSQL on Supabase)

### 3.1 Create the project

1. Sign in at [supabase.com](https://supabase.com) and create a new project.
2. Record the project reference (the string between `db.` and `.supabase.co`) and the database password.
3. Keep the project active. Free-tier projects are paused after one week of inactivity, which breaks DNS resolution for the host.

### 3.2 Run the schema scripts

Open **Supabase Dashboard → SQL Editor** and execute the files in `Student_Onboarding_Backend/sql/postgres/` in numeric order, from `001_create_users_table.sql` through the highest-numbered file. All scripts are idempotent.

The seed script `012_seed_admin_user.sql` creates the default administrator:

- Email: `admin@synora.com`
- Password: `Admin@1234`

If the seeded password hash does not match locally, use the forgot-password flow to reset it.

### 3.3 Choose the correct connection string

Supabase exposes two connection endpoints. The choice matters for local development.

| Endpoint | Host pattern | Port | DNS records | Use when |
|----------|--------------|------|-------------|----------|
| Direct | `db.<ref>.supabase.co` | 5432 | IPv6 only (AAAA) | The host has a public IPv6 address (cloud deployments such as Render) |
| Session pooler (Supavisor) | `aws-0-<region>.pooler.supabase.com` | 5432 | IPv4 and IPv6 | Local development on most ISPs |
| Transaction pooler | `aws-0-<region>.pooler.supabase.com` | 6543 | IPv4 and IPv6 | Stateless workloads only; no prepared statements |

For local development, use the **Session pooler**. The username changes to `postgres.<project-ref>`. Copy the exact string from **Project Settings → Database → Connection string → Session pooler**.

Example:

```
Host=aws-0-ap-south-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.fdlpaolzdtpkrichitzn;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true;
```

---

## 4. Backend setup (.NET API)

### 4.1 Configure environment

Edit `Student_Onboarding_Backend/Student Onboarding Platform/appsettings.Development.json`. This file overrides `appsettings.json` when the environment is `Development`.

```json
{
  "AppSettings": {
    "IsProduction": false
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=aws-0-<region>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "replace-with-a-random-string-at-least-32-characters-long"
  },
  "SmtpSettings": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "Username": "apikey",
    "Password": "<smtp-password-or-api-key>",
    "FromEmail": "<verified-sender@example.com>",
    "FromName": "Student Onboarding Platform",
    "EnableSsl": true
  }
}
```

Required values:

- `ConnectionStrings:DefaultConnection` — the Supabase session-pooler string from step 3.3.
- `JwtSettings:SecretKey` — any random string of at least 32 characters.
- `SmtpSettings` — required only if you exercise OTP, welcome email, or password-reset flows. Otherwise leave the defaults.

Security note: do not commit live credentials. Move secrets to user secrets or environment variables before pushing to a shared branch.

```powershell
dotnet user-secrets init --project "Student_Onboarding_Backend\Student Onboarding Platform"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Password=..." --project "Student_Onboarding_Backend\Student Onboarding Platform"
```

### 4.2 Restore and run

```powershell
cd "D:\SOP\Student_Onboarding_Backend\Student Onboarding Platform"
dotnet restore
dotnet run
```

The API listens on `http://localhost:10000` by default. The port is fixed in `Program.cs` via the `PORT` environment variable (default `10000`). To use a different port, set `PORT` before launching:

```powershell
$env:PORT = "5050"; dotnet run
```

Swagger UI is available at `http://localhost:10000/swagger`.

---

## 5. Frontend setup (React admin portal)

### 5.1 Configure environment

Edit `student_onboard/.env`:

```env
REACT_APP_API_URL=http://localhost:10000/api
```

The URL must match the port the backend is listening on, and must include the `/api` suffix.

### 5.2 Install and run

```powershell
cd D:\SOP\student_onboard
npm install
npm start
```

The dev server starts at `http://localhost:3000`. Sign in with the seeded admin credentials from step 3.2.

---

## 6. Recommended startup order

1. Confirm the Supabase project is active (open the dashboard, check the project is not paused).
2. Start the backend (`dotnet run` from the API project folder).
3. Verify the backend by opening `http://localhost:10000/swagger`.
4. Start the frontend (`npm start` from `student_onboard/`).
5. Open `http://localhost:3000` and log in.

---

## 7. Troubleshooting

### 7.1 Frontend receives `500 Internal Server Error` on login

Inspect the latest log under `Student_Onboarding_Backend/Student Onboarding Platform/logs/log-<yyyyMMdd>.txt`.

| Symptom in log | Cause | Resolution |
|----------------|-------|------------|
| `SocketException (11001): No such host is known` | The Supabase project is paused, deleted, or the host name is wrong. | Reactivate the project in the Supabase dashboard, or correct the project reference in the connection string. |
| `No such host is known` and DNS returns only an AAAA record | Direct host is IPv6-only and the local network has no public IPv6. | Switch to the Supabase session pooler (`aws-0-<region>.pooler.supabase.com`). See step 3.3. |
| `28P01: password authentication failed` | Wrong password, or pooler username missing the `.<ref>` suffix. | Use `postgres.<project-ref>` as the username when connecting through the pooler. |
| `Connection refused` on `localhost:10000` | Backend is not running, or `PORT` collision. | Restart the backend, confirm the port in the startup log. |

### 7.2 Frontend deprecation warnings during `npm start`

The following warnings are emitted by `react-scripts 5.0.1` and `webpack-dev-server`:

```
DeprecationWarning: fs.F_OK is deprecated...
DeprecationWarning: 'onAfterSetupMiddleware' option is deprecated...
DeprecationWarning: 'onBeforeSetupMiddleware' option is deprecated...
```

These are harmless. They originate from outdated dependencies inside `react-scripts` and do not affect runtime behaviour. Suppress them only by upgrading the toolchain, which is out of scope for this project.

### 7.3 CORS errors in the browser console

The backend enables an `AllowAll` CORS policy in `Program.cs`. If you still see CORS errors:

- Confirm the request URL matches `REACT_APP_API_URL`.
- Confirm `app.UseCors("AllowAll")` is registered before `app.UseAuthentication()` in `Program.cs`.

### 7.4 BCrypt password hash mismatch on the seeded admin

If the seeded admin cannot log in, run the forgot-password flow, or insert a new hash via the SQL editor using a BCrypt generator that matches the backend version (`BCrypt.Net-Next`).

### 7.5 Port 10000 already in use

```powershell
Get-NetTCPConnection -LocalPort 10000 | Select-Object OwningProcess
Stop-Process -Id <pid> -Force
```

Or set a different port via the `PORT` environment variable and update `REACT_APP_API_URL` accordingly.

---

## 8. Mobile app (optional)

The MAUI app under `StudentOnboardingApp/` consumes the same backend API. To build it:

```powershell
cd D:\SOP\StudentOnboardingApp
dotnet workload install maui
dotnet build -t:Run -f net8.0-windows10.0.19041.0
```

Update the API base URL inside `Constants.cs` if running against a remote backend.

---

## 9. Useful documents

Additional reference material is committed under `Student_Onboarding_Backend/docs/`:

- `SETUP.md` — backend-specific setup details
- `API_REFERENCE.md` — endpoint catalogue
- `ARCHITECTURE.md` — system overview
- `DATABASE.md` — schema documentation
- `CHANGELOG.md` — release history
