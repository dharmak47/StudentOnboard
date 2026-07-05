// src/services/api.js
// ─────────────────────────────────────────────────────────────────────────────
// Integrated with C# ASP.NET Core backend
// ─────────────────────────────────────────────────────────────────────────────

const BASE_URL = process.env.REACT_APP_API_URL || "http://localhost:10000";

// ── Token helpers ─────────────────────────────────────────────────────────
export const tokenHelper = {
  getAccess:    ()      => localStorage.getItem("edu_access_token"),
  getRefresh:   ()      => localStorage.getItem("edu_refresh_token"),
  getExpiresAt: ()      => localStorage.getItem("edu_token_expires"),
  setTokens:    (a, r, expiresAt)  => {
    localStorage.setItem("edu_access_token",  a);
    localStorage.setItem("edu_refresh_token", r);
    if (expiresAt) localStorage.setItem("edu_token_expires", expiresAt);
  },
  clearTokens:  ()      => {
    localStorage.removeItem("edu_access_token");
    localStorage.removeItem("edu_refresh_token");
    localStorage.removeItem("edu_token_expires");
    localStorage.removeItem("edu_user");
    localStorage.removeItem("edu_user_pic");
  },
};

// ── Refresh token logic ───────────────────────────────────────────────────
let isRefreshing = false;
let refreshQueue = []; // queue pending requests while refreshing

const processQueue = (error, token = null) => {
  refreshQueue.forEach((p) => error ? p.reject(error) : p.resolve(token));
  refreshQueue = [];
};

async function refreshAccessToken() {
  const refreshToken = tokenHelper.getRefresh();
  if (!refreshToken) throw new Error("No refresh token.");

  const res = await fetch(`${BASE_URL}/api/Auth/refresh-token`, {
    method:  "POST",
    headers: { "Content-Type": "application/json" },
    body:    JSON.stringify({ refreshToken }),
  });

  const json = await res.json();
  if (!res.ok || !json.success) throw new Error("Session expired. Please login again.");

  // C# returns { success, data: { accessToken, refreshToken, expiresAt, user } }
  const tokens = json.data;
  tokenHelper.setTokens(tokens.accessToken, tokens.refreshToken, tokens.expiresAt);
  scheduleProactiveRefresh(tokens.expiresAt);
  return tokens.accessToken;
}

// ── Proactive token refresh ───────────────────────────────────────────────
// Refresh the token 2 minutes before it expires to prevent session drops
let proactiveRefreshTimer = null;

function scheduleProactiveRefresh(expiresAt) {
  if (proactiveRefreshTimer) clearTimeout(proactiveRefreshTimer);
  if (!expiresAt) return;

  const expiresMs = new Date(expiresAt).getTime();
  const now = Date.now();
  // Refresh 2 minutes before expiry (or immediately if less than 2 min left)
  const refreshIn = Math.max((expiresMs - now) - 120000, 5000);

  proactiveRefreshTimer = setTimeout(async () => {
    try {
      await refreshAccessToken();
    } catch {
      // Will be handled by 401 interceptor on next request
    }
  }, refreshIn);
}

// Schedule refresh on app load if we have an existing token
(function initProactiveRefresh() {
  const expiresAt = tokenHelper.getExpiresAt();
  if (expiresAt && tokenHelper.getAccess()) {
    scheduleProactiveRefresh(expiresAt);
  }
})();

// ── Keep session alive on user activity ───────────────────────────────────
// Reset an inactivity timer on user interactions so the session stays alive
let activityRefreshTimer = null;

function onUserActivity() {
  if (activityRefreshTimer) clearTimeout(activityRefreshTimer);
  if (!tokenHelper.getAccess()) return;

  const expiresAt = tokenHelper.getExpiresAt();
  if (!expiresAt) return;

  const expiresMs = new Date(expiresAt).getTime();
  const timeLeft = expiresMs - Date.now();

  // If less than 5 minutes left and user is active, refresh now
  if (timeLeft < 300000 && timeLeft > 0) {
    activityRefreshTimer = setTimeout(async () => {
      try {
        if (!isRefreshing) await refreshAccessToken();
      } catch { /* handled by 401 interceptor */ }
    }, 1000);
  }
}

if (typeof window !== "undefined") {
  ["mousedown", "keydown", "scroll", "touchstart"].forEach((evt) => {
    window.addEventListener(evt, onUserActivity, { passive: true });
  });
}

