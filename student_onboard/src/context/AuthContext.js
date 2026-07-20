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
  approvalStatus: user.approvalStatus ?? "Pending",
  avatar: `${user.firstName?.[0] || ""}${user.lastName?.[0] || ""}`.toUpperCase(),
  profilePic: user.profilePhotoUrl || null,
});

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    try { return JSON.parse(localStorage.getItem("edu_user")) || null; }
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

      // Map C# UserDto → our user object
      const userObj = mapUser(data.user);
      localStorage.setItem("edu_user", JSON.stringify(userObj));
      if (userObj.profilePic) {
        localStorage.setItem("edu_user_pic", userObj.profilePic);
      }
      setUser(userObj);
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
      setUser(null);
    }
  }, []);

  // ── Signup ───────────────────────────────────────────────────────────────
  // Calls POST /api/Auth/signup
  // On success: returns tokens and logs in user immediately
  const signup = useCallback(async ({ firstName, lastName, email, phoneNumber, password, confirmPassword }) => {
    setLoading(true); setError(null);
    try {
      const res = await authApi.signup(firstName, lastName, email, phoneNumber, password, confirmPassword);
      const data = res.data || res;

      if (!data.accessToken) {
        throw new Error("Invalid response from server.");
      }

      // Store tokens
      tokenHelper.setTokens(data.accessToken, data.refreshToken, data.expiresAt);

      // Map C# UserDto → our user object and store
      const userObj = mapUser(data.user);
      localStorage.setItem("edu_user", JSON.stringify(userObj));
      if (userObj.profilePic) {
        localStorage.setItem("edu_user_pic", userObj.profilePic);
      }
      setUser(userObj);

      return { success: true, user: userObj };
    } catch (err) {
      setError(err.message || "Signup failed. Please try again.");
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  }, []);

  // ── Verify OTP ───────────────────────────────────────────────────────────
  // Calls POST /api/Auth/verify-otp
  // Does NOT log in user; they must login after verifying email
  const verifyOtp = useCallback(async (email, otpCode) => {
    setLoading(true); setError(null);
    try {
      await authApi.verifyOtp(email, otpCode, "EmailVerification");
      setError(null);
      return { success: true };
    } catch (err) {
      setError(err.message || "OTP verification failed. Please try again.");
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  }, []);

  // ── Resend OTP ───────────────────────────────────────────────────────────
  const resendOtp = useCallback(async (email) => {
    setLoading(true); setError(null);
    try {
      await authApi.resendOtp(email, "EmailVerification");
      setError(null);
      return { success: true };
    } catch (err) {
      setError(err.message || "Failed to resend OTP. Please try again.");
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  }, []);

  // ── Derived booleans ────────────────────────────────────────────────────
  const isAdmin    = user?.role === "Admin";
  const isStudent  = user?.role === "Student";
  const isApproved = user?.approvalStatus === "Approved";
  const isPending  = user?.approvalStatus === "Pending";

  // ── updateUser: merge partial updates ────────────────────────────────────
  const updateUser = (partial) => {
    const updated = { ...user, ...partial };
    setUser(updated);
    localStorage.setItem("edu_user", JSON.stringify(updated));
  };

  return (
    <AuthContext.Provider value={{
      user,
      isAdmin,
      isStudent,
      isApproved,
      isPending,
      loading,
      error,
      login,
      logout,
      signup,
      verifyOtp,
      resendOtp,
      updateUser,
      isAuthenticated: !!user,
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
