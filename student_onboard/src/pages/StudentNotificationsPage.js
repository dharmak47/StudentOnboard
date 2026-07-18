import React, { useState, useEffect } from "react";
import { studentApi } from "../services/api";
import { useToast } from "../context/ToastContext";
import { FiBellOff } from "react-icons/fi";

const NOTIF_ICONS = {
  student_registration: "🎓",
  NewRegistration: "🎓",
  course_enrollment: "📚",
  CourseRegistration: "📚",
  approval: "✅",
  StudentApproved: "✅",
  denial: "❌",
  StudentDenied: "❌",
  payment: "💳",
  PaymentUpdate: "💳",
  AdminMessage: "📢",
  BirthdayWish: "🎂",
  default: "🔔",
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

export default function StudentNotificationsPage() {
  const toast = useToast();
  const [notifications, setNotifications] = useState([]);
  const [filter, setFilter] = useState("all");
  const [loading, setLoading] = useState(true);
  const [expandedNotif, setExpandedNotif] = useState(null);

  // Fetch notifications on mount
  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      try {
        const res = await studentApi.getNotifications();
        setNotifications(res.data || []);
      } catch (err) {
        toast.error(err.message || "Failed to load notifications.");
        setNotifications([]);
      } finally {
        setLoading(false);
      }
    };
    fetch();
  }, [toast]);

  const markRead = async (id) => {
    try {
      await studentApi.markNotificationRead(id);
      setNotifications((prev) => prev.map((n) => n.id === id ? { ...n, isRead: true } : n));
    } catch {
      // silently fail
    }
  };

  const filtered = filter === "unread"
    ? notifications.filter((n) => !n.isRead)
    : notifications;

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  return (
    <div className="page">
      {/* Header with stats */}
      <div style={{ marginBottom: 24 }}>
        <h1 style={{ fontFamily: "var(--font-display)", fontSize: "1.5rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: 8 }}>
          Notifications
        </h1>
        {unreadCount > 0 && (
          <p style={{ color: "var(--text-muted)", fontSize: "0.9rem" }}>
            You have <strong>{unreadCount}</strong> unread notification{unreadCount !== 1 ? "s" : ""}
          </p>
        )}
      </div>

      {/* Filter tabs */}
      <div style={{ display: "flex", gap: 8, marginBottom: 24, borderBottom: "1px solid var(--border)", paddingBottom: 0 }}>
        {[
          { id: "all", label: `All (${notifications.length})` },
          { id: "unread", label: `Unread (${unreadCount})` },
        ].map((f) => (
          <button
            key={f.id}
            onClick={() => setFilter(f.id)}
            style={{
              padding: "12px 16px", border: "none", background: "transparent", cursor: "pointer",
              fontFamily: "var(--font-display)", fontWeight: filter === f.id ? 700 : 500,
              color: filter === f.id ? "var(--primary)" : "var(--text-muted)",
              borderBottom: filter === f.id ? "2px solid var(--primary)" : "none",
              fontSize: "0.9rem", transition: "var(--transition)",
            }}
          >
            {f.label}
          </button>
        ))}
      </div>

      {/* Notifications list */}
      {loading ? (
        <div style={{ textAlign: "center", padding: 60, color: "var(--text-muted)" }}>
          Loading...
        </div>
      ) : filtered.length === 0 ? (
        <div className="card" style={{ padding: 60, textAlign: "center" }}>
          <div style={{ fontSize: 48, marginBottom: 16 }}>
            <FiBellOff size={48} />
          </div>
          <p style={{ color: "var(--text-muted)", fontSize: "0.9rem" }}>
            {filter === "unread" ? "No unread notifications" : "No notifications yet"}
          </p>
        </div>
      ) : (
        <div className="card" style={{ padding: 0, overflow: "hidden" }}>
          {filtered.map((notif, i) => {
            const isExpanded = expandedNotif === notif.id;
            return (
              <div
                key={notif.id}
                onClick={() => {
                  if (!notif.isRead) markRead(notif.id);
                  setExpandedNotif(isExpanded ? null : notif.id);
                }}
                style={{
                  padding: "16px 20px", cursor: "pointer",
                  display: "flex", gap: 12, alignItems: "flex-start",
                  background: isExpanded ? "var(--surface-3)" : notif.isRead ? "transparent" : "var(--surface-2)",
                  borderBottom: i < filtered.length - 1 ? "1px solid var(--border-light)" : "none",
                  transition: "var(--transition)",
                  animation: `fadeUp 0.25s ease both`,
                  animationDelay: `${i * 0.03}s`,
                }}
              >
                <div style={{ width: 36, height: 36, borderRadius: 10, background: notif.isRead ? "var(--surface-3)" : "var(--primary-light, #EEF2FF)", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 16, flexShrink: 0 }}>
                  {NOTIF_ICONS[notif.type] || NOTIF_ICONS.default}
                </div>
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontFamily: "var(--font-display)", fontSize: "0.85rem", fontWeight: notif.isRead ? 500 : 700, color: "var(--text-primary)", marginBottom: 2 }}>
                    {notif.title}
                  </div>
                  <div style={{
                    fontSize: "0.8rem", color: "var(--text-muted)", lineHeight: 1.4,
                    ...(isExpanded
                      ? { whiteSpace: "pre-wrap", wordBreak: "break-word" }
                      : { overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }),
                  }}>
                    {notif.message}
                  </div>
                  <div style={{ fontSize: "0.7rem", color: "var(--text-muted)", marginTop: 4 }}>
                    {timeAgo(notif.createdAt)}
                  </div>
                </div>
                {!notif.isRead && (
                  <div style={{ width: 8, height: 8, borderRadius: "50%", background: "#5B5BD6", flexShrink: 0, marginTop: 6 }} />
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
