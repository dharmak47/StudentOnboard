import React from "react";
import { useAuth } from "../context/AuthContext";

export default function ApprovalPendingPage() {
  const { user, logout } = useAuth();
  return (
    <div style={{
      minHeight: "100vh", display: "flex", alignItems: "center",
      justifyContent: "center", background: "var(--surface-2)",
    }}>
      <div style={{
        background: "var(--surface-1)", borderRadius: 16, padding: 40,
        maxWidth: 480, width: "100%", textAlign: "center",
        boxShadow: "0 4px 24px rgba(0,0,0,0.08)",
      }}>
        <div style={{ fontSize: 48, marginBottom: 16 }}>⏳</div>
        <h2 style={{ color: "var(--text-1)", marginBottom: 8 }}>Account Under Review</h2>
        <p style={{ color: "var(--text-2)", marginBottom: 24 }}>
          Hi <strong>{user?.name}</strong>, your account has been submitted for admin approval.
          You will receive an email notification once your account is approved.
        </p>
        <div style={{
          background: "var(--surface-2)", borderRadius: 8,
          padding: "12px 16px", marginBottom: 24,
          fontSize: 13, color: "var(--text-2)", textAlign: "left",
        }}>
          <div><strong>Email:</strong> {user?.email}</div>
          <div><strong>Status:</strong> Pending Approval</div>
        </div>
        <button onClick={logout} style={{
          background: "var(--primary)", color: "#fff", border: "none",
          borderRadius: 8, padding: "10px 24px", cursor: "pointer", fontSize: 14,
        }}>
          Sign Out
        </button>
      </div>
    </div>
  );
}
