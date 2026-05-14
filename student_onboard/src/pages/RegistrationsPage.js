// src/pages/RegistrationsPage.js
import React, { useState, useEffect, useMemo } from "react";
import { registrationsApi, coursesApi } from "../services/api";
import { useToast } from "../context/ToastContext";
import { SearchInput, PageLoader, EmptyState } from "../components/common";

// Status config — values must match backend validator: Pending, Paid, Partial, Refunded
const STATUS_CONFIG = {
  Pending:  { label: "Unpaid",         color: "#DC2626", bg: "#FEE2E2", dot: "#EF4444" },
  Partial:  { label: "Partially Paid", color: "#1D4ED8", bg: "#DBEAFE", dot: "#3B82F6" },
  Paid:     { label: "Paid",           color: "#15803D", bg: "#DCFCE7", dot: "#22C55E" },
};

export default function RegistrationsPage() {
  const toast = useToast();
  const [registrations, setRegistrations] = useState([]);
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [courseFilter, setCourseFilter] = useState("all");
  const [search, setSearch] = useState("");
  const [updatingId, setUpdatingId] = useState(null);
  const [editingAmount, setEditingAmount] = useState({});

  const initialLoadDone = React.useRef(false);

  const fetchData = async () => {
    if (!initialLoadDone.current) setLoading(true);
    try {
      const [regRes, courseRes] = await Promise.all([
        registrationsApi.getAll({ limit: 200 }),
        coursesApi.getAll(),
      ]);
      setRegistrations(regRes.data || []);
      setCourses(courseRes.data || []);
    } catch (err) {
      if (!initialLoadDone.current) toast.error("Failed to load registrations.");
    } finally {
      setLoading(false);
      initialLoadDone.current = true;
    }
  };

  useEffect(() => {
    fetchData();
    const interval = setInterval(fetchData, 15000); // Auto-refresh every 15s
    return () => clearInterval(interval);
    /* eslint-disable-next-line */
  }, []);

  const handlePaymentChange = async (regId, newStatus, amount) => {
    setUpdatingId(regId);
    try {
      const paymentAmount = amount != null ? Number(amount) : null;
      await registrationsApi.updatePayment(regId, {
        paymentStatus: newStatus,
        paymentAmount,
        notes: `Payment marked as ${newStatus} by admin`,
      });
      setRegistrations((prev) =>
        prev.map((r) => r.id === regId ? { ...r, paymentStatus: newStatus, paymentAmount } : r)
      );
      setEditingAmount((prev) => { const n = { ...prev }; delete n[regId]; return n; });
      toast.success(`Payment updated to ${newStatus}.`);
    } catch (err) {
      toast.error(err.message || "Failed to update payment.");
    } finally {
      setUpdatingId(null);
    }
  };

  const handleAmountSave = (regId, currentStatus) => {
    const amount = editingAmount[regId];
    if (amount === undefined || amount === "") return;
    handlePaymentChange(regId, currentStatus || "Partial", Number(amount));
  };

  const filtered = useMemo(() => {
    let list = registrations;
    if (courseFilter !== "all") {
      list = list.filter((r) => r.courseId === courseFilter);
    }
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(
        (r) =>
          r.studentName?.toLowerCase().includes(q) ||
          r.studentEmail?.toLowerCase().includes(q) ||
          r.courseName?.toLowerCase().includes(q)
      );
    }
    return list;
  }, [registrations, courseFilter, search]);

  const stats = useMemo(() => {
    const byStatus = { Pending: 0, Partial: 0, Paid: 0 };
    registrations.forEach((r) => {
      const s = r.paymentStatus || "Pending";
      byStatus[s] = (byStatus[s] || 0) + 1;
    });
    return byStatus;
  }, [registrations]);

  if (loading) return <PageLoader />;

  return (
    <div className="page" style={{ display: "flex", flexDirection: "column", gap: 20 }}>
      {/* Header */}
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", flexWrap: "wrap", gap: 14 }}>
        <div>
          <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.4rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: 4 }}>
            Course Registrations
          </h2>
          <p style={{ color: "var(--text-muted)", fontSize: "0.85rem" }}>
            {registrations.length} total registrations
          </p>
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          {Object.entries(stats).map(([status, count]) => {
            const cfg = STATUS_CONFIG[status] || { label: status, dot: "var(--text-muted)" };
            return (
              <div key={status} className="card" style={{
                padding: "8px 16px", display: "flex", alignItems: "center", gap: 8,
                animation: "fadeUp 0.3s ease both",
              }}>
                <span style={{ width: 8, height: 8, borderRadius: "50%", background: cfg.dot }} />
                <span style={{ fontFamily: "var(--font-display)", fontSize: "0.8rem", fontWeight: 700, color: "var(--text-primary)" }}>{count}</span>
                <span style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{cfg.label}</span>
              </div>
            );
          })}
        </div>
      </div>

      {/* Filters toolbar */}
      <div style={{ display: "flex", alignItems: "center", gap: 14, flexWrap: "wrap" }}>
        <div style={{ position: "relative" }}>
          <select
            value={courseFilter}
            onChange={(e) => setCourseFilter(e.target.value)}
            className="input-field"
            style={{ padding: "9px 36px 9px 14px", minWidth: 220, fontFamily: "var(--font-display)", fontSize: "0.82rem", fontWeight: 600, appearance: "none", cursor: "pointer" }}
          >
            <option value="all">All Courses</option>
            {courses.map((c) => (
              <option key={c.id} value={c.id}>{c.title}</option>
            ))}
          </select>
          <span style={{ position: "absolute", right: 12, top: "50%", transform: "translateY(-50%)", pointerEvents: "none", color: "var(--text-muted)", fontSize: 12 }}>▼</span>
        </div>
        <div style={{ marginLeft: "auto" }}>
          <SearchInput value={search} onChange={setSearch} placeholder="Search student or course..." />
        </div>
      </div>

      {/* Table */}
      <div className="card" style={{ overflow: "hidden" }}>
        <div style={{
          display: "grid", gridTemplateColumns: "2fr 1.8fr 1.2fr 1fr 1fr",
          gap: 16, padding: "12px 20px", background: "var(--surface-2)", borderBottom: "1px solid var(--border)",
        }}>
          {["Student", "Course", "Payment", "Amount", "Registered"].map((h) => (
            <div key={h} style={{ fontFamily: "var(--font-display)", fontSize: "0.72rem", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.06em" }}>{h}</div>
          ))}
        </div>

        {filtered.length === 0 ? (
          <EmptyState icon="📋" title="No registrations found" description="Try changing your course filter or search term." />
        ) : (
          filtered.map((r, i) => (
            <div key={r.id} style={{
              display: "grid", gridTemplateColumns: "2fr 1.8fr 1.2fr 1fr 1fr",
              gap: 16, padding: "14px 20px", borderBottom: "1px solid var(--border-light)",
              animation: `fadeUp 0.35s ease both`, animationDelay: `${i * 0.03}s`,
              transition: "var(--transition)",
            }}
            onMouseOver={(e) => e.currentTarget.style.background = "var(--surface-3)"}
            onMouseOut={(e) => e.currentTarget.style.background = "transparent"}
            >
              {/* Student */}
              <div>
                <div style={{ fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.875rem", color: "var(--text-primary)" }}>{r.studentName}</div>
                <div style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginTop: 2 }}>{r.studentEmail}</div>
              </div>
              {/* Course */}
              <div style={{ display: "flex", alignItems: "center" }}>
                <span style={{ fontSize: "0.82rem", color: "var(--text-secondary)", fontFamily: "var(--font-display)", fontWeight: 500 }}>{r.courseName}</span>
              </div>
              {/* Payment Status */}
              <div style={{ display: "flex", alignItems: "center" }}>
                {(() => {
                  const status = r.paymentStatus || "Pending";
                  const cfg = STATUS_CONFIG[status] || STATUS_CONFIG.Pending;
                  return (
                    <div style={{ position: "relative" }}>
                      <select
                        value={status}
                        onChange={(e) => handlePaymentChange(r.id, e.target.value, editingAmount[r.id] ?? r.paymentAmount)}
                        disabled={updatingId === r.id}
                        style={{
                          padding: "6px 30px 6px 12px",
                          fontSize: "0.76rem",
                          fontFamily: "var(--font-display)",
                          fontWeight: 700,
                          appearance: "none",
                          cursor: updatingId === r.id ? "wait" : "pointer",
                          border: "none",
                          borderRadius: 20,
                          background: cfg.bg,
                          color: cfg.color,
                          outline: "none",
                          opacity: updatingId === r.id ? 0.6 : 1,
                        }}
                      >
                        <option value="Pending">Unpaid</option>
                        <option value="Partial">Partially Paid</option>
                        <option value="Paid">Paid</option>
                      </select>
                      <span style={{ position: "absolute", right: 10, top: "50%", transform: "translateY(-50%)", pointerEvents: "none", fontSize: 9, color: cfg.color }}>&#9660;</span>
                    </div>
                  );
                })()}
              </div>
              {/* Amount */}
              <div style={{ display: "flex", alignItems: "center" }}>
                <div style={{ position: "relative", display: "inline-flex", alignItems: "center" }}>
                  <span style={{ position: "absolute", left: 8, fontSize: "0.75rem", color: "var(--text-muted)", pointerEvents: "none", fontFamily: "var(--font-display)" }}>Rs.</span>
                  <input
                    type="number"
                    min="0"
                    placeholder="0"
                    value={editingAmount[r.id] !== undefined ? editingAmount[r.id] : (r.paymentAmount || "")}
                    onChange={(e) => setEditingAmount((prev) => ({ ...prev, [r.id]: e.target.value }))}
                    onBlur={() => {
                      if (editingAmount[r.id] !== undefined && editingAmount[r.id] !== String(r.paymentAmount || "")) {
                        handleAmountSave(r.id, r.paymentStatus);
                      }
                    }}
                    onKeyDown={(e) => { if (e.key === "Enter") e.target.blur(); }}
                    style={{
                      width: 100, padding: "6px 8px 6px 30px",
                      fontSize: "0.82rem", fontFamily: "var(--font-display)", fontWeight: 700,
                      border: "1.5px solid var(--border)",
                      borderRadius: 10, background: "var(--surface)",
                      color: "var(--text-primary)", outline: "none",
                      transition: "border-color 0.2s",
                    }}
                    onFocus={(e) => e.target.style.borderColor = "var(--primary)"}
                  />
                </div>
              </div>
              {/* Registered */}
              <div style={{ display: "flex", alignItems: "center" }}>
                <span style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>
                  {r.registeredAt ? new Date(r.registeredAt).toLocaleDateString() : "—"}
                </span>
              </div>
            </div>
          ))
        )}

        <div style={{ padding: "14px 20px", borderTop: "1px solid var(--border)", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontFamily: "var(--font-body)" }}>
            Showing {filtered.length} of {registrations.length} registrations
          </span>
        </div>
      </div>
    </div>
  );
}
