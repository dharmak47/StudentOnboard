import React, { useState, useEffect } from "react";
import { studentApi } from "../services/api";
import { studentProgressApi } from "../services/studentProgressApi";
import { useToast } from "../context/ToastContext";
import { ProgressBar } from "../components/common/ProgressBar";

export default function StudentProgressTrackingPage() {
  const toast = useToast();
  const [loading, setLoading] = useState(true);
  const [registrations, setRegistrations] = useState([]);
  const [progressMap, setProgressMap] = useState({});
  const [selectedCourseId, setSelectedCourseId] = useState(null);

  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      try {
        const regsRes = await studentApi.getRegisteredCourses();
        if (regsRes.data) {
          setRegistrations(regsRes.data);

          // Fetch progress for all courses (both active and completed)
          const progressData = {};
          for (const reg of regsRes.data) {
            try {
              const progressRes = await studentProgressApi.getProgress(reg.registrationId || reg.id);
              if (progressRes.data) {
                progressData[reg.id] = progressRes.data;
              }
            } catch (err) {
              // Progress not available for this course - will show fallback message
              console.log(`Progress not available for course ${reg.courseName}`);
            }
          }
          setProgressMap(progressData);

          // Set first course as selected if available
          if (regsRes.data.length > 0) {
            setSelectedCourseId(regsRes.data[0].id);
          }
        }
      } catch (err) {
        toast.error(err.message || "Failed to load progress.");
      } finally {
        setLoading(false);
      }
    };
    fetch();
  }, [toast]);

  const selectedCourse = registrations.find(r => r.id === selectedCourseId);
  const selectedProgress = selectedCourse ? progressMap[selectedCourse.id] : null;

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
        📈 My Progress
      </h1>

      {registrations && registrations.length > 0 ? (
        <div style={{ display: "grid", gridTemplateColumns: "250px 1fr", gap: 24 }}>
          {/* Courses List */}
          <div className="card" style={{ padding: 16, height: "fit-content" }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "0.95rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 12 }}>
              My Courses
            </h3>
            <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
              {registrations.map((course) => {
                const progress = progressMap[course.id];
                const isSelected = selectedCourseId === course.id;
                return (
                  <button
                    key={course.id}
                    onClick={() => setSelectedCourseId(course.id)}
                    style={{
                      padding: 12,
                      background: isSelected ? "rgba(91,91,214,0.15)" : "transparent",
                      border: `1px solid ${isSelected ? "var(--primary)" : "var(--border-light)"}`,
                      borderRadius: 8,
                      cursor: "pointer",
                      textAlign: "left",
                      transition: "var(--transition)"
                    }}
                  >
                    <div style={{ fontSize: "0.85rem", fontWeight: isSelected ? 700 : 600, color: "var(--text-primary)" }}>
                      {course.courseName}
                    </div>
                    <div style={{ fontSize: "0.7rem", color: "var(--text-muted)", marginTop: 4 }}>
                      {course.isCompleted ? "✓ Completed" : progress ? `${progress.progressPercentage.toFixed(0)}% complete` : "—"}
                    </div>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Progress Details */}
          <div>
            {selectedCourse ? (
              <>
                {selectedCourse.isCompleted ? (
                  <div className="card" style={{ padding: 32, textAlign: "center" }}>
                    <div style={{ fontSize: 48, marginBottom: 16 }}>🎉</div>
                    <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.3rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 8 }}>
                      Course Completed!
                    </h2>
                    <p style={{ color: "var(--text-muted)", marginBottom: 16 }}>
                      You have successfully completed the <strong>{selectedCourse.courseName}</strong> course.
                    </p>
                    <button
                      style={{
                        padding: "10px 20px",
                        background: "var(--primary)",
                        color: "#fff",
                        border: "none",
                        borderRadius: 6,
                        cursor: "pointer",
                        fontWeight: 600
                      }}
                    >
                      📜 View Certificate
                    </button>
                  </div>
                ) : selectedProgress ? (
                  <div>
                    {/* Course Header */}
                    <div className="card animate-fadeUp" style={{ padding: 24, marginBottom: 20 }}>
                      <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.3rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 16 }}>
                        {selectedCourse.courseName}
                      </h2>

                      {/* Overall Progress */}
                      <div style={{ marginBottom: 24 }}>
                        <div style={{ marginBottom: 8, display: "flex", justifyContent: "space-between" }}>
                          <span style={{ fontSize: "0.9rem", fontWeight: 600, color: "var(--text-primary)" }}>Overall Progress</span>
                          <span style={{ fontSize: "0.9rem", fontWeight: 700, color: "var(--primary)" }}>
                            {selectedProgress.progressPercentage.toFixed(1)}%
                          </span>
                        </div>
                        <ProgressBar
                          percentage={selectedProgress.progressPercentage}
                          height="12px"
                          animated={true}
                        />
                      </div>

                      {/* Key Metrics */}
                      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(150px, 1fr))", gap: 12 }}>
                        {[
                          { label: "Completed Modules", value: `${selectedProgress.completedModules || 0}/${selectedProgress.totalModules || 0}` },
                          { label: "Days Remaining", value: selectedProgress.daysRemaining > 0 ? selectedProgress.daysRemaining : "—" },
                          { label: "Completion Date", value: new Date(selectedProgress.expectedCompletionDate).toLocaleDateString() },
                        ].map((metric) => (
                          <div key={metric.label} style={{ padding: 12, backgroundColor: "var(--surface-2)", borderRadius: 6 }}>
                            <div style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginBottom: 4 }}>
                              {metric.label}
                            </div>
                            <div style={{ fontSize: "1.1rem", fontWeight: 700, color: "var(--text-primary)" }}>
                              {metric.value}
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>

                    {/* Modules Progress */}
                    {selectedProgress.modules && selectedProgress.modules.length > 0 && (
                      <div className="card animate-fadeUp" style={{ padding: 24 }}>
                        <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 16 }}>
                          Modules Progress
                        </h3>
                        <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
                          {selectedProgress.modules.map((module, idx) => (
                            <div key={idx} style={{ padding: 12, backgroundColor: "var(--surface-2)", borderRadius: 8 }}>
                              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 8 }}>
                                <span style={{ fontWeight: 600, color: "var(--text-primary)" }}>
                                  {module.name || `Module ${idx + 1}`}
                                </span>
                                <span style={{
                                  background: module.completed ? "#DCFCE7" : "#FEF3C7",
                                  color: module.completed ? "#15803D" : "#92400E",
                                  padding: "4px 10px",
                                  borderRadius: 4,
                                  fontSize: "0.7rem",
                                  fontWeight: 600
                                }}>
                                  {module.completed ? "✓ Completed" : "In Progress"}
                                </span>
                              </div>
                              {module.progress !== undefined && (
                                <ProgressBar
                                  percentage={module.progress}
                                  height="8px"
                                  animated={!module.completed}
                                />
                              )}
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* At Risk Alert */}
                    {selectedProgress.progressPercentage < 30 && (
                      <div style={{
                        marginTop: 20,
                        padding: 16,
                        backgroundColor: "#FFF3CD",
                        border: "1px solid #FFC107",
                        borderRadius: 8,
                        color: "#856404"
                      }}>
                        <strong>⚠️ Falling Behind:</strong> You are at {selectedProgress.progressPercentage.toFixed(1)}% progress. Consider reaching out for support.
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="card" style={{ padding: 40, textAlign: "center" }}>
                    <div style={{ fontSize: 48, marginBottom: 16 }}>📊</div>
                    <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 8 }}>
                      Progress Tracking Coming Soon
                    </h3>
                    <p style={{ color: "var(--text-muted)", marginBottom: 16 }}>
                      Detailed progress tracking data is not yet available for this course. Your admin will enable tracking once the course begins.
                    </p>
                    <div style={{ fontSize: "0.85rem", color: "var(--text-secondary)", padding: 12, background: "var(--surface-2)", borderRadius: 6 }}>
                      In the meantime, check your dashboard for course details and completion status updates.
                    </div>
                  </div>
                )}
              </>
            ) : (
              <div className="card" style={{ padding: 40, textAlign: "center" }}>
                <div style={{ fontSize: 48, marginBottom: 16 }}>📋</div>
                <p style={{ color: "var(--text-muted)" }}>
                  Select a course to view detailed progress.
                </p>
              </div>
            )}
          </div>
        </div>
      ) : (
        <div className="card animate-fadeUp" style={{ padding: 60, textAlign: "center" }}>
          <div style={{ fontSize: 48, marginBottom: 16 }}>📚</div>
          <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 8 }}>
            No Courses Enrolled
          </h3>
          <p style={{ color: "var(--text-muted)", fontSize: "0.9rem" }}>
            Register for a course to start tracking your progress.
          </p>
        </div>
      )}
    </div>
  );
}
