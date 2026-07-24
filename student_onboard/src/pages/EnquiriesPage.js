// src/pages/EnquiriesPage.js
import React, { useEffect, useState } from "react";
import { enquiriesApi } from "../services/api";
import { FiMail, FiPhone, FiUser, FiMessageSquare, FiCheckCircle, FiClock, FiRefreshCw } from "react-icons/fi";

export default function EnquiriesPage() {
  const [enquiries, setEnquiries] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showAddModal, setShowAddModal] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitSuccess, setSubmitSuccess] = useState(false);
  const [newEnquiry, setNewEnquiry] = useState({ name: "", email: "", phoneNumber: "", message: "" });

  const load = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await enquiriesApi.getAll();
      setEnquiries(res.data || []);
    } catch (e) {
      console.error("Failed to load enquiries", e);
      setError("Failed to load enquiries. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleResolve = async (id) => {
    try {
      await enquiriesApi.resolve(id);
      load();
    } catch (e) {
      console.error("Resolve failed", e);
    }
  };

  const handleAddEnquiry = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    setSubmitSuccess(false);
    try {
      await enquiriesApi.submit(newEnquiry);
      setSubmitSuccess(true);
      setNewEnquiry({ name: "", email: "", phoneNumber: "", message: "" });
      load();
      setTimeout(() => {
        setShowAddModal(false);
        setSubmitSuccess(false);
      }, 2000);
    } catch (err) {
      console.error("Submit failed", err);
      alert("Failed to submit enquiry. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const pending = enquiries.filter((e) => e.status !== "Resolved");
  const resolved = enquiries.filter((e) => e.status === "Resolved");

  return (
    <div style={{ fontFamily: "var(--font-body)" }}>
      {/* Header */}
      <div style={{ marginBottom: 28, display: "flex", alignItems: "center", justifyContent: "space-between", flexWrap: "wrap", gap: 12 }}>
        <div>
          <h1 style={{ fontFamily: "var(--font-display)", fontSize: "1.6rem", fontWeight: 800, color: "var(--text-primary)", margin: 0, marginBottom: 4 }}>
            Enquiries
          </h1>
          <p style={{ color: "var(--text-muted)", fontSize: "0.875rem", margin: 0 }}>
            View and respond to prospective student enquiries
          </p>
        </div>
        <div style={{ display: "flex", gap: 10 }}>
          <button
            onClick={() => setShowAddModal(true)}
            style={{
              display: "flex", alignItems: "center", gap: 6,
              background: "#10B981", color: "#fff", border: "none",
              borderRadius: 8, padding: "8px 16px", cursor: "pointer",
              fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.85rem",
            }}
          >
            + Add Enquiry
          </button>
          <button
            onClick={load}
            style={{
              display: "flex", alignItems: "center", gap: 6,
              background: "var(--primary)", color: "#fff", border: "none",
              borderRadius: 8, padding: "8px 16px", cursor: "pointer",
              fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.85rem",
            }}
          >
            <FiRefreshCw size={14} /> Refresh
          </button>
        </div>
      </div>

      {/* Stats */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 16, marginBottom: 28 }}>
        {[
          { label: "Total Enquiries", value: enquiries.length, icon: <FiMail size={20} />, color: "#5B5BD6", bg: "rgba(91,91,214,0.1)" },
          { label: "Pending", value: pending.length, icon: <FiClock size={20} />, color: "#F59E0B", bg: "rgba(245,158,11,0.1)" },
          { label: "Resolved", value: resolved.length, icon: <FiCheckCircle size={20} />, color: "#10B981", bg: "rgba(16,185,129,0.1)" },
        ].map((stat) => (
          <div key={stat.label} style={{
            background: "var(--surface-1)", borderRadius: 12, padding: "18px 20px",
            border: "1px solid var(--border)", display: "flex", alignItems: "center", gap: 14,
            boxShadow: "var(--shadow-sm)"
          }}>
            <div style={{ width: 44, height: 44, borderRadius: 10, background: stat.bg, display: "flex", alignItems: "center", justifyContent: "center", color: stat.color, flexShrink: 0 }}>
              {stat.icon}
            </div>
            <div>
              <div style={{ fontFamily: "var(--font-display)", fontSize: "1.5rem", fontWeight: 800, color: "var(--text-primary)", lineHeight: 1 }}>{stat.value}</div>
              <div style={{ fontFamily: "var(--font-body)", fontSize: "0.8rem", color: "var(--text-muted)", marginTop: 2 }}>{stat.label}</div>
            </div>
          </div>
        ))}
      </div>

      {/* Content */}
      {loading ? (
        <div style={{ display: "flex", alignItems: "center", justifyContent: "center", height: 200, color: "var(--text-muted)", fontFamily: "var(--font-body)" }}>
          <div style={{ textAlign: "center" }}>
            <div style={{ width: 36, height: 36, border: "3px solid rgba(91,91,214,0.2)", borderTop: "3px solid #5B5BD6", borderRadius: "50%", animation: "spin 0.75s linear infinite", margin: "0 auto 12px" }} />
            Loading enquiries...
          </div>
        </div>
      ) : error ? (
        <div style={{ background: "#FEE2E2", border: "1px solid #FECACA", borderRadius: 10, padding: 20, color: "#B91C1C", fontFamily: "var(--font-body)", fontSize: "0.9rem" }}>
          ⚠️ {error}
        </div>
      ) : enquiries.length === 0 ? (
        <div style={{
          background: "var(--surface-1)", borderRadius: 12, border: "1px dashed var(--border)",
          padding: 48, textAlign: "center", color: "var(--text-muted)"
        }}>
          <FiMail size={40} style={{ marginBottom: 12, opacity: 0.4 }} />
          <p style={{ fontFamily: "var(--font-display)", fontWeight: 700, fontSize: "1rem", color: "var(--text-secondary)", margin: "0 0 4px" }}>No Enquiries Yet</p>
          <p style={{ fontFamily: "var(--font-body)", fontSize: "0.85rem", margin: 0 }}>Submitted enquiries from the login page will appear here.</p>
        </div>
      ) : (
        <div style={{ background: "var(--surface-1)", borderRadius: 12, border: "1px solid var(--border)", overflow: "hidden", boxShadow: "var(--shadow-sm)" }}>
          {/* Table header */}
          <div style={{
            display: "grid", gridTemplateColumns: "1.5fr 1.5fr 1fr 2fr 1fr 1fr",
            gap: 0, borderBottom: "1px solid var(--border)",
            background: "var(--surface-2)", padding: "10px 20px"
          }}>
            {[
              { icon: <FiUser size={12} />, label: "Name" },
              { icon: <FiMail size={12} />, label: "Email" },
              { icon: <FiPhone size={12} />, label: "Phone" },
              { icon: <FiMessageSquare size={12} />, label: "Message" },
              { icon: null, label: "Date" },
              { icon: null, label: "Status / Action" },
            ].map((col) => (
              <div key={col.label} style={{
                display: "flex", alignItems: "center", gap: 5,
                fontFamily: "var(--font-display)", fontWeight: 700, fontSize: "0.72rem",
                color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.06em"
              }}>
                {col.icon} {col.label}
              </div>
            ))}
          </div>

          {/* Rows */}
          {enquiries.map((e, i) => {
            const isResolved = e.status === "Resolved";
            return (
              <div key={e.id} style={{
                display: "grid", gridTemplateColumns: "1.5fr 1.5fr 1fr 2fr 1fr 1fr",
                gap: 0, alignItems: "center", padding: "14px 20px",
                borderBottom: i < enquiries.length - 1 ? "1px solid var(--border)" : "none",
                background: isResolved ? "rgba(16,185,129,0.03)" : "transparent",
                transition: "background 0.15s ease",
              }}
                onMouseEnter={e2 => e2.currentTarget.style.background = "var(--surface-2)"}
                onMouseLeave={e2 => e2.currentTarget.style.background = isResolved ? "rgba(16,185,129,0.03)" : "transparent"}
              >
                {/* Name */}
                <div style={{ fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.875rem", color: "var(--text-primary)" }}>
                  {e.name}
                </div>
                {/* Email */}
                <div style={{ fontFamily: "var(--font-body)", fontSize: "0.82rem", color: "var(--text-secondary)", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                  {e.email}
                </div>
                {/* Phone */}
                <div style={{ fontFamily: "var(--font-body)", fontSize: "0.82rem", color: "var(--text-muted)" }}>
                  {e.phoneNumber || "—"}
                </div>
                {/* Message */}
                <div style={{
                  fontFamily: "var(--font-body)", fontSize: "0.82rem", color: "var(--text-secondary)",
                  overflow: "hidden", display: "-webkit-box", WebkitLineClamp: 2,
                  WebkitBoxOrient: "vertical", lineHeight: 1.4
                }}>
                  {e.message}
                </div>
                {/* Date */}
                <div style={{ fontFamily: "var(--font-body)", fontSize: "0.78rem", color: "var(--text-muted)" }}>
                  {e.createdAt ? new Date(e.createdAt).toLocaleDateString("en-IN", { day: "2-digit", month: "short", year: "numeric" }) : "—"}
                </div>
                {/* Status & Action */}
                <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
                  <span style={{
                    display: "inline-flex", alignItems: "center", gap: 4,
                    padding: "3px 8px", borderRadius: 20, fontSize: "0.72rem", fontWeight: 700,
                    fontFamily: "var(--font-display)",
                    background: isResolved ? "rgba(16,185,129,0.12)" : "rgba(245,158,11,0.12)",
                    color: isResolved ? "#059669" : "#D97706",
                  }}>
                    {isResolved ? <FiCheckCircle size={10} /> : <FiClock size={10} />}
                    {isResolved ? "Resolved" : "Pending"}
                  </span>
                  {!isResolved && (
                    <button
                      onClick={() => handleResolve(e.id)}
                      style={{
                        display: "inline-flex", alignItems: "center", gap: 4,
                        background: "var(--primary)", color: "#fff", border: "none",
                        borderRadius: 6, padding: "4px 10px", cursor: "pointer",
                        fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.72rem",
                        transition: "opacity 0.15s ease",
                      }}
                      onMouseEnter={ev => ev.currentTarget.style.opacity = "0.85"}
                      onMouseLeave={ev => ev.currentTarget.style.opacity = "1"}
                    >
                      <FiCheckCircle size={11} /> Mark Resolved
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Add Enquiry Modal */}
      {showAddModal && (
        <div style={{
          position: "fixed", top: 0, left: 0, right: 0, bottom: 0,
          background: "rgba(0,0,0,0.5)", backdropFilter: "blur(4px)",
          display: "flex", alignItems: "center", justifyContent: "center", zIndex: 9999
        }}>
          <div style={{
            background: "#fff", borderRadius: 16, padding: 32, width: "100%", maxWidth: 450,
            boxShadow: "0 20px 40px rgba(0,0,0,0.2)", position: "relative"
          }}>
            <button onClick={() => setShowAddModal(false)} style={{
              position: "absolute", top: 16, right: 16, background: "none", border: "none", fontSize: 24, cursor: "pointer", color: "#9898B8"
            }}>×</button>
            
            <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.5rem", fontWeight: 800, color: "#1A1A3E", marginBottom: 8 }}>Add New Enquiry</h2>
            <p style={{ color: "#5A5A82", fontSize: "0.9rem", marginBottom: 24 }}>Manually enter a new prospective student enquiry.</p>

            {submitSuccess ? (
              <div style={{ padding: 20, background: "#D1FAE5", color: "#065F46", borderRadius: 8, textAlign: "center" }}>
                <span style={{ fontSize: 24, display: "block", marginBottom: 8 }}>✅</span>
                Enquiry has been added successfully!
              </div>
            ) : (
              <form onSubmit={handleAddEnquiry} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
                <div>
                  <label className="label">Full Name</label>
                  <input className="input-field" type="text" placeholder="John Doe" required
                    value={newEnquiry.name} onChange={(e) => setNewEnquiry({ ...newEnquiry, name: e.target.value })} />
                </div>
                
                <div>
                  <label className="label">Email Address</label>
                  <input className="input-field" type="email" placeholder="john@example.com" required
                    value={newEnquiry.email} onChange={(e) => setNewEnquiry({ ...newEnquiry, email: e.target.value })} />
                </div>
                
                <div>
                  <label className="label">Phone Number</label>
                  <input className="input-field" type="tel" placeholder="+1 234 567 8900"
                    value={newEnquiry.phoneNumber} onChange={(e) => setNewEnquiry({ ...newEnquiry, phoneNumber: e.target.value })} />
                </div>
                
                <div>
                  <label className="label">Message</label>
                  <textarea className="input-field" placeholder="Enquiry details..." required rows={4}
                    style={{ resize: "vertical", padding: "12px 16px" }}
                    value={newEnquiry.message} onChange={(e) => setNewEnquiry({ ...newEnquiry, message: e.target.value })} />
                </div>
                
                <button type="submit" className="btn-primary" disabled={isSubmitting}
                  style={{ width: "100%", justifyContent: "center", padding: "12px", marginTop: 8 }}>
                  {isSubmitting ? "Submitting..." : "Submit Enquiry"}
                </button>
              </form>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
