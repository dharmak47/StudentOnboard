import React, { useState, useEffect } from "react";
import { studentApi } from "../services/api";
import { useToast } from "../context/ToastContext";

export default function StudentDashboardPage({ onNavigate }) {
  const toast = useToast();
  const [loading, setLoading] = useState(true);
  const [data, setData] = useState(null);

  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      try {
        const res = await studentApi.getDashboard();
        setData(res.data);
      } catch (err) {
        toast.error(err.message || "Failed to load dashboard.");
        setData(null);
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
      {/* Welcome heading */}
      <h1 style={{ fontFamily: "var(--font-display)", fontSize: "1.8rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: 32 }}>
        Welcome back, {data?.firstName}!
      </h1>

      {/* Stat cards */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))", gap: 16, marginBottom: 32 }}>
        {[
          { label: "Registered Courses", value: data?.registeredCoursesCount ?? 0 },
          { label: "Course Status", value: data?.courseStatus ?? "—" },
          { label: "Payment Status", value: data?.paymentStatus ?? "—" },
        ].map((stat) => (
          <div key={stat.label} className="card" style={{ padding: 20 }}>
            <div style={{ color: "var(--text-muted)", fontSize: "0.8rem", marginBottom: 8 }}>{stat.label}</div>
            <div style={{ fontSize: "1.8rem", fontWeight: 700, color: "var(--text-primary)" }}>{stat.value}</div>
          </div>
        ))}
      </div>

      {/* Active course card */}
      {data?.courseName && (
        <div className="card animate-fadeUp" style={{ padding: 24, marginBottom: 24 }}>
          <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 16 }}>
            Active Course
          </h3>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(150px, 1fr))", gap: 16 }}>
            {[
              { label: "Course Name", value: data.courseName },
              { label: "Duration", value: data.courseDuration || "—" },
              { label: "Batch Timing", value: data.batchTiming || "—" },
              { label: "Enrolled Date", value: data.enrolledDate || "—" },
              { label: "Amount Due", value: data.amountDue ? `₹${data.amountDue}` : "—" },
            ].map((item) => (
              <div key={item.label}>
                <div style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginBottom: 4 }}>{item.label}</div>
                <div style={{ fontSize: "0.95rem", fontWeight: 600, color: "var(--text-secondary)" }}>{item.value}</div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Completed courses table */}
      {data?.completedCourses && data.completedCourses.length > 0 && (
        <div className="card animate-fadeUp" style={{ padding: 24, marginBottom: 24 }}>
          <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 16 }}>
            Completed Courses
          </h3>
          <div style={{ overflowX: "auto" }}>
            <table style={{ width: "100%", borderCollapse: "collapse" }}>
              <thead>
                <tr style={{ borderBottom: "1px solid var(--border)" }}>
                  <th style={{ textAlign: "left", padding: "10px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Course Name</th>
                  <th style={{ textAlign: "left", padding: "10px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Duration</th>
                  <th style={{ textAlign: "left", padding: "10px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Completed At</th>
                </tr>
              </thead>
              <tbody>
                {data.completedCourses.map((course, i) => (
                  <tr key={i} style={{ borderBottom: i < data.completedCourses.length - 1 ? "1px solid var(--border-light)" : "none" }}>
                    <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>{course.courseName || "—"}</td>
                    <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>{course.courseDuration || "—"}</td>
                    <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>{course.completedAt || "—"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Empty state */}
      {!data?.courseName && (!data?.completedCourses || data.completedCourses.length === 0) && (
        <div className="card animate-fadeUp" style={{ padding: 60, textAlign: "center" }}>
          <div style={{ fontSize: 48, marginBottom: 16 }}>📚</div>
          <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 8 }}>
            No courses yet
          </h3>
          <p style={{ color: "var(--text-muted)", fontSize: "0.9rem", marginBottom: 20 }}>
            Explore our course catalogue and register for a course to get started.
          </p>
          <button
            onClick={() => onNavigate("courses")}
            style={{
              background: "var(--primary)", color: "#fff", border: "none",
              borderRadius: 8, padding: "10px 20px", cursor: "pointer", fontSize: "0.9rem", fontWeight: 600,
            }}
          >
            Browse Courses
          </button>
        </div>
      )}
    </div>
  );
}
