// src/pages/StudentsPage.js
import React, { useState } from "react";
import { useStudents, useStudentStats } from "../hooks/useStudents";
import { useToast } from "../context/ToastContext";
import { SearchInput, FilterTabs, StatusBadge, Avatar, PageLoader, EmptyState, ConfirmModal } from "../components/common";
import { studentsApi } from "../services/api";

const STATUS_TABS = [
  { label: "All",      value: "all"      },
  { label: "Pending",  value: "pending"  },
  { label: "Approved", value: "approved" },
  { label: "Blocked",  value: "blocked"  },
];

export default function StudentsPage() {
  const toast = useToast();
  const [statusFilter, setStatusFilter] = useState("all");
  const [search, setSearch]             = useState("");
  const [selected, setSelected]         = useState(null);
  const [confirmAction, setConfirmAction] = useState(null); // { student, action }
  const [actionLoading, setActionLoading] = useState(false);
  const [addUserOpen, setAddUserOpen] = useState(false);
  const [changePassTarget, setChangePassTarget] = useState(null);

  const { students, loading, meta, updateStatus } = useStudents(
    { status: statusFilter, search },
  );
  const { stats, refetchStats } = useStudentStats();

  const tabsWithCounts = STATUS_TABS.map((t) => ({
    ...t,
    count: t.value === "all" ? stats?.total : stats?.[t.value],
  }));

  const handleStatusChange = async () => {
    if (!confirmAction) return;
    setActionLoading(true);
    try {
      await updateStatus(confirmAction.student.id, confirmAction.action);
      refetchStats();
      toast.success(`Student ${confirmAction.action === "approved" ? "approved" : "blocked"} successfully.`);
      if (selected?.id === confirmAction.student.id) {
        setSelected((prev) => ({ ...prev, status: confirmAction.action }));
      }
    } catch (err) {
      toast.error(err.message || "Action failed.");
    } finally {
      setActionLoading(false);
      setConfirmAction(null);
    }
  };

  return (
    <div className="page" style={{ display: "flex", gap: 24, height: "100%" }}>
      {/* Main table panel */}
      <div style={{ flex: 1, display: "flex", flexDirection: "column", gap: 20, minWidth: 0 }}>

        {/* Toolbar */}
        <div style={{ display: "flex", alignItems: "center", gap: 14, flexWrap: "wrap" }}>
          <FilterTabs options={tabsWithCounts} value={statusFilter} onChange={setStatusFilter} />
          <div style={{ marginLeft: "auto", display: "flex", gap: 12, alignItems: "center" }}>
            <button className="btn-primary" onClick={() => setAddUserOpen(true)} style={{ justifyContent: "center", padding: "10px 16px", fontSize: "0.9rem" }}>+ Add User</button>
            <SearchInput value={search} onChange={setSearch} placeholder="Search by name or email..." />
          </div>
        </div>

        {/* Loading state */}
        {loading ? <PageLoader /> : (
        /* Table */
        <div className="card" style={{ overflow: "hidden" }}>
          {/* Table header */}
          <div style={{ display: "grid", gridTemplateColumns: "2fr 2fr 1.5fr 1fr 140px", gap: 16, padding: "12px 20px", background: "var(--surface-2)", borderBottom: "1px solid var(--border)" }}>
            {["Student", "Contact", "Course", "Joined", "Actions"].map((h) => (
              <div key={h} style={{ fontFamily: "var(--font-display)", fontSize: "0.72rem", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.06em" }}>{h}</div>
            ))}
          </div>

          {/* Rows */}
          {students.length === 0 ? (
            <EmptyState icon="👨‍🎓" title="No students found" description="Try changing your filters or search term." />
          ) : (
            students.map((s, i) => (
              <StudentRow
                key={s.id}
                student={s}
                index={i}
                isSelected={selected?.id === s.id}
                onClick={() => setSelected(s.id === selected?.id ? null : s)}
                onApprove={() => setConfirmAction({ student: s, action: "approved" })}
                onBlock={() => setConfirmAction({ student: s, action: "blocked" })}
              />
            ))
          )}

          {/* Footer */}
          <div style={{ padding: "14px 20px", borderTop: "1px solid var(--border)", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
            <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontFamily: "var(--font-body)" }}>
              Showing {students.length} of {meta.total} students
            </span>
            <div style={{ display: "flex", gap: 8 }}>
              {Array.from({ length: meta.pages }, (_, i) => i + 1).map((p) => (
                <button key={p} style={{ width: 32, height: 32, borderRadius: 8, border: "1px solid var(--border)", background: p === meta.page ? "var(--primary)" : "var(--surface)", color: p === meta.page ? "#fff" : "var(--text-secondary)", fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.8rem", cursor: "pointer" }}>{p}</button>
              ))}
            </div>
          </div>
        </div>
        )}
      </div>

      {/* Detail Drawer */}
      {selected && (
        <StudentDrawer
          student={selected}
          onClose={() => setSelected(null)}
          onApprove={() => setConfirmAction({ student: selected, action: "approved" })}
          onBlock={() => setConfirmAction({ student: selected, action: "blocked" })}
          onChangePassword={() => setChangePassTarget(selected)}
        />
      )}

      {/* Add User Modal */}
      <AddUserModal
        isOpen={addUserOpen}
        onClose={() => setAddUserOpen(false)}
        onSuccess={() => {
          setAddUserOpen(false);
          refetchStats?.();
        }}
        toast={toast}
      />

      {/* Change Password Modal */}
      <ChangePasswordModal
        isOpen={!!changePassTarget}
        student={changePassTarget}
        onClose={() => setChangePassTarget(null)}
        onSuccess={() => {
          setChangePassTarget(null);
          setSelected(null);
        }}
        toast={toast}
      />

      {/* Confirm Modal */}
      <ConfirmModal
        isOpen={!!confirmAction}
        title={confirmAction?.action === "approved" ? "Approve Student?" : "Block Student?"}
        message={confirmAction?.action === "approved"
          ? `${confirmAction?.student?.name} will be granted full access to their enrolled course.`
          : `${confirmAction?.student?.name} will lose access to the platform. You can unblock them later.`}
        confirmLabel={confirmAction?.action === "approved" ? "Yes, Approve" : "Yes, Block"}
        confirmStyle={confirmAction?.action === "approved" ? "primary" : "danger"}
        onConfirm={handleStatusChange}
        onCancel={() => setConfirmAction(null)}
        loading={actionLoading}
      />
    </div>
  );
}

function StudentRow({ student: s, index, isSelected, onClick, onApprove, onBlock }) {
  return (
    <div onClick={onClick}
      style={{
        display: "grid", gridTemplateColumns: "2fr 2fr 1.5fr 1fr 140px", gap: 16,
        padding: "15px 20px", cursor: "pointer", transition: "var(--transition)",
        background: isSelected ? "var(--surface-3)" : "transparent",
        borderBottom: "1px solid var(--border-light)",
        animation: `fadeUp 0.35s ease both`, animationDelay: `${index * 0.04}s`,
      }}>
      {/* Student */}
      <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
        <Avatar initials={s.avatar} size={36} />
        <div>
          <div style={{ fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.875rem", color: "var(--text-primary)" }}>{s.name}</div>
          <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>ID: {s.id}</div>
        </div>
      </div>
      {/* Contact */}
      <div>
        <div style={{ fontSize: "0.82rem", color: "var(--text-secondary)" }}>{s.email}</div>
        <div style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginTop: 2 }}>{s.phone}</div>
      </div>
      {/* Course */}
      <div style={{ display: "flex", alignItems: "center" }}>
        <span style={{ fontSize: "0.82rem", color: "var(--text-secondary)", fontFamily: "var(--font-display)", fontWeight: 500 }}>{s.course}</span>
      </div>
      {/* Joined */}
      <div style={{ display: "flex", alignItems: "center" }}>
        <span style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>{s.enrolledAt}</span>
      </div>
      {/* Actions */}
      <div style={{ display: "flex", alignItems: "center", gap: 8 }} onClick={(e) => e.stopPropagation()}>
        <StatusBadge status={s.status} />
        <div style={{ display: "flex", gap: 4 }}>
          {s.status !== "approved" && (
            <button className="btn-success" onClick={onApprove} style={{ padding: "5px 10px", fontSize: "0.72rem" }}>✓</button>
          )}
          {s.status !== "blocked" && (
            <button className="btn-danger" onClick={onBlock} style={{ padding: "5px 10px", fontSize: "0.72rem" }}>✕</button>
          )}
        </div>
      </div>
    </div>
  );
}

function StudentDrawer({ student: s, onClose, onApprove, onBlock, onChangePassword }) {
  return (
    <div className="animate-slideInRight" style={{ width: 310, background: "var(--surface)", border: "1px solid var(--border)", borderRadius: "var(--radius-xl)", padding: 0, flexShrink: 0, alignSelf: "flex-start", overflow: "hidden", boxShadow: "var(--shadow-lg)" }}>
      {/* Header */}
      <div style={{ background: "linear-gradient(135deg, #5B5BD6, #8B8FD4)", padding: "28px 24px 20px", position: "relative" }}>
        <button onClick={onClose} style={{ position: "absolute", top: 14, right: 14, background: "rgba(255,255,255,0.2)", border: "none", borderRadius: 8, width: 28, height: 28, color: "#fff", cursor: "pointer", fontSize: 16, display: "flex", alignItems: "center", justifyContent: "center" }}>×</button>
        <Avatar initials={s.avatar} size={60} style={{ border: "3px solid rgba(255,255,255,0.4)" }} />
        <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 800, color: "#fff", marginTop: 12, marginBottom: 4 }}>{s.name}</h3>
        <StatusBadge status={s.status} />
      </div>

      {/* Details */}
      <div style={{ padding: 22, display: "flex", flexDirection: "column", gap: 16 }}>
        {[
          ["📧 Email",       s.email],
          ["📱 Phone",       s.phone],
          ["🏙️ City",        s.city],
          ["🎓 Degree",      s.degree],
          ["📚 Course",      s.course],
          ["📅 Enrolled",    s.enrolledAt],
        ].map(([label, value]) => (
          <div key={label} style={{ display: "flex", flexDirection: "column", gap: 3 }}>
            <span style={{ fontFamily: "var(--font-display)", fontSize: "0.7rem", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.06em" }}>{label}</span>
            <span style={{ fontFamily: "var(--font-body)", fontSize: "0.875rem", color: "var(--text-secondary)" }}>{value}</span>
          </div>
        ))}
      </div>

      {/* Action buttons */}
      <div style={{ padding: "0 22px", display: "flex", flexDirection: "column", gap: 8 }}>
        <div style={{ display: "flex", gap: 10 }}>
          {s.status !== "approved" && (
            <button className="btn-success" onClick={onApprove} style={{ flex: 1, justifyContent: "center", padding: "11px" }}>✓ Approve</button>
          )}
          {s.status !== "blocked" && (
            <button className="btn-danger"  onClick={onBlock}   style={{ flex: 1, justifyContent: "center", padding: "11px" }}>✕ Block</button>
          )}
        </div>
        <button className="btn-ghost" onClick={onChangePassword} style={{ justifyContent: "center", padding: "11px", fontSize: "0.85rem" }}>🔐 Change Password</button>
      </div>
      <div style={{ height: 22 }} />
    </div>
  );
}

function AddUserModal({ isOpen, onClose, onSuccess, toast }) {
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    phoneNumber: "",
    password: "",
    role: "Student",
  });
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.firstName.trim() || !form.lastName.trim() || !form.email.trim() || !form.password.trim()) {
      toast.error("Please fill in all required fields.");
      return;
    }
    if (form.password.length < 6) {
      toast.error("Password must be at least 6 characters.");
      return;
    }
    setSaving(true);
    try {
      await studentsApi.createUser(form);
      toast.success("User created successfully!");
      setForm({ firstName: "", lastName: "", email: "", phoneNumber: "", password: "", role: "Student" });
      onSuccess();
    } catch (err) {
      toast.error(err.message || "Failed to create user.");
    } finally {
      setSaving(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="overlay" onClick={onClose}>
      <div className="card-elevated animate-fadeUp"
        onClick={(e) => e.stopPropagation()}
        style={{ background: "var(--surface)", borderRadius: "var(--radius-xl)", padding: 36, width: "100%", maxWidth: 500, maxHeight: "90vh", overflowY: "auto" }}>
        <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.4rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: 4 }}>Add New User</h2>
        <p style={{ color: "var(--text-muted)", marginBottom: 24, fontSize: "0.9rem" }}>Create a new student or admin account</p>
        <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
            <div>
              <label className="label">First Name</label>
              <input className="input-field" type="text" placeholder="John" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
            </div>
            <div>
              <label className="label">Last Name</label>
              <input className="input-field" type="text" placeholder="Doe" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
            </div>
          </div>
          <div>
            <label className="label">Email Address</label>
            <input className="input-field" type="email" placeholder="john@example.com" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
          </div>
          <div>
            <label className="label">Phone Number (Optional)</label>
            <input className="input-field" type="tel" placeholder="+1 (555) 000-0000" value={form.phoneNumber} onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} />
          </div>
          <div>
            <label className="label">Password</label>
            <input className="input-field" type="password" placeholder="••••••••" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} required />
          </div>
          <div>
            <label className="label">Role</label>
            <select className="input-field" value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })} style={{ cursor: "pointer" }}>
              <option value="Student">Student</option>
              <option value="Admin">Admin</option>
            </select>
          </div>
          <div style={{ display: "flex", gap: 12, justifyContent: "flex-end", marginTop: 12 }}>
            <button className="btn-ghost" onClick={onClose} type="button">Cancel</button>
            <button className="btn-primary" disabled={saving} type="submit">{saving ? "Creating..." : "Create User"}</button>
          </div>
        </form>
      </div>
    </div>
  );
}

