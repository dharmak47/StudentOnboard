# 🎓 EduAdmin Portal

> Industry-grade Student Onboarding Admin Panel built with React.js

---

## 🎨 Design System

| Token         | Value      | Usage                    |
|---------------|------------|--------------------------|
| `--primary`   | `#5B5BD6`  | Buttons, links, accents  |
| `--secondary` | `#8B8FD4`  | Gradients, hover states  |
| `--tertiary`  | `#C4C3E8`  | Borders, dividers        |
| `--blush`     | `#F5D7F0`  | Backgrounds, highlights  |

Fonts: **Plus Jakarta Sans** (display) + **DM Sans** (body)

---

## 📁 Project Structure

```
src/
├── components/
│   ├── common/          # Reusable UI atoms (Badge, Avatar, Spinner, Toast…)
│   └── layout/          # Sidebar, Topbar
├── context/
│   ├── AuthContext.js   # Global auth state (login / logout)
│   └── ToastContext.js  # Global notification system
├── hooks/
│   ├── useStudents.js   # Students data + mutations
│   └── useCourses.js    # Courses CRUD
├── pages/
│   ├── LoginPage.js
│   ├── DashboardPage.js
│   ├── StudentsPage.js
│   ├── CoursesPage.js
│   └── SettingsPage.js
├── services/
│   └── api.js           # Mock REST API layer (swap for real axios in prod)
└── styles/
    └── globals.css      # Design tokens + utility classes
```

---

## 🚀 Getting Started

```bash
npm install
npm start
```

**Demo credentials**
```
Email:    admin@eduportal.com
Password: Admin@123
```

---

## ✅ Features

### Authentication
- Email + password login with validation
- JWT token stored in `localStorage`
- Protected routes (login wall)
- Persistent session on page refresh

### Dashboard
- Live stat cards (total / approved / pending / blocked students, courses)
- Enrollment trend line chart (Recharts)
- Course distribution pie chart (Recharts)
- Recent courses table

### Student Management
- Paginated student table with filter tabs + live search
- Approve / Block with confirmation modal
- Side drawer with full student profile
- Status badges with colour coding

### Course Management
- Grid & Table view toggle
- Full CRUD: Create / Edit / Delete courses
- Category colour coding
- Emoji thumbnail picker
- Status: Active / Draft

### Settings
- Admin profile editor
- Notification toggles
- Password change form
- Danger zone

### UX / Architecture
- CSS custom properties (design tokens)
- Custom hooks for data fetching (`useStudents`, `useCourses`)
- Context API for global state (Auth, Toast)
- Simulated async API layer with realistic latency
- Skeleton-ready loading states
- Responsive layout with collapsible sidebar

---

## 🔄 Replacing Mock API

All API calls live in `src/services/api.js`.
Each function returns `{ success, data, ...meta }`.
Replace the internals with real `axios` calls:

```js
// Before (mock)
export const studentsApi = {
  getAll: async (filters) => {
    await delay(500);
    return ok([..._students]);
  }
};

// After (real)
export const studentsApi = {
  getAll: async (filters) => {
    const { data } = await axios.get("/api/students", { params: filters });
    return data;
  }
};
```

---

## 🛠 Tech Stack

| Layer       | Library              |
|-------------|----------------------|
| UI          | React 18             |
| Charts      | Recharts             |
| State       | Context API + hooks  |
| Styling     | Pure CSS (no UI lib) |
| HTTP        | Mock (→ Axios)       |
| Forms       | Controlled inputs    |
