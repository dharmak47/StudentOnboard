// src/components/layout/Topbar.js
import React, { useState, useEffect, useRef, useCallback } from "react";
import { useAuth } from "../../context/AuthContext";
import { notificationsApi } from "../../services/api";
import { FiBell, FiBellOff } from "react-icons/fi";

const PAGE_META = {
  dashboard:     { title: "Dashboard",          sub: "Welcome back! Here's your overview." },
  students:      { title: "Student Management", sub: "Review, approve, and manage enrolled students." },
  registrations: { title: "Course Registrations", sub: "Track student course registrations and payments." },
  courses:       { title: "Course Management",  sub: "Create and manage your course catalogue." },
  notifications: { title: "Notifications",      sub: "View and manage all your notifications." },
  faqs:          { title: "FAQs Management",    sub: "Create and manage frequently asked questions." },
  settings:      { title: "Settings",           sub: "Manage your account and preferences." },
};

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

export default function Topbar({ page }) {
  const { admin } = useAuth();
  const meta = PAGE_META[page] || PAGE_META.dashboard;
  const now = new Date().toLocaleDateString("en-IN", { weekday: "long", year: "numeric", month: "long", day: "numeric" });

  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState([]);
  const [showPanel, setShowPanel] = useState(false);
  const [showProfile, setShowProfile] = useState(false);
  const [loading, setLoading] = useState(false);
  const [expandedNotif, setExpandedNotif] = useState(null);
  const panelRef = useRef(null);
  const profileRef = useRef(null);
  const profilePic = localStorage.getItem("edu_admin_pic") || "";

  // Fetch unread count on mount and every 30s
  const fetchUnread = useCallback(async () => {
    try {
      const res = await notificationsApi.unreadCount();
      setUnreadCount(res.data?.count ?? res.data?.unreadCount ?? 0);
    } catch {
      // silently fail
    }
  }, []);

  useEffect(() => {
    fetchUnread();
    const interval = setInterval(fetchUnread, 15000); // Poll every 15s for faster updates
    return () => clearInterval(interval);
  }, [fetchUnread]);

  // Close panels on outside click
  useEffect(() => {
    const handler = (e) => {
      if (panelRef.current && !panelRef.current.contains(e.target)) setShowPanel(false);
      if (profileRef.current && !profileRef.current.contains(e.target)) setShowProfile(false);
    };
    if (showPanel || showProfile) document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, [showPanel, showProfile]);

  const togglePanel = async () => {
    setShowProfile(false);
    const opening = !showPanel;
    setShowPanel(opening);
    if (opening) {
      setLoading(true);
      try {
        const res = await notificationsApi.getAll();
        setNotifications(res.data || []);
      } catch {
        setNotifications([]);
      } finally {
        setLoading(false);
      }
    }
  };

  const toggleProfile = () => {
    setShowPanel(false);
    setShowProfile(!showProfile);
  };

  const markRead = async (id) => {
    try {
      await notificationsApi.markAsRead(id);
      setNotifications((prev) => prev.map((n) => n.id === id ? { ...n, isRead: true } : n));
      setUnreadCount((c) => Math.max(0, c - 1));
    } catch {
      // silently fail
    }
  };

  return (
    <header style={{
      height: 68, background: "var(--surface)", borderBottom: "1px solid var(--border)",
      display: "flex", alignItems: "center", paddingInline: 32, gap: 20, flexShrink: 0,
    }}>
      <div style={{ flex: 1 }}>
        <h1 style={{ fontFamily: "var(--font-display)", fontSize: "1.2rem", fontWeight: 800, color: "var(--text-primary)", lineHeight: 1 }}>{meta.title}</h1>
        <p style={{ color: "var(--text-muted)", fontSize: "0.78rem", marginTop: 3, fontFamily: "var(--font-body)" }}>{meta.sub}</p>
      </div>

      <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
        {/* Date pill */}
        <div style={{ background: "var(--surface-3)", border: "1px solid var(--border)", borderRadius: "var(--radius-full)", padding: "6px 14px", fontSize: "0.75rem", color: "var(--text-secondary)", fontFamily: "var(--font-body)", whiteSpace: "nowrap" }}>
          📅 {now}
        </div>

        {/* Notification bell */}
        <div ref={panelRef} style={{ position: "relative" }}>
          <button onClick={togglePanel} style={{ width: 38, height: 38, borderRadius: "var(--radius-md)", background: showPanel ? "var(--primary-light, #EEF2FF)" : "var(--surface-3)", border: `1px solid ${showPanel ? "var(--primary)" : "var(--border)"}`, cursor: "pointer", display: "flex", alignItems: "center", justifyContent: "center", position: "relative", transition: "var(--transition)" }}>
            <FiBell size={18} />
            {unreadCount > 0 && (
              <span style={{
                position: "absolute", top: -4, right: -4, minWidth: 18, height: 18, borderRadius: 9,
                background: "#EF4444", color: "#fff", fontSize: "0.65rem", fontWeight: 700,
                fontFamily: "var(--font-display)", display: "flex", alignItems: "center", justifyContent: "center",
                padding: "0 5px", border: "2px solid var(--surface)",
                animation: "pulse 2s infinite",
              }}>
                {unreadCount > 99 ? "99+" : unreadCount}
              </span>
            )}
          </button>

          {/* Notification Panel */}
          {showPanel && (
            <div style={{
              position: "absolute", top: "calc(100% + 8px)", right: 0, width: 440,
              background: "var(--surface)", border: "1px solid var(--border)", borderRadius: 16,
              boxShadow: "0 20px 60px rgba(0,0,0,0.15)", zIndex: 1000, overflow: "hidden",
              animation: "fadeUp 0.2s ease",
            }}>
              <div style={{ padding: "16px 20px", borderBottom: "1px solid var(--border)", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <div>
                  <h3 style={{ fontFamily: "var(--font-display)", fontSize: "0.95rem", fontWeight: 800, color: "var(--text-primary)", margin: 0 }}>Notifications</h3>
                  {unreadCount > 0 && (
                    <span style={{ fontSize: "0.72rem", color: "var(--text-muted)", fontFamily: "var(--font-body)" }}>{unreadCount} unread</span>
                  )}
                </div>
              </div>
              <div style={{ maxHeight: 480, overflowY: "auto" }}>
                {loading ? (
                  <div style={{ padding: 40, textAlign: "center", color: "var(--text-muted)", fontSize: "0.85rem" }}>Loading...</div>
                ) : notifications.length === 0 ? (
                  <div style={{ padding: 40, textAlign: "center" }}>
                    <div style={{ fontSize: 36, marginBottom: 8 }}><FiBellOff size={36} /></div>
                    <div style={{ color: "var(--text-muted)", fontSize: "0.85rem", fontFamily: "var(--font-body)" }}>No notifications yet</div>
                  </div>
                ) : (
                  notifications.map((n, i) => {
                    const isExpanded = expandedNotif === n.id;
                    return (
                    <div
                      key={n.id}
                      onClick={() => {
                        if (!n.isRead) markRead(n.id);
                        setExpandedNotif(isExpanded ? null : n.id);
                      }}
                      style={{
                        padding: "14px 20px", cursor: "pointer",
                        display: "flex", gap: 12, alignItems: "flex-start",
                        background: isExpanded ? "var(--surface-3)" : n.isRead ? "transparent" : "var(--surface-2)",
                        borderBottom: i < notifications.length - 1 ? "1px solid var(--border-light)" : "none",
                        transition: "var(--transition)",
                        animation: `fadeUp 0.25s ease both`,
                        animationDelay: `${i * 0.03}s`,
                      }}
                    >
                      <div style={{ width: 36, height: 36, borderRadius: 10, background: n.isRead ? "var(--surface-3)" : "var(--primary-light, #EEF2FF)", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 16, flexShrink: 0 }}>
                        {NOTIF_ICONS[n.type] || NOTIF_ICONS.default}
                      </div>
                      <div style={{ flex: 1, minWidth: 0 }}>
                        <div style={{ fontFamily: "var(--font-display)", fontSize: "0.82rem", fontWeight: n.isRead ? 500 : 700, color: "var(--text-primary)", marginBottom: 2 }}>{n.title}</div>
                        <div style={{
                          fontSize: "0.75rem", color: "var(--text-muted)", lineHeight: 1.4,
                          ...(isExpanded
                            ? { whiteSpace: "pre-wrap", wordBreak: "break-word" }
                            : { overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }),
                        }}>{n.message}</div>
                        <div style={{ fontSize: "0.68rem", color: "var(--text-muted)", marginTop: 4 }}>{timeAgo(n.createdAt)}</div>
                      </div>
                      {!n.isRead && (
                        <div style={{ width: 8, height: 8, borderRadius: "50%", background: "#5B5BD6", flexShrink: 0, marginTop: 6 }} />
                      )}
                    </div>
                    );
                  })
                )}
              </div>
            </div>
          )}
        </div>

        {/* Admin profile chip + dropdown */}
        {admin && (
          <div ref={profileRef} style={{ position: "relative" }}>
            <div
              onClick={toggleProfile}
              style={{
                display: "flex", alignItems: "center", gap: 9,
                background: showProfile ? "var(--primary-light, #EEF2FF)" : "var(--surface-3)",
                border: `1px solid ${showProfile ? "var(--primary)" : "var(--border)"}`,
                borderRadius: "var(--radius-full)", padding: "5px 14px 5px 6px", cursor: "pointer",
                transition: "var(--transition)",
              }}
            >
              {profilePic ? (
                <img src={profilePic} alt="Profile" style={{ width: 28, height: 28, borderRadius: "50%", objectFit: "cover" }} />
              ) : (
                <div style={{ width: 28, height: 28, borderRadius: "50%", background: "linear-gradient(135deg, #5B5BD6, #8B8FD4)", display: "flex", alignItems: "center", justifyContent: "center", color: "#fff", fontFamily: "var(--font-display)", fontWeight: 700, fontSize: "0.7rem" }}>
                  {admin.avatar || "AD"}
                </div>
              )}
              <span style={{ fontFamily: "var(--font-display)", fontSize: "0.8rem", fontWeight: 600, color: "var(--text-primary)" }}>{admin.name}</span>
              <span style={{ fontSize: 10, color: "var(--text-muted)", marginLeft: 2 }}>▼</span>
            </div>

            {/* Profile Dropdown */}
            {showProfile && (
              <div style={{
                position: "absolute", top: "calc(100% + 8px)", right: 0, width: 300,
                background: "var(--surface)", border: "1px solid var(--border)", borderRadius: 16,
                boxShadow: "0 20px 60px rgba(0,0,0,0.15)", zIndex: 1000, overflow: "hidden",
                animation: "fadeUp 0.2s ease",
              }}>
                {/* Profile header */}
                <div style={{ padding: "22px 20px", background: "linear-gradient(135deg, #5B5BD6, #8B8FD4)", textAlign: "center" }}>
                  {profilePic ? (
                    <img src={profilePic} alt="Profile" style={{ width: 52, height: 52, borderRadius: "50%", objectFit: "cover", margin: "0 auto 10px", display: "block", border: "3px solid rgba(255,255,255,0.3)" }} />
                  ) : (
                    <div style={{ width: 52, height: 52, borderRadius: "50%", background: "rgba(255,255,255,0.2)", display: "flex", alignItems: "center", justifyContent: "center", color: "#fff", fontFamily: "var(--font-display)", fontWeight: 800, fontSize: "1.1rem", margin: "0 auto 10px" }}>
                      {admin.avatar || "AD"}
                    </div>
                  )}
                  <div style={{ fontFamily: "var(--font-display)", fontWeight: 700, fontSize: "0.95rem", color: "#fff" }}>{admin.name}</div>
                  <div style={{ fontSize: "0.75rem", color: "rgba(255,255,255,0.7)", marginTop: 2 }}>{admin.email}</div>
                </div>

                {/* Profile details */}
                <div style={{ padding: "16px 20px" }}>
                  {[
                    ["Role", admin.role || "Admin"],
                    ["User ID", admin.id ? `${admin.id.substring(0, 8)}...` : "—"],
                  ].map(([label, value]) => (
                    <div key={label} style={{ display: "flex", justifyContent: "space-between", padding: "8px 0", borderBottom: "1px solid var(--border-light)" }}>
                      <span style={{ fontFamily: "var(--font-display)", fontSize: "0.75rem", fontWeight: 600, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.04em" }}>{label}</span>
                      <span style={{ fontSize: "0.82rem", color: "var(--text-secondary)", fontFamily: "var(--font-body)" }}>{value}</span>
                    </div>
                  ))}
                </div>

                <div style={{ padding: "4px 12px 12px", fontSize: "0.72rem", color: "var(--text-muted)", textAlign: "center" }}>
                  ℹ️ Update your profile picture in Settings
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </header>
  );
}
