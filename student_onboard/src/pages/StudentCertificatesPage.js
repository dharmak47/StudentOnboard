import React, { useState, useEffect } from "react";
import { studentApi } from "../services/api";
import { useToast } from "../context/ToastContext";

export default function StudentCertificatesPage() {
  const toast = useToast();
  const [loading, setLoading] = useState(true);
  const [completedCourses, setCompletedCourses] = useState([]);

  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      try {
        const res = await studentApi.getDashboard();
        if (res.data && res.data.completedCourses) {
          setCompletedCourses(res.data.completedCourses);
        }
      } catch (err) {
        toast.error(err.message || "Failed to load certificates.");
      } finally {
        setLoading(false);
      }
    };
    fetch();
  }, [toast]);

  const handleDownloadCertificate = (course) => {
    // Create a simple certificate preview/download
    toast.success(`Certificate for "${course.courseName}" is ready to download!`);
    // In production, this would generate/download an actual PDF certificate
  };

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
        📜 My Certificates
      </h1>

      {completedCourses && completedCourses.length > 0 ? (
        <>
          <p style={{ color: "var(--text-muted)", marginBottom: 24 }}>
            You have completed {completedCourses.length} course{completedCourses.length > 1 ? "s" : ""}. Download your certificates below.
          </p>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: 20 }}>
            {completedCourses.map((course, idx) => (
              <div key={idx} className="card" style={{ padding: 24, display: "flex", flexDirection: "column" }}>
                {/* Certificate Icon */}
                <div style={{ fontSize: 48, marginBottom: 16, textAlign: "center" }}>🏆</div>

                <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 8 }}>
                  {course.courseName}
                </h3>

                <p style={{ color: "var(--text-muted)", fontSize: "0.85rem", marginBottom: 4 }}>
                  Duration: <span style={{ color: "var(--text-secondary)" }}>{course.courseDuration || "—"}</span>
                </p>

                <p style={{ color: "var(--text-muted)", fontSize: "0.85rem", marginBottom: 16 }}>
                  Completed: <span style={{ color: "var(--text-secondary)", fontWeight: 600 }}>
                    {course.completedAt ? new Date(course.completedAt).toLocaleDateString() : "—"}
                  </span>
                </p>

                <div style={{ marginTop: "auto", display: "flex", gap: 10 }}>
                  <button
                    onClick={() => handleDownloadCertificate(course)}
                    style={{
                      flex: 1,
                      padding: "10px 16px",
                      background: "var(--primary)",
                      color: "#fff",
                      border: "none",
                      borderRadius: 6,
                      cursor: "pointer",
                      fontSize: "0.85rem",
                      fontWeight: 600,
                      transition: "var(--transition)"
                    }}
                  >
                    ⬇️ Download PDF
                  </button>
                  <button
                    style={{
                      padding: "10px 16px",
                      background: "rgba(91,91,214,0.1)",
                      color: "var(--primary)",
                      border: "1px solid var(--primary)",
                      borderRadius: 6,
                      cursor: "pointer",
                      fontSize: "0.85rem",
                      fontWeight: 600,
                      transition: "var(--transition)"
                    }}
                  >
                    👁️ View
                  </button>
                </div>
              </div>
            ))}
          </div>
        </>
      ) : (
        <div className="card animate-fadeUp" style={{ padding: 60, textAlign: "center" }}>
          <div style={{ fontSize: 48, marginBottom: 16 }}>📚</div>
          <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 8 }}>
            No Certificates Yet
          </h3>
          <p style={{ color: "var(--text-muted)", fontSize: "0.9rem", marginBottom: 20 }}>
            Complete a course to earn a certificate. Keep learning!
          </p>
        </div>
      )}
    </div>
  );
}