// ── Core fetch wrapper ────────────────────────────────────────────────────
async function request(method, path, body, isPublic = false) {
  const headers = { "Content-Type": "application/json" };

  if (!isPublic) {
    const token = tokenHelper.getAccess();
    if (token) headers["Authorization"] = `Bearer ${token}`;
  }

  let res;
  try {
    res = await fetch(`${BASE_URL}${path}`, {
      method,
      headers,
      ...(body ? { body: JSON.stringify(body) } : {}),
    });
  } catch {
    const err = new Error("Cannot connect to server. Please check your internet connection.");
    err.isNetworkError = true;
    err.status = 0;
    throw err;
  }

  // ── 401 → try refresh token once ─────────────────────────────────────
  if (res.status === 401 && !isPublic) {
    if (isRefreshing) {
      // Queue this request until refresh completes
      return new Promise((resolve, reject) => {
        refreshQueue.push({ resolve, reject });
      }).then((newToken) => {
        headers["Authorization"] = `Bearer ${newToken}`;
        return fetch(`${BASE_URL}${path}`, { method, headers, ...(body ? { body: JSON.stringify(body) } : {}) }).then(r => r.json());
      });
    }

    isRefreshing = true;
    try {
      const newToken = await refreshAccessToken();
      processQueue(null, newToken);
      headers["Authorization"] = `Bearer ${newToken}`;
      res = await fetch(`${BASE_URL}${path}`, {
        method, headers,
        ...(body ? { body: JSON.stringify(body) } : {}),
      });
    } catch (refreshErr) {
      processQueue(refreshErr, null);
      tokenHelper.clearTokens();
      window.dispatchEvent(new Event("edu:unauthorized"));
      throw new Error("Session expired. Please login again.");
    } finally {
      isRefreshing = false;
    }
  }

  // ── Parse response ────────────────────────────────────────────────────
  let data;
  const contentType = res.headers.get("content-type");
  if (contentType && contentType.includes("application/json")) {
    data = await res.json();
  } else {
    data = { success: res.ok };
  }

  if (!res.ok) {
    // C# returns { success, message } or { errors }
    const message = data?.message || data?.title || "Request failed.";
    const err = new Error(message);
    err.status = res.status;
    err.data   = data;
    throw err;
  }

  return data;
}

const get   = (path, isPublic)    => request("GET",    path, null, isPublic);
const post  = (path, body, pub)   => request("POST",   path, body, pub);
const put   = (path, body)        => request("PUT",    path, body);
const patch = (path, body)        => request("PATCH",  path, body);
const del   = (path)              => request("DELETE", path);

// File upload (multipart/form-data) — no Content-Type header so browser sets boundary
async function uploadFile(path, file, fieldName = "photo") {
  const formData = new FormData();
  formData.append(fieldName, file);

  const headers = {};
  const token = tokenHelper.getAccess();
  if (token) headers["Authorization"] = `Bearer ${token}`;

  const res = await fetch(`${BASE_URL}${path}`, {
    method: "POST",
    headers,
    body: formData,
  });

  const data = await res.json();
  if (!res.ok) {
    const err = new Error(data?.message || "Upload failed.");
    err.status = res.status;
    throw err;
  }
  return data;
}

// ── Auth API ──────────────────────────────────────────────────────────────
// C# endpoints: POST /api/Auth/signup, login, verify-otp, etc.
// Response: { accessToken, refreshToken, expiresAt, user: { id, firstName, lastName, email, role } }
export const authApi = {
  signup: (firstName, lastName, email, phoneNumber, password, confirmPassword) =>
    post("/api/Auth/signup", {
      firstName,
      lastName,
      email,
      phoneNumber,
      password,
      confirmPassword,
    }, true), // true = public route, no token needed

  verifyOtp: (email, otpCode, otpType = "EmailVerification") =>
    post("/api/Auth/verify-otp", { email, otpCode, otpType }, true),

  resendOtp: (email, otpType = "EmailVerification") =>
    post("/api/Auth/resend-otp", { email, otpType }, true),

  login: (email, password) =>
    post("/api/Auth/login", {
      email,
      password,
      deviceType: "Web",
      deviceName: "Web Portal",
    }, true), // true = public route, no token needed

  logout: (refreshToken) =>
    post("/api/Auth/logout", { refreshToken }),

  refreshToken: (refreshToken) =>
    post("/api/Auth/refresh-token", { refreshToken }, true),

  changePassword: (currentPassword, newPassword, confirmPassword) =>
    post("/api/Auth/change-password", { currentPassword, newPassword, confirmPassword }),

  forgotPassword: (email) =>
    post("/api/Auth/forgot-password", { email }, true),

  resetPassword: (email, otp, newPassword, confirmPassword) =>
    post("/api/Auth/reset-password", { email, otp, newPassword, confirmPassword }, true),
};

