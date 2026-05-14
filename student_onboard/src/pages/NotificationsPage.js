// src/pages/NotificationsPage.js
import React, { useState, useEffect, useCallback } from "react";
import { useToast } from "../context/ToastContext";
import { notificationsApi, studentsApi } from "../services/api";
import { FiBell, FiBellOff } from "react-icons/fi";

const NOTIF_ICONS = {
  student_registration: "🎓", NewRegistration: "🎓",
  course_enrollment: "📚", CourseRegistration: "📚",
  approval: "✅", StudentApproved: "✅",
  denial: "❌", StudentDenied: "❌",
  payment: "💳", PaymentUpdate: "💳",
  AdminMessage: "📢", BirthdayWish: "🎂", default: "🔔",
};

function timeAgo(dateStr) {
  const diff = Date.now() - new Date(dateStr).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return "Just now";
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  const days = Math.floor(hrs / 24);
  return `${days}d ago`;
}

function Section({ title, description, children }) {
  return (
    <div className="card animate-fadeUp" style={{ padding: 28, marginBottom: 20 }}>
      <div style={{ marginBottom: 22, paddingBottom: 18, borderBottom: "1px solid var(--border)" }}>
        <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1rem", fontWeight: 700, marginBottom: 4 }}>{title}</h3>
        {description && <p style={{ color: "var(--text-muted)", fontSize: "0.83rem" }}>{description}</p>}
      </div>
      {children}
    </div>
  );
}

