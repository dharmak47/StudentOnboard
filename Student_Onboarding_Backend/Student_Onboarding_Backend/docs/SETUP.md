# Setup Guide

## Prerequisites
- .NET 8 SDK
- A Supabase account (for development database)
- A Gmail account with App Password enabled (for dev SMTP), or any SMTP provider

## 1. Supabase Setup (Development Database)

1. Go to [supabase.com](https://supabase.com) and create a new project
2. Note your project URL and database password
3. Go to **Project Settings > Database** to find the connection string
4. Your connection string format:
   ```
   Host=db.<project-ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true
   ```

## 2. Run SQL Scripts

1. Go to **SQL Editor** in your Supabase dashboard
2. Run the scripts in order from `sql/postgres/`:

   **Phase 1 (Authentication):**
   - `001_create_users_table.sql`
   - `002_create_user_sessions_table.sql`
   - `003_create_otp_verifications_table.sql`
   - `004_create_login_attempts_table.sql`
   - `005_create_user_social_logins_table.sql`
   - `006_create_indexes.sql`

   **Phase 2 (Admin Approval, Courses, Notifications):**
   - `007_alter_users_add_approval.sql`
   - `008_create_courses_table.sql`
   - `009_create_course_registrations_table.sql`
   - `010_create_notifications_table.sql`
   - `011_create_phase2_indexes.sql`
   - `012_seed_admin_user.sql`

   All scripts are idempotent — safe to re-run.

3. **Default Admin Account** (created by script 012):
   - Email: `admin@synora.com`
   - Password: `Admin@1234`
   - Use the forgot-password flow to reset if the seeded hash doesn't match your BCrypt version.

## 3. Configure appsettings

Edit `appsettings.Development.json`:

```json
{
  "AppSettings": {
    "IsProduction": false
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.<your-ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true"
  },
  "JwtSettings": {
    "SecretKey": "your-dev-secret-key-at-least-32-characters-long"
  },
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "kuralamu04@gmail.com",
    "Password": "wmvc fkfy uwhr tuph",
    "FromEmail": "kuralamu04@gmail.com"
  }
}
```

### Gmail App Password Setup
1. Enable 2-Factor Authentication on your Google account
2. Go to Google Account > Security > App Passwords
3. Generate a new app password for "Mail"
4. Use this 16-character password in SmtpSettings.Password

## 4. Run the Project

```bash
cd "Student Onboarding Platform"
dotnet restore
dotnet build
dotnet run
```

## 5. Test with Swagger

- Open `https://localhost:<port>/swagger` in your browser
- Test the endpoints starting with `/api/auth/signup`

## Environment Toggle

The `AppSettings.IsProduction` boolean controls which database driver is used:
- `false` (default) = PostgreSQL/Supabase via Npgsql
- `true` = SQL Server via Microsoft.Data.SqlClient

This is set per-environment in the respective appsettings files.
