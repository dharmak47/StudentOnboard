// src/pages/LoginPage.js
import React, { useState } from "react";
import { useAuth } from "../context/AuthContext";

export default function LoginPage() {
  const { login, loading, error } = useAuth();
  const [form, setForm] = useState({ email: "", password: "" });
  const [show, setShow] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    await login({ email: form.email, password: form.password });
  };

  return (
    <div style={{
      minHeight: "100vh", display: "flex",
      background: "linear-gradient(135deg, #F5D7F0 0%, #C4C3E8 40%, #8B8FD4 100%)",
      fontFamily: "var(--font-body)",
    }}>
      {/* Left panel */}
      <div style={{ flex: 1, display: "flex", alignItems: "center", justifyContent: "center", padding: 60, position: "relative", overflow: "hidden" }}>
        {[[300, -100, -80, "rgba(91,91,214,0.15)"], [200, "auto", -40, "rgba(245,215,240,0.4)"], [150, 100, "auto", "rgba(196,195,232,0.3)"]].map(([size, right, bottom, bg], i) => (
          <div key={i} style={{ position: "absolute", width: size, height: size, borderRadius: "50%", background: bg, right: right === "auto" ? undefined : right, bottom: bottom === "auto" ? undefined : bottom }} />
        ))}
        <div style={{ position: "relative", zIndex: 1, maxWidth: 460 }}>
          <div style={{ width: 64, height: 64, borderRadius: 18, background: "linear-gradient(135deg, #5B5BD6, #8B8FD4)", display: "flex", alignItems: "center", justifyContent: "center", marginBottom: 28, boxShadow: "0 8px 32px rgba(91,91,214,0.3)" }}>
            <span style={{ fontSize: 28, fontFamily: "var(--font-display)", fontWeight: 900, color: "#fff", letterSpacing: "-0.02em" }}>SO</span>
          </div>
          <h1 style={{ fontFamily: "var(--font-display)", fontSize: "2.8rem", fontWeight: 800, color: "#1A1A3E", lineHeight: 1.15, marginBottom: 18 }}>
            Student Onboarding<br />
            <span style={{ color: "#5B5BD6" }}>Admin Portal</span>
          </h1>
          <p style={{ color: "#5A5A82", fontSize: "1.05rem", lineHeight: 1.7, maxWidth: 380 }}>
            Manage student registrations, approve enrollments, and keep your courses running seamlessly.
          </p>
          <div style={{ display: "flex", gap: 24, marginTop: 40 }}>
            {[["👨‍🎓", "Students"], ["📚", "Courses"], ["📊", "Analytics"]].map(([icon, label]) => (
              <div key={label} style={{ display: "flex", alignItems: "center", gap: 8, color: "#5A5A82", fontSize: "0.875rem", fontFamily: "var(--font-display)", fontWeight: 600 }}>
                <span>{icon}</span> {label}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Right login card */}
      <div style={{ width: 480, display: "flex", alignItems: "center", justifyContent: "center", padding: 40, background: "rgba(255,255,255,0.55)", backdropFilter: "blur(20px)", borderLeft: "1px solid rgba(255,255,255,0.6)" }}>
        <div className="animate-fadeUp" style={{ width: "100%", maxWidth: 380 }}>
          <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.7rem", fontWeight: 800, color: "#1A1A3E", marginBottom: 6 }}>Welcome back</h2>
          <p style={{ color: "#9898B8", fontSize: "0.9rem", marginBottom: 32 }}>Sign in to your admin account</p>

          {/* Error from C# backend */}
          {error && (
            <div style={{ background: "#FEE2E2", border: "1px solid #FECACA", borderRadius: 10, padding: "12px 16px", marginBottom: 20, color: "#B91C1C", fontSize: "0.875rem", display: "flex", gap: 10, alignItems: "flex-start" }}>
              <span style={{ flexShrink: 0 }}>⚠️</span>
              <span>{error}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: 20 }}>
            <div>
              <label className="label">Email Address</label>
              <input className="input-field" type="email" placeholder="admin@institute.com"
                value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
            </div>
            <div>
              <label className="label">Password</label>
              <div style={{ position: "relative" }}>
                <input className="input-field" type={show ? "text" : "password"} placeholder="••••••••"
                  value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })}
                  required style={{ paddingRight: 44 }} />
                <button type="button" onClick={() => setShow(!show)}
                  style={{ position: "absolute", right: 14, top: "50%", transform: "translateY(-50%)", background: "none", border: "none", cursor: "pointer", color: "var(--text-muted)", fontSize: 16 }}>
                  {show ? "🙈" : "👁️"}
                </button>
              </div>
            </div>

            {/* Forgot Password link */}
            <div style={{ textAlign: "right", marginTop: -10 }}>
              <span style={{ fontSize: "0.82rem", color: "var(--primary)", cursor: "pointer", fontFamily: "var(--font-display)", fontWeight: 600 }}>
                Forgot password?
              </span>
            </div>

            <button type="submit" className="btn-primary" disabled={loading}
              style={{ width: "100%", justifyContent: "center", padding: "13px", fontSize: "0.95rem", marginTop: 4 }}>
              {loading ? (
                <><span style={{ width: 16, height: 16, border: "2px solid rgba(255,255,255,0.3)", borderTop: "2px solid #fff", borderRadius: "50%", animation: "spin 0.75s linear infinite", display: "inline-block" }} /> Signing in...</>
              ) : "Sign In →"}
            </button>
          </form>

          {/* Backend status indicator */}
          <div style={{ marginTop: 28, padding: 16, background: "rgba(91,91,214,0.08)", borderRadius: 10, border: "1px solid rgba(91,91,214,0.15)" }}>
            <p style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginBottom: 6, fontFamily: "var(--font-display)", fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.05em" }}>
              Backend
            </p>
            <p style={{ fontSize: "0.82rem", color: "var(--text-secondary)", fontFamily: "var(--font-body)" }}>
              🔗 Connected to C# ASP.NET Core API
            </p>
            <p style={{ fontSize: "0.78rem", color: "var(--text-muted)", fontFamily: "var(--font-body)", marginTop: 4 }}>
              {process.env.REACT_APP_API_URL || "http://localhost:5192"}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
