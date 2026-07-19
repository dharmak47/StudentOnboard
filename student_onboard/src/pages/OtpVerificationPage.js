import React, { useState, useEffect } from "react";
import { useAuth } from "../context/AuthContext";
import { useToast } from "../context/ToastContext";

export default function OtpVerificationPage({ email, onBack, onSuccess }) {
  const { verifyOtp, resendOtp, loading } = useAuth();
  const toast = useToast();
  const [otp, setOtp] = useState("");
  const [timer, setTimer] = useState(0);
  const [canResend, setCanResend] = useState(false);

  // Timer countdown
  useEffect(() => {
    if (timer <= 0) {
      setCanResend(true);
      return;
    }
    const interval = setInterval(() => {
      setTimer((t) => t - 1);
    }, 1000);
    return () => clearInterval(interval);
  }, [timer]);

  const handleVerify = async (e) => {
    e.preventDefault();

    if (!otp || otp.length !== 6) {
      toast.error("Please enter a valid 6-digit OTP.");
      return;
    }

    const result = await verifyOtp(email, otp);
    if (result.success) {
      toast.success("Email verified! Please sign in with your credentials.");
      onSuccess();
    } else {
      toast.error(result.error || "OTP verification failed.");
      setOtp("");
    }
  };

  const handleResend = async () => {
    setCanResend(false);
    setTimer(60);
    const result = await resendOtp(email);
    if (result.success) {
      toast.success("OTP resent to your email.");
    } else {
      toast.error(result.error || "Failed to resend OTP.");
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
            <span style={{ fontSize: 28, fontFamily: "var(--font-display)", fontWeight: 900, color: "#fff", letterSpacing: "-0.02em" }}>✓</span>
          </div>
          <h1 style={{ fontFamily: "var(--font-display)", fontSize: "2.8rem", fontWeight: 800, color: "#1A1A3E", lineHeight: 1.15, marginBottom: 18 }}>
            Verify Email
          </h1>
          <p style={{ color: "#5A5A82", fontSize: "1.05rem", lineHeight: 1.7, maxWidth: 380 }}>
            We sent a verification code to confirm your email address. This helps keep your account secure.
          </p>
          <div style={{ display: "flex", gap: 24, marginTop: 40 }}>
            {[["🔐", "Secure"], ["⚡", "Quick"], ["✔️", "Easy"]].map(([icon, label]) => (
              <div key={label} style={{ display: "flex", alignItems: "center", gap: 8, color: "#5A5A82", fontSize: "0.875rem", fontFamily: "var(--font-display)", fontWeight: 600 }}>
                <span>{icon}</span> {label}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Right OTP card */}
      <div style={{ width: 520, display: "flex", alignItems: "center", justifyContent: "center", padding: 40, background: "rgba(255,255,255,0.55)", backdropFilter: "blur(20px)", borderLeft: "1px solid rgba(255,255,255,0.6)" }}>
        <div className="animate-fadeUp" style={{ width: "100%", maxWidth: 420 }}>
          <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.7rem", fontWeight: 800, color: "#1A1A3E", marginBottom: 6 }}>Enter OTP</h2>
          <p style={{ color: "#9898B8", fontSize: "0.9rem", marginBottom: 8 }}>
            Check your email: <strong style={{ color: "#5A5A82" }}>{email}</strong>
          </p>
          <p style={{ color: "#9898B8", fontSize: "0.85rem", marginBottom: 32 }}>
            Enter the 6-digit code to verify your email
          </p>

          <form onSubmit={handleVerify} style={{ display: "flex", flexDirection: "column", gap: 20 }}>
            {/* OTP Input */}
            <div>
              <label className="label">Verification Code</label>
              <input
                className="input-field"
                type="text"
                placeholder="000000"
                maxLength="6"
                value={otp}
                onChange={(e) => {
                  const value = e.target.value.replace(/\D/g, "");
                  setOtp(value);
                }}
                style={{
                  fontFamily: "monospace",
                  fontSize: "1.5rem",
                  letterSpacing: "0.5rem",
                  textAlign: "center",
                  fontWeight: "600",
                }}
                required
              />
              <p style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginTop: 8 }}>
                Enter the 6-digit code sent to your email
              </p>
            </div>

            {/* Verify button */}
            <button
              type="submit"
              className="btn-primary"
              disabled={loading || otp.length !== 6}
              style={{ width: "100%", justifyContent: "center", padding: "13px", fontSize: "0.95rem" }}
            >
              {loading ? (
                <><span style={{ width: 16, height: 16, border: "2px solid rgba(255,255,255,0.3)", borderTop: "2px solid #fff", borderRadius: "50%", animation: "spin 0.75s linear infinite", display: "inline-block" }} /> Verifying...</>
              ) : "Verify Email →"}
            </button>
          </form>

          {/* Resend section */}
          <div style={{ marginTop: 28, padding: 16, background: "rgba(91,91,214,0.08)", borderRadius: 10, border: "1px solid rgba(91,91,214,0.15)" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 8 }}>
              <p style={{ fontSize: "0.85rem", color: "var(--text-muted)", margin: 0, fontFamily: "var(--font-display)", fontWeight: 600 }}>
                Didn't receive code?
              </p>
              {timer > 0 && (
                <span style={{ fontSize: "0.85rem", color: "var(--primary)", fontWeight: 600 }}>
                  {timer}s
                </span>
              )}
            </div>
            <button
              type="button"
              onClick={handleResend}
              disabled={!canResend || loading}
              style={{
                width: "100%",
                background: canResend ? "var(--primary)" : "rgba(91,91,214,0.1)",
                color: canResend ? "#fff" : "var(--text-muted)",
                border: "none",
                borderRadius: 8,
                padding: "10px 14px",
                cursor: canResend ? "pointer" : "not-allowed",
                fontSize: "0.85rem",
                fontFamily: "var(--font-display)",
                fontWeight: 600,
                transition: "var(--transition)",
              }}
            >
              Resend Code
            </button>
          </div>

          {/* Back to signup */}
          <div style={{ marginTop: 24, textAlign: "center", fontSize: "0.9rem", color: "#5A5A82" }}>
            <span onClick={onBack} style={{ color: "var(--primary)", cursor: "pointer", fontWeight: 600, fontFamily: "var(--font-display)" }}>
              ← Back to Signup
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
