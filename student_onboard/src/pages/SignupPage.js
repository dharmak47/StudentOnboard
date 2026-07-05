import React, { useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useToast } from "../context/ToastContext";

export default function SignupPage({ onSignup, onBack }) {
  const { signup, loading } = useAuth();
  const toast = useToast();
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    phoneNumber: "",
    password: "",
    confirmPassword: "",
  });
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Validation
    if (!form.firstName.trim()) {
      toast.error("First name is required.");
      return;
    }
    if (!form.lastName.trim()) {
      toast.error("Last name is required.");
      return;
    }
    if (!form.email.trim()) {
      toast.error("Email is required.");
      return;
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
      toast.error("Please enter a valid email address.");
      return;
    }
    if (form.password.length < 6) {
      toast.error("Password must be at least 6 characters.");
      return;
    }
    if (form.password !== form.confirmPassword) {
      toast.error("Passwords do not match.");
      return;
    }

    const result = await signup({
      firstName: form.firstName,
      lastName: form.lastName,
      email: form.email,
      phoneNumber: form.phoneNumber || null,
      password: form.password,
      confirmPassword: form.confirmPassword,
    });

    if (result.success) {
      toast.success("Signup successful! You are now logged in.");
      if (onSignup) {
        onSignup(result.user);
      }
    } else {
      toast.error(result.error || "Signup failed. Please try again.");
    }
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
            <span style={{ color: "#5B5BD6" }}>Create Account</span>
          </h1>
          <p style={{ color: "#5A5A82", fontSize: "1.05rem", lineHeight: 1.7, maxWidth: 380 }}>
            Join the learning community and start your educational journey today.
          </p>
          <div style={{ display: "flex", gap: 24, marginTop: 40 }}>
            {[["👨‍🎓", "Students"], ["📚", "Courses"], ["📊", "Progress"]].map(([icon, label]) => (
              <div key={label} style={{ display: "flex", alignItems: "center", gap: 8, color: "#5A5A82", fontSize: "0.875rem", fontFamily: "var(--font-display)", fontWeight: 600 }}>
                <span>{icon}</span> {label}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Right signup card */}
      <div style={{ width: 520, display: "flex", alignItems: "center", justifyContent: "center", padding: 40, background: "rgba(255,255,255,0.55)", backdropFilter: "blur(20px)", borderLeft: "1px solid rgba(255,255,255,0.6)" }}>
        <div className="animate-fadeUp" style={{ width: "100%", maxWidth: 420 }}>
          <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.7rem", fontWeight: 800, color: "#1A1A3E", marginBottom: 6 }}>Create Your Account</h2>
          <p style={{ color: "#9898B8", fontSize: "0.9rem", marginBottom: 32 }}>Fill in your details to get started</p>

          <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
            {/* Name fields in two columns */}
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
              <div>
                <label className="label">First Name</label>
                <input className="input-field" type="text" placeholder="John"
                  value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
              </div>
              <div>
                <label className="label">Last Name</label>
                <input className="input-field" type="text" placeholder="Doe"
                  value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
              </div>
            </div>

            {/* Email */}
            <div>
              <label className="label">Email Address</label>
              <input className="input-field" type="email" placeholder="john@example.com"
                value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
            </div>

            {/* Phone (optional) */}
            <div>
              <label className="label">Phone Number (Optional)</label>
              <input className="input-field" type="tel" placeholder="+1 (555) 000-0000"
                value={form.phoneNumber} onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} />
            </div>

            {/* Password */}
            <div>
              <label className="label">Password</label>
              <div style={{ position: "relative" }}>
                <input className="input-field" type={showPassword ? "text" : "password"} placeholder="••••••••"
                  value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })}
                  required style={{ paddingRight: 44 }} />
                <button type="button" onClick={() => setShowPassword(!showPassword)}
                  style={{ position: "absolute", right: 14, top: "50%", transform: "translateY(-50%)", background: "none", border: "none", cursor: "pointer", color: "var(--text-muted)", fontSize: 16 }}>
                  {showPassword ? "🙈" : "👁️"}
                </button>
              </div>
            </div>

            {/* Confirm Password */}
            <div>
              <label className="label">Confirm Password</label>
              <div style={{ position: "relative" }}>
                <input className="input-field" type={showConfirm ? "text" : "password"} placeholder="••••••••"
                  value={form.confirmPassword} onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })}
                  required style={{ paddingRight: 44 }} />
                <button type="button" onClick={() => setShowConfirm(!showConfirm)}
                  style={{ position: "absolute", right: 14, top: "50%", transform: "translateY(-50%)", background: "none", border: "none", cursor: "pointer", color: "var(--text-muted)", fontSize: 16 }}>
                  {showConfirm ? "🙈" : "👁️"}
                </button>
              </div>
            </div>

            {/* Submit button */}
            <button type="submit" className="btn-primary" disabled={loading}
              style={{ width: "100%", justifyContent: "center", padding: "13px", fontSize: "0.95rem", marginTop: 8 }}>
              {loading ? (
                <><span style={{ width: 16, height: 16, border: "2px solid rgba(255,255,255,0.3)", borderTop: "2px solid #fff", borderRadius: "50%", animation: "spin 0.75s linear infinite", display: "inline-block" }} /> Creating Account...</>
              ) : "Create Account →"}
            </button>
          </form>

          {/* Sign in link */}
          <div style={{ marginTop: 24, textAlign: "center", fontSize: "0.9rem", color: "#5A5A82" }}>
            Already have an account?{" "}
            <span onClick={onBack} style={{ color: "var(--primary)", cursor: "pointer", fontWeight: 600, fontFamily: "var(--font-display)" }}>
              Sign In
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