// ── Students API ──────────────────────────────────────────────────────────
// Connected to AdminController: /api/Admin/students
const mapStudent = (s) => ({
  id: s.id,
  name: `${s.firstName} ${s.lastName}`,
  avatar: `${(s.firstName?.[0] || "").toUpperCase()}${(s.lastName?.[0] || "").toUpperCase()}`,
  email: s.email,
  phone: s.phoneNumber || "—",
  status: s.approvalStatus?.toLowerCase() === "denied" ? "blocked" : (s.approvalStatus || "pending").toLowerCase(),
  enrolledAt: s.createdAt ? new Date(s.createdAt).toLocaleDateString() : "—",
  course: s.registeredCourses?.[0]?.courseName || "—",
  city: s.address || "—",
  degree: s.education || "—",
  isActive: s.isActive,
  emailVerified: s.emailVerified,
});

export const studentsApi = {
  getAll: async (params = {}) => {
    const qp = new URLSearchParams();
    if (params.page)   qp.set("page",     params.page);
    if (params.limit)  qp.set("pageSize", params.limit);
    if (params.status && params.status !== "all") {
      const statusMap = { blocked: "Denied", approved: "Approved", pending: "Pending" };
      qp.set("status", statusMap[params.status] || params.status.charAt(0).toUpperCase() + params.status.slice(1));
    }
    if (params.search) qp.set("search",   params.search);
    const res = await get(`/api/Admin/students?${qp}`);
    const pg = res.data;
    return { data: (pg.items || []).map(mapStudent), total: pg.totalCount, page: pg.page, pages: pg.totalPages };
  },
  getById: async (id) => {
    const res = await get(`/api/Admin/students/${id}`);
    return { data: mapStudent(res.data) };
  },
  updateStatus: async (id, status) => {
    if (status === "approved") {
      await post(`/api/Admin/students/${id}/approve`);
    } else {
      await post(`/api/Admin/students/${id}/deny`, { reason: "Blocked by admin" });
    }
    const res = await get(`/api/Admin/students/${id}`);
    return { data: mapStudent(res.data) };
  },
  stats: () => get("/api/Admin/students/stats"),
  createUser: async (payload) => {
    return post("/api/Admin/users", payload);
  },
  changePassword: async (id, newPassword) => {
    return put(`/api/Admin/users/${id}/password`, { newPassword });
  },
};

// ── Courses API ───────────────────────────────────────────────────────────
// Connected to CourseController (GET) and AdminController (CRUD)
const mapCourse = (c) => ({
  id: c.id,
  title: c.name,
  description: c.description || "",
  instructor: c.instructor || "—",
  duration: c.duration || "—",
  category: c.category || "",
  price: c.fees || 0,
  offerPrice: c.offerPrice,
  thumbnail: c.thumbnail || "📘",
  status: c.isActive ? "active" : "non active",
  students: c.studentsCount || 0,
  rating: c.rating || null,
  syllabus: c.syllabus,
  batchTiming: c.batchTiming || "",
});

const unmapCourse = (f) => ({
  name: f.title,
  description: f.description,
  fees: Number(f.price) || 0,
  offerPrice: f.offerPrice ? Number(f.offerPrice) : null,
  syllabus: f.syllabus || null,
  duration: f.duration,
  instructor: f.instructor,
  category: f.category,
  thumbnail: f.thumbnail,
  isActive: f.status !== "non active",
  batchTiming: f.batchTiming || null,
});

