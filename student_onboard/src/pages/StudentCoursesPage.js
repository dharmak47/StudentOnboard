import React, { useState, useEffect } from "react";
import { publicCoursesApi, studentApi } from "../services/api";
import { useToast } from "../context/ToastContext";

const StatusBadge = ({ status }) => {
  const styles = {
    pending: { bg: "#FEF3C7", color: "#92400E", border: "#FCD34D" },
    approved: { bg: "#DCFCE7", color: "#15803D", border: "#86EFAC" },
    completed: { bg: "#DBEAFE", color: "#0C4A6E", border: "#7DD3FC" },
    blocked: { bg: "#FEE2E2", color: "#B91C1C", border: "#FECACA" },
  };
  const style = styles[status?.toLowerCase()] || styles.pending;
  return (
    <span style={{
      background: style.bg, color: style.color, border: `1px solid ${style.border}`,
      borderRadius: 6, padding: "4px 10px", fontSize: "0.75rem", fontWeight: 600,
      textTransform: "capitalize",
    }}>
      {status || "Pending"}
    </span>
  );
};

export default function StudentCoursesPage({ onNavigate }) {
  const toast = useToast();
  const [tab, setTab] = useState("browse");
  const [browseCourses, setBrowseCourses] = useState([]);
  const [registrations, setRegistrations] = useState([]);
  const [browsing, setBrowsing] = useState(true);
  const [registering, setRegistering] = useState(false);
  const [loading, setLoading] = useState(false);

  // Fetch both on mount
  useEffect(() => {
    const fetch = async () => {
      try {
        const [coursesRes, regsRes] = await Promise.all([
          publicCoursesApi.getAll(),
          studentApi.getRegisteredCourses(),
        ]);
        setBrowseCourses(coursesRes.data || []);
        setRegistrations(regsRes.data || []);
      } catch (err) {
        toast.error(err.message || "Failed to load courses.");
      } finally {
        setBrowsing(false);
      }
    };
    fetch();
  }, [toast]);

  const handleRegister = async (courseId) => {
    setRegistering(true);
    try {
      await studentApi.registerForCourse(courseId);
      toast.success("Registered successfully!");
      // Refresh registrations
      const regsRes = await studentApi.getRegisteredCourses();
      setRegistrations(regsRes.data || []);
    } catch (err) {
      toast.error(err.message || "Failed to register.");
    } finally {
      setRegistering(false);
    }
  };

  const isRegistered = (courseId) => {
    return registrations.some((r) => r.courseId === courseId || r.id === courseId);
  };

  return (
    <div className="page">
      {/* Tabs */}
      <div style={{ display: "flex", gap: 8, marginBottom: 24, borderBottom: "1px solid var(--border)", paddingBottom: 0 }}>
        {[
          { id: "browse", label: "Browse Courses" },
          { id: "registrations", label: "My Registrations" },
        ].map((t) => (
          <button
            key={t.id}
            onClick={() => setTab(t.id)}
            style={{
              padding: "12px 16px", border: "none", background: "transparent", cursor: "pointer",
              fontFamily: "var(--font-display)", fontWeight: tab === t.id ? 700 : 500,
              color: tab === t.id ? "var(--primary)" : "var(--text-muted)",
              borderBottom: tab === t.id ? "2px solid var(--primary)" : "none",
              fontSize: "0.95rem", transition: "var(--transition)",
            }}
          >
            {t.label}
          </button>
        ))}
      </div>

      {/* Browse Courses Tab */}
      {tab === "browse" && (
        <div>
          {browsing ? (
            <div style={{ textAlign: "center", color: "var(--text-muted)", padding: 40 }}>Loading...</div>
          ) : browseCourses.length === 0 ? (
            <div style={{ textAlign: "center", padding: 60 }}>
              <div style={{ fontSize: 48, marginBottom: 16 }}>📚</div>
              <p style={{ color: "var(--text-muted)" }}>No courses available</p>
            </div>
          ) : (
            <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(260px, 1fr))", gap: 16 }}>
              {browseCourses.map((course) => {
                const registered = isRegistered(course.id);
                return (
                  <div key={course.id} className="card animate-fadeUp" style={{ padding: 16, display: "flex", flexDirection: "column" }}>
                    <div style={{ fontSize: 32, marginBottom: 8 }}>{course.thumbnail}</div>
                    <h4 style={{ fontFamily: "var(--font-display)", fontSize: "0.95rem", fontWeight: 700, marginBottom: 4, color: "var(--text-primary)" }}>
                      {course.title}
                    </h4>
                    <p style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginBottom: 8 }}>
                      by {course.instructor}
                    </p>
                    <div style={{ display: "flex", gap: 8, marginBottom: 12, flexWrap: "wrap" }}>
                      <span style={{ fontSize: "0.75rem", background: "var(--surface-2)", padding: "4px 8px", borderRadius: 4 }}>
                        {course.category}
                      </span>
                      <span style={{ fontSize: "0.75rem", background: "var(--surface-2)", padding: "4px 8px", borderRadius: 4 }}>
                        {course.duration}
                      </span>
                    </div>
                    <div style={{ marginBottom: 12, flex: 1 }}>
                      <div style={{ fontSize: "0.85rem", color: "var(--text-secondary)" }}>
                        {course.offerPrice ? (
                          <>
                            <span style={{ textDecoration: "line-through", color: "var(--text-muted)" }}>₹{course.price}</span>
                            {" "}₹{course.offerPrice}
                          </>
                        ) : (
                          `₹${course.price}`
                        )}
                      </div>
                    </div>
                    <button
                      onClick={() => handleRegister(course.id)}
                      disabled={registered || registering}
                      style={{
                        width: "100%", padding: "8px 12px", border: "none", borderRadius: 6,
                        background: registered ? "var(--surface-3)" : "var(--primary)",
                        color: registered ? "var(--text-muted)" : "#fff",
                        cursor: registered ? "default" : "pointer", fontSize: "0.8rem", fontWeight: 600,
                      }}
                    >
                      {registered ? "Already Registered" : registering ? "Registering..." : "Register"}
                    </button>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      )}

      {/* My Registrations Tab */}
      {tab === "registrations" && (
        <div>
          {registrations.length === 0 ? (
            <div style={{ textAlign: "center", padding: 60 }}>
              <div style={{ fontSize: 48, marginBottom: 16 }}>📋</div>
              <p style={{ color: "var(--text-muted)", marginBottom: 20 }}>No registrations yet</p>
              <button
                onClick={() => setTab("browse")}
                style={{
                  background: "var(--primary)", color: "#fff", border: "none",
                  borderRadius: 8, padding: "10px 20px", cursor: "pointer", fontSize: "0.9rem",
                }}
              >
                Browse Courses
              </button>
            </div>
          ) : (
            <div className="card" style={{ padding: 20, overflowX: "auto" }}>
              <table style={{ width: "100%", borderCollapse: "collapse" }}>
                <thead>
                  <tr style={{ borderBottom: "1px solid var(--border)" }}>
                    <th style={{ textAlign: "left", padding: "12px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Course Name</th>
                    <th style={{ textAlign: "left", padding: "12px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Fees</th>
                    <th style={{ textAlign: "left", padding: "12px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Course Status</th>
                    <th style={{ textAlign: "left", padding: "12px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Payment Status</th>
                    <th style={{ textAlign: "left", padding: "12px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Registered Date</th>
                  </tr>
                </thead>
                <tbody>
                  {registrations.map((reg, i) => (
                    <tr key={reg.id || i} style={{ borderBottom: i < registrations.length - 1 ? "1px solid var(--border-light)" : "none" }}>
                      <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>
                        {reg.courseName}
                      </td>
                      <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>
                        ₹{reg.fees}
                      </td>
                      <td style={{ padding: "12px 0" }}>
                        <StatusBadge status={reg.isCompleted ? "completed" : reg.status || "pending"} />
                      </td>
                      <td style={{ padding: "12px 0" }}>
                        <StatusBadge status={reg.paymentStatus} />
                      </td>
                      <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>
                        {reg.registeredDate ? new Date(reg.registeredDate).toLocaleDateString() : "—"}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
