import React, { useState, useEffect } from "react";
import { studentApi } from "../services/api";
import { useToast } from "../context/ToastContext";

const PaymentStatusBadge = ({ status }) => {
  const styles = {
    completed: { bg: "#DCFCE7", color: "#15803D", border: "#86EFAC" },
    pending: { bg: "#FEF3C7", color: "#92400E", border: "#FCD34D" },
    failed: { bg: "#FEE2E2", color: "#B91C1C", border: "#FECACA" },
    partial: { bg: "#FED7AA", color: "#92400E", border: "#FDBA74" },
  };
  const style = styles[status?.toLowerCase()] || styles.pending;
  return (
    <span style={{
      background: style.bg, color: style.color, border: `1px solid ${style.border}`,
      borderRadius: 6, padding: "4px 10px", fontSize: "0.75rem", fontWeight: 600,
      textTransform: "capitalize", display: "inline-block"
    }}>
      {status || "Pending"}
    </span>
  );
};

export default function StudentPaymentHistoryPage() {
  const toast = useToast();
  const [loading, setLoading] = useState(true);
  const [registrations, setRegistrations] = useState([]);
  const [totalPaid, setTotalPaid] = useState(0);

  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      try {
        const res = await studentApi.getRegisteredCourses();
        if (res.data) {
          setRegistrations(res.data);
          // Calculate total paid
          const total = res.data.reduce((sum, reg) => {
            if (reg.paymentStatus === "Completed" || reg.paymentStatus === "completed") {
              return sum + (reg.fees || 0);
            }
            return sum;
          }, 0);
          setTotalPaid(total);
        }
      } catch (err) {
        toast.error(err.message || "Failed to load payment history.");
      } finally {
        setLoading(false);
      }
    };
    fetch();
  }, [toast]);

  if (loading) {
    return (
      <div style={{ display: "flex", alignItems: "center", justifyContent: "center", minHeight: 400 }}>
        <div style={{ textAlign: "center", color: "var(--text-muted)" }}>Loading...</div>
      </div>
    );
  }

  return (
    <div className="page">
      <h1 style={{ fontFamily: "var(--font-display)", fontSize: "1.8rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: 32 }}>
        💳 Payment History
      </h1>

      {/* Summary Cards */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: 16, marginBottom: 32 }}>
        <div className="card" style={{ padding: 20 }}>
          <div style={{ color: "var(--text-muted)", fontSize: "0.8rem", marginBottom: 8 }}>Total Paid</div>
          <div style={{ fontSize: "1.8rem", fontWeight: 700, color: "var(--primary)" }}>₹{totalPaid.toLocaleString()}</div>
        </div>
        <div className="card" style={{ padding: 20 }}>
          <div style={{ color: "var(--text-muted)", fontSize: "0.8rem", marginBottom: 8 }}>Total Registrations</div>
          <div style={{ fontSize: "1.8rem", fontWeight: 700, color: "var(--text-primary)" }}>{registrations.length}</div>
        </div>
        <div className="card" style={{ padding: 20 }}>
          <div style={{ color: "var(--text-muted)", fontSize: "0.8rem", marginBottom: 8 }}>Pending Payments</div>
          <div style={{ fontSize: "1.8rem", fontWeight: 700, color: "#F59E0B" }}>
            {registrations.filter(r => r.paymentStatus !== "Completed" && r.paymentStatus !== "completed").length}
          </div>
        </div>
      </div>

      {/* Payment History Table */}
      {registrations && registrations.length > 0 ? (
        <div className="card animate-fadeUp" style={{ padding: 24, overflowX: "auto" }}>
          <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 16 }}>
            Transaction Details
          </h3>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr style={{ borderBottom: "1px solid var(--border)" }}>
                <th style={{ textAlign: "left", padding: "12px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Course Name</th>
                <th style={{ textAlign: "left", padding: "12px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Amount</th>
                <th style={{ textAlign: "left", padding: "12px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Payment Status</th>
                <th style={{ textAlign: "left", padding: "12px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Registered Date</th>
                <th style={{ textAlign: "left", padding: "12px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Action</th>
              </tr>
            </thead>
            <tbody>
              {registrations.map((reg, i) => (
                <tr key={reg.id || i} style={{ borderBottom: i < registrations.length - 1 ? "1px solid var(--border-light)" : "none" }}>
                  <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>
                    {reg.courseName}
                  </td>
                  <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem", fontWeight: 600 }}>
                    ₹{reg.fees?.toLocaleString()}
                  </td>
                  <td style={{ padding: "12px 0" }}>
                    <PaymentStatusBadge status={reg.paymentStatus} />
                  </td>
                  <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>
                    {reg.registeredDate ? new Date(reg.registeredDate).toLocaleDateString() : "—"}
                  </td>
                  <td style={{ padding: "12px 0" }}>
                    <button
                      style={{
                        padding: "6px 12px",
                        background: "rgba(91,91,214,0.1)",
                        color: "var(--primary)",
                        border: "1px solid rgba(91,91,214,0.3)",
                        borderRadius: 4,
                        cursor: "pointer",
                        fontSize: "0.75rem",
                        fontWeight: 600
                      }}
                    >
                      Invoice
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="card animate-fadeUp" style={{ padding: 60, textAlign: "center" }}>
          <div style={{ fontSize: 48, marginBottom: 16 }}>💰</div>
          <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 8 }}>
            No Payment History
          </h3>
          <p style={{ color: "var(--text-muted)", fontSize: "0.9rem" }}>
            Register for a course to see your payment history here.
          </p>
        </div>
      )}
    </div>
  );
}
