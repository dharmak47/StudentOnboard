// src/pages/CoursesPage.js
import React, { useState } from "react";
import { useCourses } from "../hooks/useCourses";
import { useToast } from "../context/ToastContext";
import { PageLoader, EmptyState, StatusBadge, ConfirmModal } from "../components/common";

const CATEGORIES = ["Frontend", "Backend", "Design", "Data", "Full Stack", "Mobile", "DevOps"];
const CAT_COLORS = {
  Frontend:    { bg: "#EEF2FF", color: "#4338CA" },
  Backend:     { bg: "#F0FDF4", color: "#15803D" },
  Design:      { bg: "#FDF2F8", color: "#9D174D" },
  Data:        { bg: "#F5F3FF", color: "#6D28D9" },
  "Full Stack":{ bg: "#FFF7ED", color: "#C2410C" },
  Mobile:      { bg: "#ECFDF5", color: "#047857" },
  DevOps:      { bg: "#EFF6FF", color: "#1D4ED8" },
};

const EMPTY_FORM = { title: "", instructor: "", duration: "", category: "", price: "", offerPrice: "", syllabus: "", description: "", status: "active", thumbnail: "📘", batchTiming: "" };

export default function CoursesPage() {
  const toast = useToast();
  const { courses, loading, createCourse, updateCourse, deleteCourse } = useCourses();
  const [modalOpen, setModalOpen]   = useState(false);
  const [editTarget, setEditTarget] = useState(null);
  const [form, setForm]             = useState(EMPTY_FORM);
  const [saving, setSaving]         = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting]     = useState(false);
  const [viewMode, setViewMode]     = useState("grid"); // grid | table
  const [filter, setFilter]         = useState("all"); // all | active | non active

  const activeCourses   = courses.filter((c) => c.status === "active");
  const inactiveCourses = courses.filter((c) => c.status === "non active");
  const filteredCourses = filter === "all" ? courses : filter === "active" ? activeCourses : inactiveCourses;

  const openCreate = () => { setForm(EMPTY_FORM); setEditTarget(null); setModalOpen(true); };
  const openEdit   = (c) => { setForm({ title: c.title, instructor: c.instructor, duration: c.duration, category: c.category, price: String(c.price), offerPrice: c.offerPrice ? String(c.offerPrice) : "", syllabus: c.syllabus || "", description: c.description, status: c.status, thumbnail: c.thumbnail, batchTiming: c.batchTiming || "" }); setEditTarget(c); setModalOpen(true); };

  const handleSave = async (e) => {
    e.preventDefault();
    if (!form.title || !form.instructor || !form.category) { toast.error("Title, instructor and category are required."); return; }
    setSaving(true);
    try {
      const payload = { ...form, price: Number(form.price) || 0, offerPrice: form.offerPrice ? Number(form.offerPrice) : null };
      if (editTarget) {
        await updateCourse(editTarget.id, payload);
        toast.success("Course updated successfully!");
      } else {
        await createCourse(payload);
        toast.success("Course created successfully!");
      }
      setModalOpen(false);
    } catch (err) {
      toast.error(err.message || "Save failed.");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteCourse(deleteTarget.id);
      toast.success("Course deleted.");
      setDeleteTarget(null);
    } catch (err) {
      toast.error(err.message || "Delete failed.");
    } finally {
      setDeleting(false);
    }
  };

  if (loading) return <PageLoader />;

  return (
    <div className="page" style={{ display: "flex", flexDirection: "column", gap: 22 }}>

      {/* Toolbar */}
      <div style={{ display: "flex", alignItems: "center", gap: 14, flexWrap: "wrap" }}>
        <div style={{ display: "flex", background: "var(--surface)", border: "1px solid var(--border)", borderRadius: "var(--radius-md)", overflow: "hidden" }}>
          {[["grid", "⊞"], ["table", "☰"]].map(([mode, icon]) => (
            <button key={mode} onClick={() => setViewMode(mode)}
              style={{ padding: "8px 14px", border: "none", cursor: "pointer", background: viewMode === mode ? "var(--primary)" : "transparent", color: viewMode === mode ? "#fff" : "var(--text-muted)", fontSize: 16, transition: "var(--transition)" }}>
              {icon}
            </button>
          ))}
        </div>

        {/* Filter Tabs */}
        <div style={{ display: "flex", background: "var(--surface)", border: "1px solid var(--border)", borderRadius: "var(--radius-md)", overflow: "hidden" }}>
          {[["all", "All", courses.length], ["active", "Active", activeCourses.length], ["non active", "Non Active", inactiveCourses.length]].map(([key, label, count]) => (
            <button key={key} onClick={() => setFilter(key)}
              style={{ padding: "8px 16px", border: "none", cursor: "pointer", background: filter === key ? "var(--primary)" : "transparent", color: filter === key ? "#fff" : "var(--text-muted)", fontSize: "0.8rem", fontFamily: "var(--font-display)", fontWeight: 600, transition: "var(--transition)", display: "flex", alignItems: "center", gap: 6 }}>
              {label} <span style={{ background: filter === key ? "rgba(255,255,255,0.25)" : "var(--surface-3)", padding: "1px 7px", borderRadius: 8, fontSize: "0.72rem", fontWeight: 700 }}>{count}</span>
            </button>
          ))}
        </div>

        <span style={{ fontSize: "0.85rem", color: "var(--text-muted)", fontFamily: "var(--font-body)" }}>{filteredCourses.length} of {courses.length} courses</span>
        <button className="btn-primary" onClick={openCreate} style={{ marginLeft: "auto" }}>+ New Course</button>
      </div>

      {/* Grid View */}
      {viewMode === "grid" && (
        courses.length === 0 ? (
          <EmptyState icon="📚" title="No courses yet" description="Create your first course to get started." action={<button className="btn-primary" onClick={openCreate}>+ Create Course</button>} />
        ) : filter === "all" ? (
          <div style={{ display: "flex", flexDirection: "column", gap: 28 }}>
            {/* Active Section */}
            {activeCourses.length > 0 && (
              <div>
                <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 16 }}>
                  <span style={{ width: 8, height: 8, borderRadius: "50%", background: "#22C55E" }} />
                  <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1rem", fontWeight: 700, color: "var(--text-primary)" }}>Active Courses</h3>
                  <span style={{ fontSize: "0.75rem", fontWeight: 600, background: "#DCFCE7", color: "#15803D", padding: "2px 10px", borderRadius: 8 }}>{activeCourses.length}</span>
                </div>
                <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: 20 }}>
                  {activeCourses.map((c, i) => <CourseCard key={c.id} course={c} index={i} onEdit={() => openEdit(c)} onDelete={() => setDeleteTarget(c)} />)}
                </div>
              </div>
            )}
            {/* Non Active Section */}
            {inactiveCourses.length > 0 && (
              <div>
                <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 16 }}>
                  <span style={{ width: 8, height: 8, borderRadius: "50%", background: "#9CA3AF" }} />
                  <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1rem", fontWeight: 700, color: "var(--text-primary)" }}>Non Active Courses</h3>
                  <span style={{ fontSize: "0.75rem", fontWeight: 600, background: "var(--surface-3)", color: "var(--text-muted)", padding: "2px 10px", borderRadius: 8 }}>{inactiveCourses.length}</span>
                </div>
                <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: 20 }}>
                  {inactiveCourses.map((c, i) => <CourseCard key={c.id} course={c} index={i + activeCourses.length} onEdit={() => openEdit(c)} onDelete={() => setDeleteTarget(c)} />)}
                </div>
              </div>
            )}
          </div>
        ) : filteredCourses.length === 0 ? (
          <EmptyState icon={filter === "active" ? "✅" : "⏸️"} title={`No ${filter} courses`} description={`There are no ${filter} courses at the moment.`} />
        ) : (
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: 20 }}>
            {filteredCourses.map((c, i) => <CourseCard key={c.id} course={c} index={i} onEdit={() => openEdit(c)} onDelete={() => setDeleteTarget(c)} />)}
          </div>
        )
      )}

      {/* Table View */}
      {viewMode === "table" && (
        <div className="card" style={{ overflow: "hidden" }}>
          <div style={{ display: "grid", gridTemplateColumns: "2.5fr 1.5fr 1fr 1fr 1fr 100px", gap: 16, padding: "12px 20px", background: "var(--surface-2)", borderBottom: "1px solid var(--border)" }}>
            {["Course", "Instructor", "Duration", "Students", "Price", ""].map((h) => (
              <div key={h} style={{ fontFamily: "var(--font-display)", fontSize: "0.72rem", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.06em" }}>{h}</div>
            ))}
          </div>
          {filteredCourses.map((c, i) => (
            <div key={c.id} style={{ display: "grid", gridTemplateColumns: "2.5fr 1.5fr 1fr 1fr 1fr 100px", gap: 16, padding: "14px 20px", borderBottom: "1px solid var(--border-light)", animation: `fadeUp 0.35s ease both`, animationDelay: `${i * 0.04}s` }}>
              <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                <div style={{ width: 38, height: 38, borderRadius: 10, background: "var(--surface-3)", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 18 }}>{c.thumbnail}</div>
                <div>
                  <div style={{ fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.875rem" }}>{c.title}</div>
                  <StatusBadge status={c.status} />
                </div>
              </div>
              <div style={{ display: "flex", alignItems: "center", fontSize: "0.82rem", color: "var(--text-secondary)" }}>{c.instructor}</div>
              <div style={{ display: "flex", alignItems: "center", fontSize: "0.82rem", color: "var(--text-secondary)" }}>{c.duration}</div>
              <div style={{ display: "flex", alignItems: "center", fontSize: "0.82rem", color: "var(--text-secondary)" }}>{c.students}</div>
              <div style={{ display: "flex", alignItems: "center", fontFamily: "var(--font-display)", fontWeight: 700, fontSize: "0.875rem" }}>₹{c.price.toLocaleString()}</div>
              <div style={{ display: "flex", alignItems: "center", gap: 6 }}>
                <button className="btn-ghost" onClick={() => openEdit(c)} style={{ padding: "5px 10px", fontSize: "0.75rem" }}>Edit</button>
                <button className="btn-danger" onClick={() => setDeleteTarget(c)} style={{ padding: "5px 10px", fontSize: "0.75rem" }}>Del</button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Course Modal */}
      {modalOpen && (
        <div className="overlay" onClick={() => setModalOpen(false)}>
          <div className="card-elevated animate-fadeUp" onClick={(e) => e.stopPropagation()}
            style={{ background: "var(--surface)", borderRadius: "var(--radius-xl)", padding: 36, width: "100%", maxWidth: 560, maxHeight: "90vh", overflowY: "auto" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 28 }}>
              <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.25rem", fontWeight: 800 }}>{editTarget ? "Edit Course" : "Create New Course"}</h2>
              <button onClick={() => setModalOpen(false)} style={{ background: "var(--surface-3)", border: "none", borderRadius: 8, width: 32, height: 32, cursor: "pointer", fontSize: 18, color: "var(--text-muted)" }}>×</button>
            </div>
            <form onSubmit={handleSave} style={{ display: "flex", flexDirection: "column", gap: 18 }}>
              {/* Thumbnail picker */}
              <div>
                <label className="label">Thumbnail Icon</label>
                <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                  {["📘", "⚛️", "🟢", "🎨", "📊", "🔥", "📱", "☁️"].map((emoji) => (
                    <button key={emoji} type="button" onClick={() => setForm({ ...form, thumbnail: emoji })}
                      style={{ width: 44, height: 44, fontSize: 22, background: form.thumbnail === emoji ? "var(--surface-3)" : "transparent", border: `2px solid ${form.thumbnail === emoji ? "var(--primary)" : "var(--border)"}`, borderRadius: 10, cursor: "pointer", transition: "var(--transition)" }}>
                      {emoji}
                    </button>
                  ))}
                </div>
              </div>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
                <div style={{ gridColumn: "1/-1" }}>
                  <label className="label">Course Title *</label>
                  <input className="input-field" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} placeholder="e.g. React Fundamentals" required />
                </div>
                <div>
                  <label className="label">Instructor *</label>
                  <input className="input-field" value={form.instructor} onChange={(e) => setForm({ ...form, instructor: e.target.value })} placeholder="Dr. Name" required />
                </div>
                <div>
                  <label className="label">Duration</label>
                  <input className="input-field" value={form.duration} onChange={(e) => setForm({ ...form, duration: e.target.value })} placeholder="e.g. 8 weeks" />
                </div>
                <div>
                  <label className="label">Category *</label>
                  <select className="input-field" value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
                    <option value="">Select Category</option>
                    {CATEGORIES.map((c) => <option key={c}>{c}</option>)}
                  </select>
                </div>
                <div>
                  <label className="label">Price (₹)</label>
                  <input className="input-field" type="number" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} placeholder="4999" />
                </div>
                <div>
                  <label className="label">Offer Price (₹)</label>
                  <input className="input-field" type="number" value={form.offerPrice} onChange={(e) => setForm({ ...form, offerPrice: e.target.value })} placeholder="3999 (optional)" />
                </div>
                <div>
                  <label className="label">Status</label>
                  <select className="input-field" value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })}>
                    <option value="active">Active</option>
                    <option value="non active">Non Active</option>
                  </select>
                </div>
                <div>
                  <label className="label">Batch Timing</label>
                  <input className="input-field" value={form.batchTiming} onChange={(e) => setForm({ ...form, batchTiming: e.target.value })} placeholder="e.g. Mon-Fri, 10:00 AM - 12:00 PM" />
                </div>
                <div style={{ gridColumn: "1/-1" }}>
                  <label className="label">Description</label>
                  <textarea className="input-field" rows={3} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Brief course description..." style={{ resize: "vertical" }} />
                </div>
                <div style={{ gridColumn: "1/-1" }}>
                  <label className="label">Syllabus</label>
                  <textarea className="input-field" rows={4} value={form.syllabus} onChange={(e) => setForm({ ...form, syllabus: e.target.value })} placeholder="Enter course syllabus topics (one per line)..." style={{ resize: "vertical" }} />
                </div>
              </div>
              <div style={{ display: "flex", gap: 12, justifyContent: "flex-end", marginTop: 8 }}>
                <button type="button" className="btn-ghost" onClick={() => setModalOpen(false)}>Cancel</button>
                <button type="submit" className="btn-primary" disabled={saving}>
                  {saving ? "Saving..." : editTarget ? "Save Changes" : "Create Course"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete Confirm */}
      <ConfirmModal
        isOpen={!!deleteTarget}
        title="Delete Course?"
        message={`"${deleteTarget?.title}" will be permanently removed. Enrolled students will lose access.`}
        confirmLabel="Delete"
        confirmStyle="danger"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
        loading={deleting}
      />
    </div>
  );
}

function CourseCard({ course: c, index, onEdit, onDelete }) {
  const catStyle = CAT_COLORS[c.category] || { bg: "var(--surface-3)", color: "var(--primary)" };
  return (
    <div className="card animate-fadeUp" style={{ animationDelay: `${index * 0.06}s`, overflow: "hidden", display: "flex", flexDirection: "column" }}>
      <div style={{ padding: "20px 22px 16px", background: "var(--surface-2)", borderBottom: "1px solid var(--border)" }}>
        <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 14 }}>
          {c.category ? <span style={{ padding: "4px 12px", borderRadius: "var(--radius-full)", fontSize: "0.72rem", fontWeight: 700, fontFamily: "var(--font-display)", background: catStyle.bg, color: catStyle.color }}>{c.category}</span> : <span />}
          <StatusBadge status={c.status} />
        </div>
        <div style={{ fontSize: 32, marginBottom: 10 }}>{c.thumbnail}</div>
        <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1rem", fontWeight: 800, marginBottom: 4 }}>{c.title}</h3>
        <p style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>{c.instructor}</p>
      </div>
      <div style={{ padding: "16px 22px", flex: 1 }}>
        <p style={{ fontSize: "0.83rem", color: "var(--text-secondary)", lineHeight: 1.55, marginBottom: 16 }}>{c.description}</p>
        <div style={{ display: "flex", gap: 18, marginBottom: 16 }}>
          {[["👥", `${c.students} students`], ["⏱️", c.duration], ["⭐", `${c.rating || "–"}`], ...(c.batchTiming ? [["🕐", c.batchTiming]] : [])].map(([icon, val]) => (
            <div key={val} style={{ fontSize: "0.78rem", color: "var(--text-muted)", fontFamily: "var(--font-body)" }}>{icon} {val}</div>
          ))}
        </div>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <span style={{ fontFamily: "var(--font-display)", fontWeight: 800, fontSize: "1.05rem", color: "var(--primary)" }}>₹{c.price.toLocaleString()}</span>
          <div style={{ display: "flex", gap: 8 }}>
            <button className="btn-ghost" onClick={onEdit} style={{ padding: "7px 14px", fontSize: "0.8rem" }}>✏️ Edit</button>
            <button className="btn-danger" onClick={onDelete} style={{ padding: "7px 14px", fontSize: "0.8rem" }}>🗑️</button>
          </div>
        </div>
      </div>
    </div>
  );
}