function ChangePasswordModal({ isOpen, student, onClose, onSuccess, toast }) {
  const [newPassword, setNewPassword] = useState("");
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!newPassword.trim() || newPassword.length < 6) {
      toast.error("Password must be at least 6 characters.");
      return;
    }
    setSaving(true);
    try {
      await studentsApi.changePassword(student.id, newPassword);
      toast.success(`Password changed for ${student.name}. They will need to log in again.`);
      setNewPassword("");
      onSuccess();
    } catch (err) {
      toast.error(err.message || "Failed to change password.");
    } finally {
      setSaving(false);
    }
  };

  if (!isOpen || !student) return null;

  return (
    <div className="overlay" onClick={onClose}>
      <div className="card-elevated animate-fadeUp"
        onClick={(e) => e.stopPropagation()}
        style={{ background: "var(--surface)", borderRadius: "var(--radius-xl)", padding: 36, width: "100%", maxWidth: 420, maxHeight: "90vh", overflowY: "auto" }}>
        <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.4rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: 8 }}>Change Password</h2>
        <p style={{ color: "var(--text-muted)", marginBottom: 24, fontSize: "0.9rem" }}>Set a new password for <strong>{student.name}</strong></p>
        <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <div>
            <label className="label">New Password</label>
            <input className="input-field" type="password" placeholder="••••••••" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} required />
          </div>
          <div style={{ display: "flex", gap: 12, justifyContent: "flex-end", marginTop: 12 }}>
            <button className="btn-ghost" onClick={onClose} type="button">Cancel</button>
            <button className="btn-primary" disabled={saving} type="submit">{saving ? "Updating..." : "Change Password"}</button>
          </div>
        </form>
      </div>
    </div>
  );
}