export const coursesApi = {
  getAll: async (params = {}) => {
    const res = await get("/api/Admin/courses");
    return { data: (res.data || []).map(mapCourse) };
  },
  getById: async (id) => {
    const res = await get(`/api/Course/${id}`);
    return { data: mapCourse(res.data) };
  },
  create: async (payload) => {
    const res = await post("/api/Admin/courses", unmapCourse(payload));
    return { data: mapCourse(res.data) };
  },
  update: async (id, payload) => {
    const res = await put(`/api/Admin/courses/${id}`, unmapCourse(payload));
    return { data: mapCourse(res.data) };
  },
  delete: (id) => del(`/api/Admin/courses/${id}`),
};

// ── Course Registrations API ──────────────────────────────────────────────
// Connected to AdminController: /api/Admin/course-registrations
export const registrationsApi = {
  getAll: async (params = {}) => {
    const qp = new URLSearchParams();
    if (params.page) qp.set("page", params.page);
    if (params.limit) qp.set("pageSize", params.limit || 50);
    const res = await get(`/api/Admin/course-registrations?${qp}`);
    const pg = res.data;
    return { data: pg.items || [], total: pg.totalCount, page: pg.page, pages: pg.totalPages };
  },
  updatePayment: async (id, payload) => {
    const res = await put(`/api/Admin/course-registrations/${id}/payment`, payload);
    return res;
  },
};

// ── Notifications API ─────────────────────────────────────────────────────
// Connected to AdminController: /api/Admin/notifications
export const notificationsApi = {
  getAll:       ()   => get("/api/Admin/notifications"),
  unreadCount:  ()   => get("/api/Admin/notifications/unread-count"),
  markAsRead:   (id) => put(`/api/Admin/notifications/${id}/read`),
  send:         (title, message, studentIds) => post("/api/Admin/notifications/send", { title, message, studentIds: studentIds || null }),
};

// ── FAQs API ──────────────────────────────────────────────────────────
// Connected to AdminController: /api/Admin/faqs
export const faqsApi = {
  getAll: async () => {
    const res = await get("/api/Admin/faqs");
    return { data: res.data || [] };
  },
  create: async (payload) => {
    const res = await post("/api/Admin/faqs", payload);
    return { data: res.data };
  },
  update: async (id, payload) => {
    const res = await put(`/api/Admin/faqs/${id}`, payload);
    return { data: res.data };
  },
  delete: (id) => del(`/api/Admin/faqs/${id}`),
};

// ── Admin Profile API ────────────────────────────────────────────────────
export const adminProfileApi = {
  uploadPhoto: (file) => uploadFile("/api/Admin/profile/photo", file, "photo"),
};

// ── Student API ───────────────────────────────────────────────────────────
export const studentApi = {
  getProfile:           ()           => get("/api/Student/profile"),
  updateProfile:        (payload)    => put("/api/Student/profile", payload),
  uploadPhoto:          (file)       => uploadFile("/api/Student/profile/photo", file, "photo"),
  getDashboard:         ()           => get("/api/Student/dashboard"),
  getRegisteredCourses: ()           => get("/api/Student/courses"),
  registerForCourse:    (courseId)   => post("/api/Student/courses/register", { courseId }),
  getNotifications:     ()           => get("/api/Student/notifications"),
  markNotificationRead: (id)         => put(`/api/Student/notifications/${id}/read`),
  submitReview:         (courseId, rating, remarks) =>
                                        post(`/api/Student/courses/${courseId}/review`, { rating, remarks }),
  getCourseReviews:     (courseId)   => get(`/api/Student/courses/${courseId}/reviews`, true),
  getFaqs:              ()           => get("/api/Student/faqs", true),
};

// ── Public Courses API ────────────────────────────────────────────────────
export const publicCoursesApi = {
  getAll:  async () => {
    const res = await get("/api/Course", true);
    return { ...res, data: (res.data || []).map(mapCourse) };
  },
  getById: async (id) => {
    const res = await get(`/api/Course/${id}`, true);
    return { ...res, data: mapCourse(res.data) };
  },
};

//── Analytics API ─────────────────────────────────────────────────────────
// Uses AdminController dashboard endpoint
export const analyticsApi = {
  overview: async () => {
    const res = await get("/api/Admin/dashboard");
    const d = res.data;
    return {
      data: {
        enrollmentTrend: [],
        courseDistribution: [],
        totalStudents: d.totalStudents,
        totalCourses: d.totalCourses,
        totalRegistrations: d.totalRegistrations,
      },
    };
  },
};
