// src/components/layout/Sidebar.js
import React, { useState } from "react";
import { useAuth } from "../../context/AuthContext";
import { Avatar } from "../common";
import { FiBell, FiMail } from "react-icons/fi";

const ADMIN_NAV = [
  { id: "dashboard",      icon: "▦",  label: "Dashboard"      },
  { id: "students",       icon: "◎",  label: "Students"       },
  { id: "registrations",  icon: "▤",  label: "Registrations"  },
  { id: "courses",        icon: "◈",  label: "Courses"        },
  { id: "notifications",  icon: <FiBell size={16} />, label: "Notifications"  },
  { id: "faqs",           icon: "?",  label: "FAQs"           },
  { id: "enquiries",      icon: <FiMail size={16} />, label: "Enquiries"      },
];

const STUDENT_NAV = [
  { id: "dashboard",      icon: "▦",  label: "Dashboard"      },
  { id: "courses",        icon: "◈",  label: "Courses"        },
  { id: "notifications",  icon: <FiBell size={16} />, label: "Notifications"  },
  { id: "profile",        icon: "◎",  label: "My Profile"     },
];

export default function Sidebar({ active, onChange }) {
  const { user, isAdmin, logout } = useAuth();
  const [collapsed, setCollapsed] = useState(false);
  const profilePic = localStorage.getItem("edu_user_pic") || "";
  const NAV_ITEMS = isAdmin ? ADMIN_NAV : STUDENT_NAV;

  return (
    <aside style={{
      width: collapsed ? 72 : 248,
      background: "#1A1A3E",
      display: "flex",
      flexDirection: "column",
      transition: "width 0.3s cubic-bezier(0.4,0,0.2,1)",
      overflow: "hidden",
      flexShrink: 0,
      position: "relative",
      zIndex: 10,
    }}>
      {/* Decorative top gradient */}
      <div style={{ position: "absolute", top: 0, left: 0, right: 0, height: 3, background: "linear-gradient(90deg, #5B5BD6, #8B8FD4, #F5D7F0)" }} />

      {/* Logo */}
      <div style={{ padding: collapsed ? "22px 12px" : "22px 22px", display: "flex", alignItems: "center", gap: 12, borderBottom: "1px solid rgba(255,255,255,0.06)", justifyContent: collapsed ? "center" : "flex-start", flexWrap: "nowrap" }}>
        {!collapsed && (
          <>
            <div style={{ width: 38, height: 38, borderRadius: 10, background: "linear-gradient(135deg, #5B5BD6, #8B8FD4)", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 18, flexShrink: 0, boxShadow: "0 4px 14px rgba(91,91,214,0.5)" }}>🎓</div>
            <div style={{ overflow: "hidden", flex: 1 }}>
              <div style={{ color: "#FFFFFF", fontFamily: "var(--font-display)", fontWeight: 800, fontSize: "0.95rem", whiteSpace: "nowrap" }}>EduAdmin</div>
              <div style={{ color: "rgba(255,255,255,0.35)", fontSize: "0.7rem", fontFamily: "var(--font-body)" }}>Admin Portal</div>
            </div>
          </>
        )}
        <button onClick={() => setCollapsed(!collapsed)}
          style={{ background: "rgba(255,255,255,0.08)", border: "none", borderRadius: 8, width: 36, height: 36, minWidth: 36, display: "flex", alignItems: "center", justifyContent: "center", cursor: "pointer", color: "rgba(255,255,255,0.5)", flexShrink: 0, fontSize: 15, transition: "var(--transition)" }}
          title={collapsed ? "Expand sidebar" : "Collapse sidebar"}>
          {collapsed ? "»" : "«"}
        </button>
      </div>

      {/* Nav */}
      <nav style={{ flex: 1, padding: "14px 10px", overflowY: "auto" }}>
        {!collapsed && (
          <p style={{ color: "rgba(255,255,255,0.25)", fontSize: "0.65rem", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.1em", fontFamily: "var(--font-display)", padding: "0 10px", marginBottom: 10 }}>Main Menu</p>
        )}
        {NAV_ITEMS.map((item) => {
          const isActive = active === item.id;
          return (
            <button key={item.id} onClick={() => onChange(item.id)}
              style={{
                width: "100%", display: "flex", alignItems: "center", gap: 12,
                padding: collapsed ? "12px" : "11px 14px",
                borderRadius: 10, border: "none", cursor: "pointer",
                marginBottom: 4, transition: "var(--transition)",
                background: isActive ? "rgba(91,91,214,0.25)" : "transparent",
                justifyContent: collapsed ? "center" : "flex-start",
                position: "relative",
              }}>
              {isActive && <div style={{ position: "absolute", left: 0, top: "20%", bottom: "20%", width: 3, borderRadius: "0 3px 3px 0", background: "#5B5BD6" }} />}
              <span style={{ fontSize: 17, color: isActive ? "#8B8FD4" : "rgba(255,255,255,0.4)", transition: "var(--transition)" }}>{item.icon}</span>
              {!collapsed && <span style={{ fontFamily: "var(--font-display)", fontSize: "0.875rem", fontWeight: isActive ? 700 : 500, color: isActive ? "#C4C3E8" : "rgba(255,255,255,0.5)", whiteSpace: "nowrap" }}>{item.label}</span>}
            </button>
          );
        })}
      </nav>

      {/* User profile */}
      <div style={{ padding: "12px 10px", borderTop: "1px solid rgba(255,255,255,0.06)" }}>
        {!collapsed && user && (
          <div style={{ display: "flex", alignItems: "center", gap: 10, padding: "10px 12px", borderRadius: 10, background: "rgba(255,255,255,0.05)", marginBottom: 8 }}>
            {profilePic ? (
              <img src={profilePic} alt="Profile" style={{ width: 34, height: 34, borderRadius: "50%", objectFit: "cover", flexShrink: 0 }} />
            ) : (
              <Avatar initials={user.avatar || "US"} size={34} />
            )}
            <div style={{ overflow: "hidden", flex: 1 }}>
              <div style={{ color: "rgba(255,255,255,0.9)", fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.8rem", whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>{user.name}</div>
              <div style={{ color: "rgba(255,255,255,0.35)", fontSize: "0.68rem" }}>{isAdmin ? "Admin" : "Student"}</div>
            </div>
          </div>
        )}
        <button onClick={logout}
          style={{ width: "100%", display: "flex", alignItems: "center", gap: 10, padding: collapsed ? "11px" : "10px 14px", borderRadius: 10, border: "none", cursor: "pointer", background: "rgba(239,68,68,0.1)", justifyContent: collapsed ? "center" : "flex-start", transition: "var(--transition)" }}>
          <span style={{ fontSize: 17 }}>🚪</span>
          {!collapsed && <span style={{ fontFamily: "var(--font-display)", fontSize: "0.8rem", fontWeight: 600, color: "#F87171" }}>Logout</span>}
        </button>
      </div>
    </aside>
  );
}