function Toggle({ label, description, value, onChange }) {
  return (
    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", padding: "14px 0", borderBottom: "1px solid var(--border-light)" }}>
      <div>
        <div style={{ fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.88rem", color: "var(--text-primary)" }}>{label}</div>
        {description && <div style={{ fontSize: "0.78rem", color: "var(--text-muted)", marginTop: 2 }}>{description}</div>}
      </div>
      <div onClick={() => onChange(!value)}
        style={{ width: 46, height: 26, borderRadius: 13, background: value ? "var(--primary)" : "var(--border)", cursor: "pointer", position: "relative", transition: "background 0.25s ease", flexShrink: 0 }}>
        <div style={{ position: "absolute", top: 3, left: value ? 23 : 3, width: 20, height: 20, borderRadius: "50%", background: "#fff", transition: "left 0.25s ease", boxShadow: "0 2px 6px rgba(0,0,0,0.15)" }} />
      </div>
    </div>
  );
}

export default function NotificationsPage() {
  const toast = useToast();

  // All notifications state
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [expandedNotif, setExpandedNotif] = useState(null);
  const [tab, setTab] = useState("all"); // all | unread | send | preferences

  // Send notification state
  const [msgForm, setMsgForm] = useState({ title: "", message: "", target: "all" });
  const [msgLoading, setMsgLoading] = useState(false);
  const [studentList, setStudentList] = useState([]);
  const [selectedStudents, setSelectedStudents] = useState([]);
  const [studentsLoaded, setStudentsLoaded] = useState(false);
  const [studentSearch, setStudentSearch] = useState("");

  // Notification preferences
  const [notifs, setNotifs] = useState({ newRegistration: true, pendingApproval: true, courseEnrollment: false, weeklyReport: true });

  const fetchNotifications = useCallback(async () => {
    setLoading(true);
    try {
      const res = await notificationsApi.getAll();
      setNotifications(res.data || []);
    } catch {
      setNotifications([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchNotifications(); }, [fetchNotifications]);

  const markRead = async (id) => {
    try {
      await notificationsApi.markAsRead(id);
      setNotifications((prev) => prev.map((n) => n.id === id ? { ...n, isRead: true } : n));
    } catch { /* silently fail */ }
  };

  // Load students when "selected" target is chosen
  const loadStudents = async () => {
    if (studentsLoaded) return;
    try {
      const res = await studentsApi.getAll({ limit: 200, status: "approved" });
      setStudentList(res.data || []);
      setStudentsLoaded(true);
    } catch { setStudentList([]); }
  };

  const handleTargetChange = (target) => {
    setMsgForm({ ...msgForm, target });
    if (target === "selected") loadStudents();
    if (target === "all") { setSelectedStudents([]); setStudentSearch(""); }
  };

  const toggleStudent = (id) => {
    setSelectedStudents((prev) => prev.includes(id) ? prev.filter((s) => s !== id) : [...prev, id]);
  };

  const handleSendNotification = async (e) => {
    e.preventDefault();
    if (!msgForm.title.trim() || !msgForm.message.trim()) { toast.error("Title and message are required."); return; }
    if (msgForm.target === "selected" && selectedStudents.length === 0) { toast.error("Select at least one student."); return; }

    setMsgLoading(true);
    try {
      await notificationsApi.send(msgForm.title, msgForm.message, msgForm.target === "selected" ? selectedStudents : null);
      toast.success("Notification sent successfully!");
      setMsgForm({ title: "", message: "", target: "all" });
      setSelectedStudents([]);
      fetchNotifications();
    } catch (err) {
      toast.error(err.message || "Failed to send notification.");
    } finally {
      setMsgLoading(false);
    }
  };

  const filteredNotifs = tab === "unread" ? notifications.filter((n) => !n.isRead) : notifications;

  const TABS = [
    { id: "all", label: "All Notifications", icon: <FiBell size={14} /> },
    { id: "unread", label: "Unread", icon: "🔵" },
    { id: "send", label: "Send Notification", icon: "📤" },
    { id: "preferences", label: "Preferences", icon: "⚙️" },
  ];

  return (
    <div className="page" style={{ display: "flex", flexDirection: "column", gap: 22 }}>
      {/* Tab bar */}
      <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
        {TABS.map((t) => (
          <button key={t.id} onClick={() => setTab(t.id)}
            style={{
              padding: "9px 18px", borderRadius: "var(--radius-md)",
              border: `1.5px solid ${tab === t.id ? "var(--primary)" : "var(--border)"}`,
              background: tab === t.id ? "var(--primary)" : "var(--surface)",
              color: tab === t.id ? "#fff" : "var(--text-secondary)",
              fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.84rem",
              cursor: "pointer", transition: "var(--transition)",
              display: "flex", alignItems: "center", gap: 6,
            }}>
            <span style={{ fontSize: "0.9rem" }}>{t.icon}</span> {t.label}
            {t.id === "unread" && notifications.filter((n) => !n.isRead).length > 0 && (
              <span style={{
                minWidth: 20, height: 20, borderRadius: 10, background: tab === t.id ? "rgba(255,255,255,0.3)" : "#EF4444",
                color: "#fff", fontSize: "0.68rem", fontWeight: 700, display: "flex", alignItems: "center", justifyContent: "center", padding: "0 5px",
              }}>
                {notifications.filter((n) => !n.isRead).length}
              </span>
            )}
          </button>
        ))}
      </div>

      {/* All / Unread Notifications */}
      {(tab === "all" || tab === "unread") && (
        <div className="card animate-fadeUp" style={{ overflow: "hidden" }}>
          <div style={{ padding: "16px 22px", borderBottom: "1px solid var(--border)", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1rem", fontWeight: 800, color: "var(--text-primary)", margin: 0 }}>
              {tab === "unread" ? "Unread Notifications" : "All Notifications"}
            </h3>
            <button className="btn-ghost" onClick={fetchNotifications} style={{ padding: "6px 12px", fontSize: "0.78rem" }}>
              Refresh
            </button>
          </div>
          <div style={{ maxHeight: 600, overflowY: "auto" }}>
            {loading ? (
              <div style={{ padding: 50, textAlign: "center", color: "var(--text-muted)", fontSize: "0.85rem" }}>Loading...</div>
            ) : filteredNotifs.length === 0 ? (
              <div style={{ padding: 50, textAlign: "center" }}>
                <div style={{ marginBottom: 10 }}><FiBellOff size={40} color="var(--text-muted)" /></div>
                <div style={{ color: "var(--text-muted)", fontSize: "0.88rem", fontFamily: "var(--font-body)" }}>
                  {tab === "unread" ? "No unread notifications" : "No notifications yet"}
                </div>
              </div>
            ) : (
              filteredNotifs.map((n, i) => {
                const isExpanded = expandedNotif === n.id;
                return (
                  <div key={n.id}
                    onClick={() => {
                      if (!n.isRead) markRead(n.id);
                      setExpandedNotif(isExpanded ? null : n.id);
                    }}
                    style={{
                      padding: "16px 22px", cursor: "pointer",
                      display: "flex", gap: 14, alignItems: "flex-start",
                      background: isExpanded ? "var(--surface-3)" : n.isRead ? "transparent" : "var(--surface-2)",
                      borderBottom: i < filteredNotifs.length - 1 ? "1px solid var(--border-light)" : "none",
                      transition: "var(--transition)",
                      animation: `fadeUp 0.25s ease both`,
                      animationDelay: `${i * 0.03}s`,
                    }}>
                    <div style={{
                      width: 40, height: 40, borderRadius: 12,
                      background: n.isRead ? "var(--surface-3)" : "var(--primary-light, #EEF2FF)",
                      display: "flex", alignItems: "center", justifyContent: "center",
                      fontSize: 18, flexShrink: 0,
                    }}>
                      {NOTIF_ICONS[n.type] || NOTIF_ICONS.default}
                    </div>
                    <div style={{ flex: 1, minWidth: 0 }}>
                      <div style={{
                        fontFamily: "var(--font-display)", fontSize: "0.88rem",
                        fontWeight: n.isRead ? 500 : 700, color: "var(--text-primary)", marginBottom: 3,
                      }}>{n.title}</div>
                      <div style={{
                        fontSize: "0.8rem", color: "var(--text-secondary)", lineHeight: 1.5,
                        ...(isExpanded
                          ? { whiteSpace: "pre-wrap", wordBreak: "break-word" }
                          : { overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }),
                      }}>{n.message}</div>
                      <div style={{ fontSize: "0.72rem", color: "var(--text-muted)", marginTop: 5 }}>{timeAgo(n.createdAt)}</div>
                    </div>
                    {!n.isRead && (
                      <div style={{ width: 9, height: 9, borderRadius: "50%", background: "#5B5BD6", flexShrink: 0, marginTop: 8 }} />
                    )}
                  </div>
                );
              })
            )}
          </div>
        </div>
      )}

      {/* Send Notification */}
      {tab === "send" && (
        <Section title="Send Notification" description="Draft and send a message to all or selected students.">
          <form onSubmit={handleSendNotification} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
            <div>
              <label className="label">Subject *</label>
              <input className="input-field" value={msgForm.title} onChange={(e) => setMsgForm({ ...msgForm, title: e.target.value })} placeholder="e.g. Important Announcement" required />
            </div>
            <div>
              <label className="label">Message *</label>
              <textarea className="input-field" rows={4} value={msgForm.message} onChange={(e) => setMsgForm({ ...msgForm, message: e.target.value })} placeholder="Write your message here..." style={{ resize: "vertical" }} required />
            </div>
            <div>
              <label className="label">Send To</label>
              <div style={{ display: "flex", gap: 10, marginTop: 6 }}>
                {[["all", "All Students"], ["selected", "Selected Students"]].map(([val, label]) => (
                  <button key={val} type="button" onClick={() => handleTargetChange(val)}
                    style={{ padding: "8px 18px", borderRadius: "var(--radius-md)", border: `1.5px solid ${msgForm.target === val ? "var(--primary)" : "var(--border)"}`, background: msgForm.target === val ? "var(--primary)" : "var(--surface)", color: msgForm.target === val ? "#fff" : "var(--text-secondary)", fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.82rem", cursor: "pointer", transition: "var(--transition)" }}>
                    {label}
                  </button>
                ))}
              </div>
            </div>
            {msgForm.target === "selected" && (
              <div>
                <label className="label">Select Students ({selectedStudents.length} selected)</label>
                <input
                  className="input-field"
                  value={studentSearch}
                  onChange={(e) => setStudentSearch(e.target.value)}
                  placeholder="Search students by name or email..."
                  style={{ marginTop: 6, marginBottom: 0 }}
                />
                <div style={{ maxHeight: 220, overflowY: "auto", border: "1px solid var(--border)", borderRadius: "0 0 var(--radius-md) var(--radius-md)", borderTop: "none" }}>
                  {studentList.length === 0 ? (
                    <div style={{ padding: 20, textAlign: "center", color: "var(--text-muted)", fontSize: "0.85rem" }}>No students found</div>
                  ) : (
                    studentList
                      .filter((s) => {
                        if (!studentSearch.trim()) return true;
                        const q = studentSearch.toLowerCase();
                        return (s.name || "").toLowerCase().includes(q) || (s.email || "").toLowerCase().includes(q);
                      })
                      .map((s) => (
                        <label key={s.id} style={{ display: "flex", alignItems: "center", gap: 10, padding: "10px 14px", cursor: "pointer", borderBottom: "1px solid var(--border-light)", background: selectedStudents.includes(s.id) ? "var(--surface-2)" : "transparent" }}>
                          <input type="checkbox" checked={selectedStudents.includes(s.id)} onChange={() => toggleStudent(s.id)} style={{ accentColor: "var(--primary)" }} />
                          <div style={{ flex: 1 }}>
                            <div style={{ fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.85rem" }}>{s.name}</div>
                            <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{s.email}</div>
                          </div>
                        </label>
                      ))
                  )}
                  {studentList.length > 0 && studentSearch.trim() && studentList.filter((s) => {
                    const q = studentSearch.toLowerCase();
                    return (s.name || "").toLowerCase().includes(q) || (s.email || "").toLowerCase().includes(q);
                  }).length === 0 && (
                    <div style={{ padding: 20, textAlign: "center", color: "var(--text-muted)", fontSize: "0.85rem" }}>No students match "{studentSearch}"</div>
                  )}
                </div>
              </div>
            )}
            <div style={{ display: "flex", justifyContent: "flex-end", marginTop: 4 }}>
              <button type="submit" className="btn-primary" disabled={msgLoading}>
                {msgLoading ? "Sending..." : "Send Notification"}
              </button>
            </div>
          </form>
        </Section>
      )}

      {/* Notification Preferences */}
      {tab === "preferences" && (
        <Section title="Notification Preferences" description="Choose what you get notified about.">
          <Toggle label="New Student Registration" description="Get notified when a student signs up." value={notifs.newRegistration} onChange={(v) => setNotifs({ ...notifs, newRegistration: v })} />
          <Toggle label="Pending Approvals" description="Alerts when students await review." value={notifs.pendingApproval} onChange={(v) => setNotifs({ ...notifs, pendingApproval: v })} />
          <Toggle label="Course Enrollment Updates" description="When a student enrolls in a course." value={notifs.courseEnrollment} onChange={(v) => setNotifs({ ...notifs, courseEnrollment: v })} />
          <Toggle label="Weekly Summary Report" description="A digest every Monday morning." value={notifs.weeklyReport} onChange={(v) => setNotifs({ ...notifs, weeklyReport: v })} />
          <div style={{ marginTop: 20, display: "flex", justifyContent: "flex-end" }}>
            <button className="btn-primary" onClick={() => toast.success("Notification preferences saved!")}>Save Preferences</button>
          </div>
        </Section>
      )}
    </div>
  );
}
