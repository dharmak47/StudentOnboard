import React, { useState, useEffect } from "react";
import { studentApi } from "../services/api";
import { studentProgressApi } from "../services/studentProgressApi";
import { useToast } from "../context/ToastContext";
import { ProgressBar } from "../components/common/ProgressBar";
import { ModuleProgress } from "../components/student/ModuleProgress";

export default function StudentDashboardPage({ onNavigate }) {
  const toast = useToast();

  const [loading, setLoading] = useState(true);
  const [data, setData] = useState(null);
  const [progressData, setProgressData] = useState(null);

  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      try {
        const res = await studentApi.getDashboard();
        setData(res.data);

        // Fetch progress for active course if exists
        if (res.data && res.data.courseStatus && res.data.courseStatus !== "Pending Payment") {
          try {
            // Try to fetch progress data
            const registrations = await studentApi.getRegisteredCourses();
            if (registrations.data && registrations.data.length > 0) {
              const activeReg = registrations.data.find(r => !r.isCompleted);
              if (activeReg) {
                const progressRes = await studentProgressApi.getProgress(activeReg.registrationId || activeReg.id);
                if (progressRes.data) {
                  setProgressData(progressRes.data);
                }
              }
            }
          } catch (err) {
            console.log("Progress data not available yet");
          }
        }
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
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(150px, 1fr))", gap: 16, marginBottom: 20 }}>
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

          {/* Progress Section */}
          {progressData && (
            <div style={{ marginTop: 20, paddingTop: 20, borderTop: "1px solid var(--border-light)" }}>
              <h4 style={{ fontSize: "0.95rem", fontWeight: 600, color: "var(--text-primary)", marginBottom: 16 }}>
                Course Progress
              </h4>

              {/* Progress Bar */}
              <ProgressBar
                percentage={progressData.progressPercentage}
                label={`${progressData.progressPercentage.toFixed(1)}% Complete`}
                height="12px"
                animated={true}
                showMilestones={true}
                milestones={[
                  { percentage: 0, label: "Start" },
                  { percentage: 33, label: "1/3" },
                  { percentage: 66, label: "2/3" },
                  { percentage: 100, label: "Complete" }
                ]}
              />

              {/* Days Remaining */}
              {progressData.daysRemaining > 0 && (
                <div style={{ marginTop: 16, display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(150px, 1fr))", gap: 12 }}>
                  <div style={{ padding: "12px", backgroundColor: "#f5f5f5", borderRadius: "6px" }}>
                    <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginBottom: "4px" }}>Days Remaining</div>
                    <div style={{ fontSize: "1.4rem", fontWeight: "700", color: "var(--primary)" }}>{progressData.daysRemaining}</div>
                  </div>
                  <div style={{ padding: "12px", backgroundColor: "#f5f5f5", borderRadius: "6px" }}>
                    <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginBottom: "4px" }}>Expected Completion</div>
                    <div style={{ fontSize: "0.95rem", fontWeight: "600", color: "var(--text-secondary)" }}>
                      {new Date(progressData.expectedCompletionDate).toLocaleDateString()}
                    </div>
                  </div>
                </div>
              )}

              {/* Module Progress */}
              {progressData.modules && progressData.modules.length > 0 && (
                <div style={{ marginTop: 20 }}>
                  <h5 style={{ fontSize: "0.9rem", fontWeight: "600", color: "var(--text-primary)", marginBottom: "12px" }}>
                    Modules ({progressData.completedModules}/{progressData.totalModules})
                  </h5>
                  <ModuleProgress
                    modules={progressData.modules}
                    currentModule={progressData.currentModule}
                    viewType="grid"
                  />

                </div>
              )}

              {/* At Risk Alert */}
              {progressData.progressPercentage < 30 && !progressData.isCompleted && (
                <div style={{
                  marginTop: 16,
                  padding: "12px",
                  backgroundColor: "#fff3cd",
                  border: "1px solid #ffc107",
                  borderRadius: "6px",
                  color: "#856404"
                }}>
                  <strong>⚠️ Falling Behind:</strong> You're at {progressData.progressPercentage.toFixed(1)}% progress. Consider reaching out for support.
                </div>
              )}
            </div>
          )}
        </div>
      )}

      {/* Completed courses table */}
      {data?.completedCourses && data.completedCourses.length > 0 && (
        <div className="card animate-fadeUp" style={{ padding: 24, marginBottom: 24 }}>
          <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 16 }}>
            🎉 Completed Courses
          </h3>
          <div style={{ overflowX: "auto" }}>
            <table style={{ width: "100%", borderCollapse: "collapse" }}>
              <thead>
                <tr style={{ borderBottom: "1px solid var(--border)" }}>
                  <th style={{ textAlign: "left", padding: "10px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Course Name</th>
                  <th style={{ textAlign: "left", padding: "10px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Duration</th>
                  <th style={{ textAlign: "left", padding: "10px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Completed Date</th>
                  <th style={{ textAlign: "left", padding: "10px 0", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>Status</th>
                </tr>
              </thead>
              <tbody>
                {data.completedCourses.map((course, i) => (
                  <tr key={i} style={{ borderBottom: i < data.completedCourses.length - 1 ? "1px solid var(--border-light)" : "none" }}>
                    <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>{course.courseName || "—"}</td>
                    <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>{course.courseDuration || "—"}</td>
                    <td style={{ padding: "12px 0", color: "var(--text-secondary)", fontSize: "0.9rem" }}>
                      {course.completedAt ? new Date(course.completedAt).toLocaleDateString() : "—"}
                    </td>
                    <td style={{ padding: "12px 0" }}>
                      <span style={{
                        background: "#DBEAFE", color: "#0C4A6E", border: "1px solid #7DD3FC",
                        borderRadius: 6, padding: "4px 10px", fontSize: "0.75rem", fontWeight: 600,
                        textTransform: "capitalize", display: "inline-block"
                      }}>
                        ✓ Completed
                      </span>
                    </td>
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
