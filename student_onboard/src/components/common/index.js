// src/components/common/index.js

import React, { useState, useEffect, useRef } from "react";

// ── Spinner ────────────────────────────────────────────────
export function Spinner({ size = 32, color = "var(--primary)" }) {
  return (
    <div style={{ width: size, height: size, border: `3px solid var(--border)`, borderTop: `3px solid ${color}`, borderRadius: "50%", animation: "spin 0.75s linear infinite" }} />
  );
}

export function PageLoader() {
  return (
    <div style={{ display: "flex", alignItems: "center", justifyContent: "center", height: "60vh", flexDirection: "column", gap: 16 }}>
      <Spinner size={42} />
      <p style={{ color: "var(--text-muted)", fontSize: "0.875rem", fontFamily: "var(--font-body)" }}>Loading...</p>
    </div>
  );
}

// ── Empty State ────────────────────────────────────────────
export function EmptyState({ icon = "📭", title = "Nothing here", description = "", action }) {
  return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", padding: "60px 24px", textAlign: "center" }}>
      <div style={{ fontSize: 52, marginBottom: 16 }}>{icon}</div>
      <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.1rem", color: "var(--text-primary)", marginBottom: 8 }}>{title}</h3>
      {description && <p style={{ color: "var(--text-muted)", fontSize: "0.875rem", maxWidth: 320, lineHeight: 1.6 }}>{description}</p>}
      {action && <div style={{ marginTop: 20 }}>{action}</div>}
    </div>
  );
}

// ── Confirm Modal ──────────────────────────────────────────
export function ConfirmModal({ isOpen, title, message, confirmLabel = "Confirm", confirmStyle = "danger", onConfirm, onCancel, loading }) {
  if (!isOpen) return null;
  const btnStyle = confirmStyle === "danger"
    ? { background: "#EF4444", color: "#fff", border: "none", padding: "10px 24px", borderRadius: "var(--radius-md)", fontFamily: "var(--font-display)", fontWeight: 600, cursor: "pointer", fontSize: "0.875rem" }
    : { background: "var(--primary)", color: "#fff", border: "none", padding: "10px 24px", borderRadius: "var(--radius-md)", fontFamily: "var(--font-display)", fontWeight: 600, cursor: "pointer", fontSize: "0.875rem" };
  return (
    <div className="overlay" onClick={onCancel}>
      <div className="card-elevated animate-fadeUp" onClick={(e) => e.stopPropagation()}
        style={{ background: "var(--surface)", padding: "32px", maxWidth: 420, width: "90%", borderRadius: "var(--radius-xl)" }}>
        <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.15rem", marginBottom: 10 }}>{title}</h3>
        <p style={{ color: "var(--text-secondary)", fontSize: "0.9rem", lineHeight: 1.6, marginBottom: 28 }}>{message}</p>
        <div style={{ display: "flex", gap: 12, justifyContent: "flex-end" }}>
          <button className="btn-ghost" onClick={onCancel} style={{ padding: "10px 20px" }}>Cancel</button>
          <button style={btnStyle} onClick={onConfirm} disabled={loading}>
            {loading ? "Processing..." : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}

// ── Search Input (debounced — waits 400ms after typing stops) ──
export function SearchInput({ value, onChange, placeholder = "Search...", debounceMs = 400 }) {
  const [localValue, setLocalValue] = useState(value);
  const timerRef = useRef(null);

  // Sync external value changes
  useEffect(() => { setLocalValue(value); }, [value]);

  const handleChange = (e) => {
    const val = e.target.value;
    setLocalValue(val);
    if (timerRef.current) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => onChange(val), debounceMs);
  };

  // Cleanup timer on unmount
  useEffect(() => () => { if (timerRef.current) clearTimeout(timerRef.current); }, []);

  return (
    <div style={{ position: "relative" }}>
      <span style={{ position: "absolute", left: 14, top: "50%", transform: "translateY(-50%)", color: "var(--text-muted)", fontSize: 15, pointerEvents: "none" }}>🔍</span>
      <input
        className="input-field"
        value={localValue}
        onChange={handleChange}
        placeholder={placeholder}
        style={{ paddingLeft: 40, width: 260 }}
      />
    </div>
  );
}

// ── Status Badge ───────────────────────────────────────────
export function StatusBadge({ status }) {
  const map = {
    approved: "badge-success",
    blocked:  "badge-danger",
    pending:  "badge-warning",
    active:   "badge-success",
    "non active": "badge-muted",
  };
  const dot = { approved: "#22C55E", blocked: "#EF4444", pending: "#F59E0B", active: "#22C55E", "non active": "#9CA3AF" };
  return (
    <span className={`badge ${map[status] || "badge-muted"}`}>
      <span style={{ width: 6, height: 6, borderRadius: "50%", background: dot[status] || "#9CA3AF", display: "inline-block" }} />
      {status}
    </span>
  );
}

// ── Avatar ─────────────────────────────────────────────────
export function Avatar({ initials, size = 38, style: extraStyle }) {
  return (
    <div className="avatar" style={{ width: size, height: size, fontSize: size * 0.35, ...extraStyle }}>
      {initials}
    </div>
  );
}

// ── Stat Card ──────────────────────────────────────────────
export function StatCard({ icon, label, value, sub, accent = "var(--primary)", index = 0 }) {
  return (
    <div className={`card animate-fadeUp stagger-${index + 1}`}
      style={{ padding: "24px", display: "flex", alignItems: "flex-start", gap: 18 }}>
      <div style={{ width: 50, height: 50, borderRadius: "var(--radius-md)", background: `${accent}18`, display: "flex", alignItems: "center", justifyContent: "center", fontSize: 22, flexShrink: 0 }}>
        {icon}
      </div>
      <div>
        <p style={{ color: "var(--text-muted)", fontSize: "0.75rem", fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.05em", fontFamily: "var(--font-display)", marginBottom: 4 }}>{label}</p>
        <p style={{ fontFamily: "var(--font-display)", fontSize: "1.9rem", fontWeight: 800, color: "var(--text-primary)", lineHeight: 1 }}>{value}</p>
        {sub && <p style={{ color: "var(--text-muted)", fontSize: "0.78rem", marginTop: 4 }}>{sub}</p>}
      </div>
    </div>
  );
}

// ── Filter Tabs ────────────────────────────────────────────
export function FilterTabs({ options, value, onChange }) {
  return (
    <div style={{ display: "flex", gap: 4, background: "var(--surface-3)", padding: 4, borderRadius: "var(--radius-md)", width: "fit-content" }}>
      {options.map((opt) => (
        <button key={opt.value} onClick={() => onChange(opt.value)}
          style={{
            padding: "7px 16px", border: "none", borderRadius: "calc(var(--radius-md) - 2px)", cursor: "pointer",
            fontFamily: "var(--font-display)", fontSize: "0.8rem", fontWeight: 600, transition: "var(--transition)",
            background: value === opt.value ? "var(--surface)" : "transparent",
            color:      value === opt.value ? "var(--primary)" : "var(--text-muted)",
            boxShadow:  value === opt.value ? "var(--shadow-sm)" : "none",
          }}>
          {opt.label}
          {opt.count !== undefined && (
            <span style={{ marginLeft: 6, background: value === opt.value ? "var(--primary)" : "var(--border)", color: value === opt.value ? "#fff" : "var(--text-muted)", borderRadius: "var(--radius-full)", padding: "1px 7px", fontSize: "0.7rem" }}>
              {opt.count}
            </span>
          )}
        </button>
      ))}
    </div>
  );
}
