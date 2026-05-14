// src/context/AuthContext.js
// ─────────────────────────────────────────────────────────────────────────────
// Integrated with C# ASP.NET Core backend
// Login response shape:
//   { accessToken, refreshToken, expiresAt, user: { id, firstName, lastName, email, role } }
// ─────────────────────────────────────────────────────────────────────────────
import React, { createContext, useContext, useState, useCallback } from "react";
import { authApi, tokenHelper } from "../services/api";

const AuthContext = createContext(null);

// ── Helper: build admin object from C# UserDto ────────────────────────────
// C# returns: { id, firstName, lastName, email, phoneNumber, emailVerified, role }
// We map it to a clean admin object for the UI
const mapUser = (user) => ({
  id:     user.id,
  name:   `${user.firstName} ${user.lastName}`.trim(),
  email:  user.email,
  role:   user.role,
  avatar: `${user.firstName?.[0] || ""}${user.lastName?.[0] || ""}`.toUpperCase(),
  profilePic: user.profilePhotoUrl || null,
});

export function AuthProvider({ children }) {
  const [admin, setAdmin] = useState(() => {
    try { return JSON.parse(localStorage.getItem("edu_admin")) || null; }
    catch { return null; }
  });
  const [loading, setLoading] = useState(false);
  const [error,   setError]   = useState(null);

  // ── Login ───────────────────────────────────────────────────────────────
  // Calls POST /api/Auth/login
  // On success: stores accessToken + refreshToken + admin info
  const login = useCallback(async ({ email, password }) => {
    setLoading(true); setError(null);
    try {
      const res = await authApi.login(email, password);

      // C# response: { accessToken, refreshToken, expiresAt, user }
      // Check both res directly and res.data (some C# APIs wrap in data)
      const data = res.data || res;

      if (!data.accessToken) {
        throw new Error("Invalid response from server.");
      }

      // Store tokens (including expiresAt for proactive refresh)
      tokenHelper.setTokens(data.accessToken, data.refreshToken, data.expiresAt);

      // Map C# UserDto → our admin object
      const adminObj = mapUser(data.user);
      localStorage.setItem("edu_admin", JSON.stringify(adminObj));
      if (adminObj.profilePic) {
        localStorage.setItem("edu_admin_pic", adminObj.profilePic);
      }
      setAdmin(adminObj);
      return true;

    } catch (err) {
      // C# returns { success: false, message: "..." } on bad credentials
      setError(err.message || "Login failed. Check your credentials.");
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  // ── Logout ──────────────────────────────────────────────────────────────
  // Calls POST /api/Auth/logout  (requires Bearer token + refreshToken in body)
  const logout = useCallback(async () => {
    try {
      const refreshToken = tokenHelper.getRefresh();
      if (refreshToken) await authApi.logout(refreshToken);
    } catch {
      // Even if API call fails, clear local state
    } finally {
      tokenHelper.clearTokens();
      setAdmin(null);
    }
  }, []);

  return (
    <AuthContext.Provider value={{
      admin,
      loading,
      error,
      login,
      logout,
      isAuthenticated: !!admin,
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
};
