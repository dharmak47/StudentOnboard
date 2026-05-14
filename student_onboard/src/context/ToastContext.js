// src/context/ToastContext.js
import React, { createContext, useContext, useState, useCallback } from "react";

const ToastContext = createContext(null);

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const addToast = useCallback((message, type = "info", duration = 3500) => {
    const id = `toast_${Date.now()}`;
    setToasts((prev) => [...prev, { id, message, type }]);
    setTimeout(() => setToasts((prev) => prev.filter((t) => t.id !== id)), duration);
  }, []);

  const removeToast = useCallback((id) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const toast = {
    success: (msg) => addToast(msg, "success"),
    error:   (msg) => addToast(msg, "error"),
    info:    (msg) => addToast(msg, "info"),
    warning: (msg) => addToast(msg, "warning"),
  };

  return (
    <ToastContext.Provider value={{ toast }}>
      {children}
      <ToastContainer toasts={toasts} removeToast={removeToast} />
    </ToastContext.Provider>
  );
}

function ToastContainer({ toasts, removeToast }) {
  if (!toasts.length) return null;
  return (
    <div style={{ position: "fixed", top: 20, right: 20, zIndex: 9999, display: "flex", flexDirection: "column", gap: 10, pointerEvents: "none" }}>
      {toasts.map((t, i) => <ToastItem key={t.id} toast={t} onRemove={() => removeToast(t.id)} index={i} />)}
    </div>
  );
}

const TOAST_CONFIG = {
  success: { icon: "✓", bg: "#F0FDF4", border: "#BBF7D0", color: "#15803D", accent: "#22C55E" },
  error:   { icon: "✕", bg: "#FEF2F2", border: "#FECACA", color: "#B91C1C", accent: "#EF4444" },
  warning: { icon: "!", bg: "#FFFBEB", border: "#FDE68A", color: "#92400E", accent: "#F59E0B" },
  info:    { icon: "i", bg: "#EEF2FF", border: "#C7D2FE", color: "#3730A3", accent: "#6366F1" },
};

function ToastItem({ toast, onRemove }) {
  const cfg = TOAST_CONFIG[toast.type] || TOAST_CONFIG.info;
  return (
    <div
      style={{
        background: cfg.bg, border: `1px solid ${cfg.border}`, borderLeft: `4px solid ${cfg.accent}`,
        borderRadius: 12, padding: "12px 16px", minWidth: 300, maxWidth: 420,
        display: "flex", alignItems: "flex-start", gap: 12,
        boxShadow: "0 8px 30px rgba(0,0,0,0.12)", pointerEvents: "all",
        animation: "slideInRight 0.3s ease",
        fontFamily: "'DM Sans', sans-serif",
      }}
    >
      <div style={{ width: 22, height: 22, borderRadius: "50%", background: cfg.accent, color: "#fff", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 12, fontWeight: 700, flexShrink: 0 }}>
        {cfg.icon}
      </div>
      <span style={{ flex: 1, fontSize: "0.875rem", color: cfg.color, fontWeight: 500, lineHeight: 1.4 }}>{toast.message}</span>
      <button onClick={onRemove} style={{ background: "none", border: "none", cursor: "pointer", color: cfg.color, fontSize: 18, lineHeight: 1, opacity: 0.6, padding: 0 }}>×</button>
    </div>
  );
}

export const useToast = () => {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useToast must be used within ToastProvider");
  return ctx.toast;
};
