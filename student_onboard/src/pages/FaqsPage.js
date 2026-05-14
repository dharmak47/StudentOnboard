// src/pages/FaqsPage.js
import React, { useState, useEffect, useCallback } from "react";
import { faqsApi } from "../services/api";
import { useToast } from "../context/ToastContext";
import { PageLoader, EmptyState, ConfirmModal } from "../components/common";

const emptyForm = { question: "", answer: "", sortOrder: 0, isActive: true };

export default function FaqsPage() {
  const toast = useToast();
  const [faqs, setFaqs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState(null); // null = create, object = edit
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const [expandedId, setExpandedId] = useState(null);

  const fetchFaqs = useCallback(async () => {
    setLoading(true);
    try {
      const res = await faqsApi.getAll();
      setFaqs((res.data || []).sort((a, b) => a.sortOrder - b.sortOrder));
    } catch {
      toast.error("Failed to load FAQs.");
      setFaqs([]);
    } finally {
      setLoading(false);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => { fetchFaqs(); }, [fetchFaqs]);

  const openCreate = () => {
    setEditing(null);
    setForm({ ...emptyForm, sortOrder: faqs.length + 1 });
    setShowModal(true);
  };

  const openEdit = (faq) => {
    setEditing(faq);
    setForm({ question: faq.question, answer: faq.answer, sortOrder: faq.sortOrder, isActive: faq.isActive });
    setShowModal(true);
  };

  const closeModal = () => { setShowModal(false); setEditing(null); setForm(emptyForm); };

  const handleSave = async (e) => {
    e.preventDefault();
    if (!form.question.trim() || !form.answer.trim()) { toast.error("Question and answer are required."); return; }

    setSaving(true);
    try {
      if (editing) {
        const res = await faqsApi.update(editing.id, form);
        setFaqs((prev) => prev.map((f) => f.id === editing.id ? res.data : f).sort((a, b) => a.sortOrder - b.sortOrder));
        toast.success("FAQ updated.");
      } else {
        const res = await faqsApi.create(form);
        setFaqs((prev) => [...prev, res.data].sort((a, b) => a.sortOrder - b.sortOrder));
        toast.success("FAQ created.");
      }
      closeModal();
    } catch (err) {
      toast.error(err.message || "Failed to save FAQ.");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await faqsApi.delete(deleteTarget.id);
      setFaqs((prev) => prev.filter((f) => f.id !== deleteTarget.id));
      toast.success("FAQ deleted.");
      setDeleteTarget(null);
    } catch (err) {
      toast.error(err.message || "Failed to delete FAQ.");
    } finally {
      setDeleting(false);
    }
  };

  const toggleActive = async (faq) => {
    try {
      const res = await faqsApi.update(faq.id, {
        question: faq.question, answer: faq.answer,
        sortOrder: faq.sortOrder, isActive: !faq.isActive,
      });
      setFaqs((prev) => prev.map((f) => f.id === faq.id ? res.data : f));
      toast.success(`FAQ ${!faq.isActive ? "activated" : "deactivated"}.`);
    } catch (err) {
      toast.error(err.message || "Failed to update FAQ.");
    }
  };

  if (loading) return <PageLoader />;

  return (
    <div className="page" style={{ display: "flex", flexDirection: "column", gap: 20 }}>
      {/* Header */}
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", flexWrap: "wrap", gap: 14 }}>
        <div>
          <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.4rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: 4 }}>
            FAQs Management
          </h2>
          <p style={{ color: "var(--text-muted)", fontSize: "0.85rem" }}>
            {faqs.length} {faqs.length === 1 ? "question" : "questions"} &middot; {faqs.filter((f) => f.isActive).length} active
          </p>
        </div>
        <button className="btn-primary" onClick={openCreate} style={{ display: "flex", alignItems: "center", gap: 8 }}>
          <span style={{ fontSize: 18 }}>+</span> Add FAQ
        </button>
      </div>

      {/* FAQ List */}
      {faqs.length === 0 ? (
        <EmptyState
          icon="❓"
          title="No FAQs yet"
          description="Create your first FAQ to help students find answers quickly."
          action={<button className="btn-primary" onClick={openCreate}>Add FAQ</button>}
        />
      ) : (
        <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
          {faqs.map((faq, i) => {
            const isExpanded = expandedId === faq.id;
            return (
              <div key={faq.id} className="card" style={{
                padding: 0, overflow: "hidden", opacity: faq.isActive ? 1 : 0.6,
                animation: `fadeUp 0.3s ease both`, animationDelay: `${i * 0.04}s`,
              }}>
                {/* Question row */}
                <div
                  onClick={() => setExpandedId(isExpanded ? null : faq.id)}
                  style={{
                    padding: "18px 22px", display: "flex", alignItems: "center", gap: 14,
                    cursor: "pointer", transition: "var(--transition)",
                    background: isExpanded ? "var(--surface-2)" : "transparent",
                  }}
                >
                  <div style={{
                    width: 36, height: 36, borderRadius: 10,
                    background: faq.isActive ? "rgba(91,91,214,0.12)" : "var(--surface-3)",
                    display: "flex", alignItems: "center", justifyContent: "center",
                    fontSize: 16, flexShrink: 0, fontFamily: "var(--font-display)", fontWeight: 700,
                    color: faq.isActive ? "var(--primary)" : "var(--text-muted)",
                  }}>
                    {faq.sortOrder}
                  </div>
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{
                      fontFamily: "var(--font-display)", fontWeight: 700, fontSize: "0.92rem",
                      color: "var(--text-primary)",
                    }}>
                      {faq.question}
                    </div>
                    {!isExpanded && (
                      <div style={{
                        fontSize: "0.8rem", color: "var(--text-muted)", marginTop: 3,
                        overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap",
                      }}>
                        {faq.answer}
                      </div>
                    )}
                  </div>
                  {!faq.isActive && (
                    <span style={{
                      padding: "3px 10px", borderRadius: "var(--radius-full)",
                      fontSize: "0.7rem", fontWeight: 700, fontFamily: "var(--font-display)",
                      background: "var(--surface-3)", color: "var(--text-muted)",
                    }}>
                      Inactive
                    </span>
                  )}
                  <span style={{
                    fontSize: 14, color: "var(--text-muted)", transition: "transform 0.2s ease",
                    transform: isExpanded ? "rotate(180deg)" : "rotate(0deg)",
                  }}>
                    ▼
                  </span>
                </div>

                {/* Expanded answer + actions */}
                {isExpanded && (
                  <div style={{ padding: "0 22px 18px 72px", animation: "fadeUp 0.2s ease both" }}>
                    <div style={{
                      fontSize: "0.88rem", color: "var(--text-secondary)", lineHeight: 1.7,
                      whiteSpace: "pre-wrap", wordBreak: "break-word", marginBottom: 16,
                    }}>
                      {faq.answer}
                    </div>
                    <div style={{ display: "flex", gap: 8 }}>
                      <button className="btn-ghost" onClick={(e) => { e.stopPropagation(); openEdit(faq); }}
                        style={{ padding: "6px 14px", fontSize: "0.78rem" }}>
                        Edit
                      </button>
                      <button className="btn-ghost" onClick={(e) => { e.stopPropagation(); toggleActive(faq); }}
                        style={{ padding: "6px 14px", fontSize: "0.78rem" }}>
                        {faq.isActive ? "Deactivate" : "Activate"}
                      </button>
                      <button className="btn-ghost" onClick={(e) => { e.stopPropagation(); setDeleteTarget(faq); }}
                        style={{ padding: "6px 14px", fontSize: "0.78rem", color: "#EF4444" }}>
                        Delete
                      </button>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Create / Edit Modal */}
      {showModal && (
        <div className="overlay" onClick={closeModal}>
          <div className="card-elevated animate-fadeUp" onClick={(e) => e.stopPropagation()}
            style={{ background: "var(--surface)", padding: 32, maxWidth: 520, width: "90%", borderRadius: "var(--radius-xl)" }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.15rem", fontWeight: 800, marginBottom: 6 }}>
              {editing ? "Edit FAQ" : "Add New FAQ"}
            </h3>
            <p style={{ color: "var(--text-muted)", fontSize: "0.83rem", marginBottom: 24 }}>
              {editing ? "Update the question and answer below." : "Fill in the details for the new FAQ."}
            </p>
            <form onSubmit={handleSave} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
              <div>
                <label className="label">Question *</label>
                <input className="input-field" value={form.question}
                  onChange={(e) => setForm({ ...form, question: e.target.value })}
                  placeholder="e.g. How do I enroll in a course?" required />
              </div>
              <div>
                <label className="label">Answer *</label>
                <textarea className="input-field" rows={4} value={form.answer}
                  onChange={(e) => setForm({ ...form, answer: e.target.value })}
                  placeholder="Write a clear answer..." style={{ resize: "vertical" }} required />
              </div>
              <div style={{ display: "flex", gap: 14 }}>
                <div style={{ flex: 1 }}>
                  <label className="label">Sort Order</label>
                  <input className="input-field" type="number" min="0" value={form.sortOrder}
                    onChange={(e) => setForm({ ...form, sortOrder: parseInt(e.target.value) || 0 })} />
                </div>
                {editing && (
                  <div style={{ flex: 1, display: "flex", alignItems: "flex-end" }}>
                    <label style={{ display: "flex", alignItems: "center", gap: 10, cursor: "pointer", padding: "10px 0" }}>
                      <input type="checkbox" checked={form.isActive}
                        onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
                        style={{ accentColor: "var(--primary)", width: 18, height: 18 }} />
                      <span style={{ fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.85rem" }}>Active</span>
                    </label>
                  </div>
                )}
              </div>
              <div style={{ display: "flex", gap: 12, justifyContent: "flex-end", marginTop: 8 }}>
                <button type="button" className="btn-ghost" onClick={closeModal} style={{ padding: "10px 20px" }}>Cancel</button>
                <button type="submit" className="btn-primary" disabled={saving}>
                  {saving ? "Saving..." : editing ? "Update FAQ" : "Create FAQ"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete Confirmation */}
      <ConfirmModal
        isOpen={!!deleteTarget}
        title="Delete FAQ"
        message={`Are you sure you want to delete "${deleteTarget?.question}"? This action cannot be undone.`}
        confirmLabel="Delete"
        confirmStyle="danger"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
        loading={deleting}
      />
    </div>
  );
}
