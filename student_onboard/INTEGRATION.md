# 🔗 Backend Integration Guide
## Connecting React Admin Panel → C# ASP.NET Core API

---

## ✅ What's Connected NOW

| Feature | Endpoint | Status |
|---|---|---|
| Admin Login | POST /api/Auth/login | ✅ Connected |
| Admin Logout | POST /api/Auth/logout | ✅ Connected |
| Change Password | POST /api/Auth/change-password | ✅ Connected |
| Token Refresh | POST /api/Auth/refresh-token | ✅ Auto-handled |
| Students List | GET /api/Student | ⏳ Waiting for teammate |
| Approve/Block Student | PUT /api/Student/{id}/status | ⏳ Waiting for teammate |
| Courses CRUD | GET/POST/PUT/DELETE /api/Course | ⏳ Waiting for teammate |

---

## 🚀 Step 1 — Set the Backend URL

Open `edu-admin/.env` and set the correct URL:

```env
# If running locally
REACT_APP_API_URL=https://localhost:7001/api

# If running on a different port
REACT_APP_API_URL=https://localhost:5001/api

# If deployed
REACT_APP_API_URL=https://your-api.azurewebsites.net/api
```

After changing .env → restart React: `npm start`

---

## 🔐 Step 2 — How Login Works

### What React sends to C#:
```json
POST /api/Auth/login
{
  "email": "admin@institute.com",
  "password": "YourPassword",
  "deviceType": "Web",
  "deviceName": "Admin Panel"
}
```

### What C# sends back:
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "abc123...",
  "expiresAt": "2024-03-16T10:30:00Z",
  "user": {
    "id": "guid-here",
    "firstName": "Super",
    "lastName": "Admin",
    "email": "admin@institute.com",
    "role": "Admin"
  }
}
```

### What React does with it:
```
accessToken  → stored in localStorage as "edu_access_token"
               sent as "Authorization: Bearer <token>" on every request
refreshToken → stored in localStorage as "edu_refresh_token"
               used to get new accessToken when it expires
user         → mapped to admin object shown in sidebar/topbar
```

---

## 🔄 Step 3 — Token Refresh (Automatic)

When the accessToken expires, React automatically:
1. Calls `POST /api/Auth/refresh-token` with the refreshToken
2. Gets a new accessToken
3. Retries the original failed request
4. If refresh also fails → logs out and redirects to login

No manual handling needed.

---

## ⏳ Step 4 — When Teammate Adds Students Controller

When your teammate builds `StudentsController.cs`, update `src/services/api.js`:

### Find this section:
```js
export const studentsApi = {
  getAll:       (params = {}) => get(`/Student?${new URLSearchParams(params)}`),
  getById:      (id)          => get(`/Student/${id}`),
  updateStatus: (id, status)  => put(`/Student/${id}/status`, { status }),
  stats:        ()            => get("/Student/stats"),
};
```

### The endpoint paths may change based on what your teammate builds.
Ask teammate: "What are the exact route names in StudentsController?"

If they use `[Route("api/[controller]")]` and class name is `StudentsController`:
→ base route = `/api/Students`

---

## ⏳ Step 5 — When Teammate Adds Courses Controller

Same process. Find in `api.js`:
```js
export const coursesApi = {
  getAll:  (params = {}) => get(`/Course?${new URLSearchParams(params)}`),
  ...
};
```
Update endpoint paths based on what teammate builds.

---

## 🔧 CORS — Important!

Ask your teammate to make sure the C# backend allows requests from `http://localhost:3000`.

In their `Program.cs` it should have something like:
```csharp
builder.Services.AddCors(options => {
    options.AddPolicy("AllowReactApp", policy => {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

app.UseCors("AllowReactApp");
```

If CORS is not set up → every API call will fail with "CORS error" even if backend is running.

---

## 🐛 Common Errors & Fixes

| Error | Cause | Fix |
|---|---|---|
| "Cannot connect to server" | C# backend not running | Run the C# project in Visual Studio / dotnet run |
| "Invalid credentials" | Wrong email/password | Check with admin credentials |
| CORS error in browser console | CORS not configured | Ask teammate to add CORS in Program.cs |
| "Session expired" | Token expired + refresh failed | Login again |
| SSL certificate error | https://localhost self-signed cert | Accept the certificate in browser OR change to http |

---

## 📁 Files Changed for Integration

```
edu-admin/
  .env                          ← Backend URL
  src/services/api.js           ← All API calls (auth connected, others ready)
  src/context/AuthContext.js    ← Login/logout with real token handling
  src/pages/LoginPage.js        ← Updated for C# response format
  src/pages/SettingsPage.js     ← Change password connected to real API
```

## Files NOT Changed (UI stays same):
```
  src/styles/globals.css        ← unchanged
  src/components/               ← unchanged
  src/pages/DashboardPage.js    ← unchanged (uses dummy until backend ready)
  src/pages/StudentsPage.js     ← unchanged (uses dummy until backend ready)
  src/pages/CoursesPage.js      ← unchanged (uses dummy until backend ready)
```
