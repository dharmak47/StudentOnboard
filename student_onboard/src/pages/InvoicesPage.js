// src/pages/InvoicesPage.js
// Admin invoice management: list all invoices, edit every field + line items,
// live-preview the result, and download the PDF.
import React, { useState, useEffect, useMemo } from "react";
import { invoicesApi } from "../services/api";
import { useToast } from "../context/ToastContext";
import InvoiceDocument from "../components/invoice/InvoiceDocument";
import InvoicePreviewModal from "../components/invoice/InvoicePreviewModal";

const STATUS_OPTIONS = ["Paid", "Partial", "Pending", "Refunded"];

const money = (v, sym = "₹") =>
  `${sym}${Number(v || 0).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;

const round2 = (n) => Math.round((Number(n) || 0) * 100) / 100;

// Mirror of the server-side total computation so the preview matches what will be saved.
function computeTotals(items, fees) {
  let subtotal = 0, discountTotal = 0, taxTotal = 0;
  const priced = items.map((it) => {
    const lineBase = round2((Number(it.quantity) || 0) * (Number(it.unitPrice) || 0));
    const afterDiscount = Math.max(lineBase - (Number(it.discountAmount) || 0), 0);
    const tax = round2(afterDiscount * ((Number(it.taxPercent) || 0) / 100));
    subtotal += lineBase;
    discountTotal += Number(it.discountAmount) || 0;
    taxTotal += tax;
    return { ...it, lineTotal: round2(afterDiscount + tax) };
  });
  const grandTotal = round2(subtotal - discountTotal + taxTotal + (Number(fees.convenienceFee) || 0) + (Number(fees.platformFee) || 0));
  const amountPaid = round2(fees.amountPaid);
  return {
    items: priced,
    subtotal: round2(subtotal),
    discountTotal: round2(discountTotal),
    taxTotal: round2(taxTotal),
    grandTotal,
    amountPaid,
    balanceDue: round2(grandTotal - amountPaid),
  };
}

const label = { display: "block", fontSize: "0.72rem", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.04em", marginBottom: 4 };
const input = { width: "100%", padding: "8px 10px", border: "1.5px solid var(--border)", borderRadius: 8, fontSize: "0.85rem", color: "var(--text-primary)", background: "var(--surface)", outline: "none", boxSizing: "border-box" };

function Field({ lbl, children }) {
  return (
    <div style={{ marginBottom: 12 }}>
      <label style={label}>{lbl}</label>
      {children}
    </div>
  );
}

function toDateInput(v) {
  if (!v) return "";
  const d = new Date(v);
  if (isNaN(d.getTime())) return "";
  return d.toISOString().slice(0, 10);
}

export default function InvoicesPage() {
  const toast = useToast();
  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(null); // full invoice DTO being edited
  const [form, setForm] = useState(null);
  const [saving, setSaving] = useState(false);
  const [openBusyId, setOpenBusyId] = useState(null);
  const [downloadPreview, setDownloadPreview] = useState(null);

  const loadList = async () => {
    try {
      const res = await invoicesApi.list({ pageSize: 100 });
      setInvoices(res.data || []);
    } catch (err) {
      toast.error(err.message || "Failed to load invoices.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadList();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const openEditor = async (id) => {
    setOpenBusyId(id);
    try {
      const res = await invoicesApi.getById(id);
      const inv = res.data;
      setEditing(inv);
      setForm({
        receiptNumber: inv.receiptNumber || "",
        transactionId: inv.transactionId || "",
        orderId: inv.orderId || "",
        referenceNumber: inv.referenceNumber || "",
        invoiceDate: toDateInput(inv.invoiceDate),
        paymentDate: toDateInput(inv.paymentDate),
        paymentStatus: inv.paymentStatus || "Paid",
        paymentMethod: inv.paymentMethod || "",
        paymentGateway: inv.paymentGateway || "",
        convenienceFee: inv.convenienceFee || 0,
        platformFee: inv.platformFee || 0,
        amountPaid: inv.amountPaid || 0,
        refundAmount: inv.refundAmount || 0,
        notes: inv.notes || "",
        terms: inv.terms || "",
        organization: { ...(inv.organization || {}) },
        customer: { ...(inv.customer || {}) },
        items: (inv.items || []).map((it) => ({
          description: it.description || "",
          quantity: it.quantity ?? 1,
          unitPrice: it.unitPrice ?? 0,
          taxPercent: it.taxPercent ?? 0,
          discountAmount: it.discountAmount ?? 0,
        })),
      });
    } catch (err) {
      toast.error(err.message || "Failed to open invoice.");
    } finally {
      setOpenBusyId(null);
    }
  };

  const set = (key, value) => setForm((f) => ({ ...f, [key]: value }));
  const setOrg = (key, value) => setForm((f) => ({ ...f, organization: { ...f.organization, [key]: value } }));
  const setCust = (key, value) => setForm((f) => ({ ...f, customer: { ...f.customer, [key]: value } }));
  const setItem = (i, key, value) =>
    setForm((f) => ({ ...f, items: f.items.map((it, idx) => (idx === i ? { ...it, [key]: value } : it)) }));
  const addItem = () =>
    setForm((f) => ({ ...f, items: [...f.items, { description: "", quantity: 1, unitPrice: 0, taxPercent: 0, discountAmount: 0 }] }));
  const removeItem = (i) => setForm((f) => ({ ...f, items: f.items.filter((_, idx) => idx !== i) }));

  // Live preview invoice (client-side recompute mirrors the server).
  const previewInvoice = useMemo(() => {
    if (!editing || !form) return null;
    const totals = computeTotals(form.items, form);
    return {
      ...editing,
      receiptNumber: form.receiptNumber,
      transactionId: form.transactionId,
      orderId: form.orderId,
      referenceNumber: form.referenceNumber,
      invoiceDate: form.invoiceDate || editing.invoiceDate,
      paymentDate: form.paymentDate || null,
      paymentStatus: form.paymentStatus,
      paymentMethod: form.paymentMethod,
      paymentGateway: form.paymentGateway,
      convenienceFee: Number(form.convenienceFee) || 0,
      platformFee: Number(form.platformFee) || 0,
      refundAmount: Number(form.refundAmount) || 0,
      notes: form.notes,
      terms: form.terms,
      organization: form.organization,
      customer: form.customer,
      ...totals,
    };
  }, [editing, form]);

  const handleSave = async () => {
    setSaving(true);
    try {
      const payload = {
        receiptNumber: form.receiptNumber || null,
        transactionId: form.transactionId || null,
        orderId: form.orderId || null,
        referenceNumber: form.referenceNumber || null,
        invoiceDate: form.invoiceDate ? new Date(form.invoiceDate).toISOString() : null,
        paymentDate: form.paymentDate ? new Date(form.paymentDate).toISOString() : null,
        paymentStatus: form.paymentStatus,
        paymentMethod: form.paymentMethod || null,
        paymentGateway: form.paymentGateway || null,
        convenienceFee: Number(form.convenienceFee) || 0,
        platformFee: Number(form.platformFee) || 0,
        amountPaid: Number(form.amountPaid) || 0,
        refundAmount: Number(form.refundAmount) || 0,
        notes: form.notes || null,
        terms: form.terms || null,
        organization: form.organization,
        customer: form.customer,
        items: form.items.map((it, idx) => ({
          description: it.description,
          quantity: Number(it.quantity) || 0,
          unitPrice: Number(it.unitPrice) || 0,
          taxPercent: Number(it.taxPercent) || 0,
          discountAmount: Number(it.discountAmount) || 0,
          sortOrder: idx,
        })),
      };
      await invoicesApi.update(editing.id, payload);
      toast.success("Invoice saved.");
      setEditing(null);
      setForm(null);
      setLoading(true);
      loadList();
    } catch (err) {
      toast.error(err.message || "Failed to save invoice.");
    } finally {
      setSaving(false);
    }
  };

  // ── List view ──────────────────────────────────────────────────────────
  if (!editing) {
    return (
      <div className="page">
        <div style={{ marginBottom: 24 }}>
          <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.4rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: 4 }}>
            Invoices
          </h2>
          <p style={{ color: "var(--text-muted)", fontSize: "0.85rem" }}>
            {invoices.length} invoice(s). Edit details, then save or download.
          </p>
        </div>

        {loading ? (
          <div style={{ textAlign: "center", color: "var(--text-muted)", padding: 40 }}>Loading…</div>
        ) : invoices.length === 0 ? (
          <div style={{ textAlign: "center", padding: 60 }}>
            <div style={{ fontSize: 48, marginBottom: 16 }}>🧾</div>
            <p style={{ color: "var(--text-muted)" }}>No invoices yet. They are generated when a payment is marked Paid.</p>
          </div>
        ) : (
          <div className="card" style={{ padding: 20, overflowX: "auto" }}>
            <table style={{ width: "100%", borderCollapse: "collapse", minWidth: 720 }}>
              <thead>
                <tr style={{ borderBottom: "1px solid var(--border)" }}>
                  {["Invoice", "Customer", "Amount", "Status", "Date", ""].map((h) => (
                    <th key={h} style={{ textAlign: h === "Amount" ? "right" : "left", padding: "12px 8px", fontSize: "0.72rem", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {invoices.map((inv, i) => (
                  <tr key={inv.id} style={{ borderBottom: i < invoices.length - 1 ? "1px solid var(--border-light)" : "none" }}>
                    <td style={{ padding: "12px 8px", fontSize: "0.85rem", fontWeight: 700, color: "var(--text-primary)" }}>{inv.invoiceNumber}</td>
                    <td style={{ padding: "12px 8px", fontSize: "0.85rem", color: "var(--text-secondary)" }}>
                      {inv.customerName || "—"}
                      <div style={{ fontSize: "0.72rem", color: "var(--text-muted)" }}>{inv.customerEmail}</div>
                    </td>
                    <td style={{ padding: "12px 8px", fontSize: "0.85rem", fontWeight: 600, textAlign: "right" }}>{money(inv.grandTotal, inv.currencySymbol)}</td>
                    <td style={{ padding: "12px 8px", fontSize: "0.8rem", fontWeight: 600 }}>{inv.paymentStatus}</td>
                    <td style={{ padding: "12px 8px", fontSize: "0.83rem", color: "var(--text-muted)" }}>{inv.invoiceDate ? new Date(inv.invoiceDate).toLocaleDateString() : "—"}</td>
                    <td style={{ padding: "12px 8px", textAlign: "right" }}>
                      <button
                        onClick={() => openEditor(inv.id)}
                        disabled={openBusyId === inv.id}
                        style={{ background: "var(--primary)", color: "#fff", border: "none", borderRadius: 8, padding: "7px 14px", cursor: "pointer", fontSize: "0.78rem", fontWeight: 700 }}
                      >
                        {openBusyId === inv.id ? "…" : "Edit"}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    );
  }

  // ── Editor view ────────────────────────────────────────────────────────
  return (
    <div className="page">
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 18, flexWrap: "wrap", gap: 10 }}>
        <div>
          <button onClick={() => { setEditing(null); setForm(null); }} style={{ background: "none", border: "none", color: "var(--primary)", cursor: "pointer", fontWeight: 600, fontSize: "0.85rem", padding: 0, marginBottom: 4 }}>
            ← Back to invoices
          </button>
          <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.3rem", fontWeight: 800, color: "var(--text-primary)" }}>
            Edit {editing.invoiceNumber}
          </h2>
        </div>
        <div style={{ display: "flex", gap: 10 }}>
          <button onClick={() => setDownloadPreview(previewInvoice)} style={{ background: "var(--surface-3)", color: "var(--primary)", border: "none", borderRadius: 8, padding: "9px 16px", cursor: "pointer", fontWeight: 700, fontSize: "0.85rem" }}>
            Preview / Download
          </button>
          <button onClick={handleSave} disabled={saving} style={{ background: "var(--primary)", color: "#fff", border: "none", borderRadius: 8, padding: "9px 18px", cursor: saving ? "wait" : "pointer", fontWeight: 700, fontSize: "0.85rem", opacity: saving ? 0.7 : 1 }}>
            {saving ? "Saving…" : "Save"}
          </button>
        </div>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "minmax(320px, 1fr) minmax(320px, 1fr)", gap: 20, alignItems: "start" }}>
        {/* Form */}
        <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <div className="card" style={{ padding: 20 }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "0.95rem", fontWeight: 700, marginBottom: 14 }}>Invoice Details</h3>
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
              <Field lbl="Receipt No."><input style={input} value={form.receiptNumber} onChange={(e) => set("receiptNumber", e.target.value)} /></Field>
              <Field lbl="Status">
                <select style={input} value={form.paymentStatus} onChange={(e) => set("paymentStatus", e.target.value)}>
                  {STATUS_OPTIONS.map((s) => <option key={s} value={s}>{s}</option>)}
                </select>
              </Field>
              <Field lbl="Invoice Date"><input type="date" style={input} value={form.invoiceDate} onChange={(e) => set("invoiceDate", e.target.value)} /></Field>
              <Field lbl="Payment Date"><input type="date" style={input} value={form.paymentDate} onChange={(e) => set("paymentDate", e.target.value)} /></Field>
              <Field lbl="Order ID"><input style={input} value={form.orderId} onChange={(e) => set("orderId", e.target.value)} /></Field>
              <Field lbl="Reference No."><input style={input} value={form.referenceNumber} onChange={(e) => set("referenceNumber", e.target.value)} /></Field>
              <Field lbl="Payment Method"><input style={input} value={form.paymentMethod} onChange={(e) => set("paymentMethod", e.target.value)} /></Field>
              <Field lbl="Payment Gateway"><input style={input} value={form.paymentGateway} onChange={(e) => set("paymentGateway", e.target.value)} /></Field>
              <Field lbl="Transaction ID"><input style={input} value={form.transactionId} onChange={(e) => set("transactionId", e.target.value)} /></Field>
            </div>
          </div>

          <div className="card" style={{ padding: 20 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 14 }}>
              <h3 style={{ fontFamily: "var(--font-display)", fontSize: "0.95rem", fontWeight: 700 }}>Line Items</h3>
              <button onClick={addItem} style={{ background: "var(--surface-3)", color: "var(--primary)", border: "none", borderRadius: 8, padding: "6px 12px", cursor: "pointer", fontWeight: 700, fontSize: "0.78rem" }}>+ Add item</button>
            </div>
            {form.items.map((it, i) => (
              <div key={i} style={{ border: "1px solid var(--border)", borderRadius: 10, padding: 12, marginBottom: 10 }}>
                <Field lbl="Description"><input style={input} value={it.description} onChange={(e) => setItem(i, "description", e.target.value)} /></Field>
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr 1fr", gap: 8 }}>
                  <Field lbl="Qty"><input type="number" min="0" style={input} value={it.quantity} onChange={(e) => setItem(i, "quantity", e.target.value)} /></Field>
                  <Field lbl="Unit Price"><input type="number" min="0" style={input} value={it.unitPrice} onChange={(e) => setItem(i, "unitPrice", e.target.value)} /></Field>
                  <Field lbl="Tax %"><input type="number" min="0" style={input} value={it.taxPercent} onChange={(e) => setItem(i, "taxPercent", e.target.value)} /></Field>
                  <Field lbl="Discount"><input type="number" min="0" style={input} value={it.discountAmount} onChange={(e) => setItem(i, "discountAmount", e.target.value)} /></Field>
                </div>
                <button onClick={() => removeItem(i)} style={{ background: "none", border: "none", color: "var(--danger)", cursor: "pointer", fontSize: "0.78rem", fontWeight: 600, padding: 0 }}>Remove</button>
              </div>
            ))}
          </div>

          <div className="card" style={{ padding: 20 }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "0.95rem", fontWeight: 700, marginBottom: 14 }}>Fees &amp; Amounts</h3>
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
              <Field lbl="Convenience Fee"><input type="number" min="0" style={input} value={form.convenienceFee} onChange={(e) => set("convenienceFee", e.target.value)} /></Field>
              <Field lbl="Platform Fee"><input type="number" min="0" style={input} value={form.platformFee} onChange={(e) => set("platformFee", e.target.value)} /></Field>
              <Field lbl="Amount Paid"><input type="number" min="0" style={input} value={form.amountPaid} onChange={(e) => set("amountPaid", e.target.value)} /></Field>
              <Field lbl="Refund Amount"><input type="number" min="0" style={input} value={form.refundAmount} onChange={(e) => set("refundAmount", e.target.value)} /></Field>
            </div>
          </div>

          <div className="card" style={{ padding: 20 }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "0.95rem", fontWeight: 700, marginBottom: 14 }}>Customer</h3>
            <Field lbl="Name"><input style={input} value={form.customer.name || ""} onChange={(e) => setCust("name", e.target.value)} /></Field>
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
              <Field lbl="Email"><input style={input} value={form.customer.email || ""} onChange={(e) => setCust("email", e.target.value)} /></Field>
              <Field lbl="Phone"><input style={input} value={form.customer.phone || ""} onChange={(e) => setCust("phone", e.target.value)} /></Field>
            </div>
            <Field lbl="Billing Address"><textarea style={{ ...input, minHeight: 60 }} value={form.customer.billingAddress || ""} onChange={(e) => setCust("billingAddress", e.target.value)} /></Field>
          </div>

          <div className="card" style={{ padding: 20 }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "0.95rem", fontWeight: 700, marginBottom: 14 }}>Organization (this invoice)</h3>
            <Field lbl="Name"><input style={input} value={form.organization.name || ""} onChange={(e) => setOrg("name", e.target.value)} /></Field>
            <Field lbl="Logo URL"><input style={input} value={form.organization.logoUrl || ""} onChange={(e) => setOrg("logoUrl", e.target.value)} /></Field>
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
              <Field lbl="Address Line 1"><input style={input} value={form.organization.addressLine1 || ""} onChange={(e) => setOrg("addressLine1", e.target.value)} /></Field>
              <Field lbl="Address Line 2"><input style={input} value={form.organization.addressLine2 || ""} onChange={(e) => setOrg("addressLine2", e.target.value)} /></Field>
              <Field lbl="City"><input style={input} value={form.organization.city || ""} onChange={(e) => setOrg("city", e.target.value)} /></Field>
              <Field lbl="State"><input style={input} value={form.organization.state || ""} onChange={(e) => setOrg("state", e.target.value)} /></Field>
              <Field lbl="Postal Code"><input style={input} value={form.organization.postalCode || ""} onChange={(e) => setOrg("postalCode", e.target.value)} /></Field>
              <Field lbl="Country"><input style={input} value={form.organization.country || ""} onChange={(e) => setOrg("country", e.target.value)} /></Field>
              <Field lbl="Phone"><input style={input} value={form.organization.phone || ""} onChange={(e) => setOrg("phone", e.target.value)} /></Field>
              <Field lbl="Email"><input style={input} value={form.organization.email || ""} onChange={(e) => setOrg("email", e.target.value)} /></Field>
              <Field lbl="Website"><input style={input} value={form.organization.website || ""} onChange={(e) => setOrg("website", e.target.value)} /></Field>
              <Field lbl="GSTIN / Tax No."><input style={input} value={form.organization.taxRegNo || ""} onChange={(e) => setOrg("taxRegNo", e.target.value)} /></Field>
            </div>
            <Field lbl="Footer Note"><input style={input} value={form.organization.footerNote || ""} onChange={(e) => setOrg("footerNote", e.target.value)} /></Field>
          </div>

          <div className="card" style={{ padding: 20 }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "0.95rem", fontWeight: 700, marginBottom: 14 }}>Notes &amp; Terms</h3>
            <Field lbl="Notes"><textarea style={{ ...input, minHeight: 70 }} value={form.notes} onChange={(e) => set("notes", e.target.value)} /></Field>
            <Field lbl="Terms &amp; Conditions"><textarea style={{ ...input, minHeight: 90 }} value={form.terms} onChange={(e) => set("terms", e.target.value)} /></Field>
          </div>
        </div>

        {/* Live preview */}
        <div style={{ position: "sticky", top: 0 }}>
          <div style={{ fontSize: "0.72rem", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em", marginBottom: 8 }}>Live Preview</div>
          <div className="card" style={{ padding: 12, overflow: "auto", maxHeight: "calc(100vh - 160px)" }}>
            <div style={{ transform: "scale(0.62)", transformOrigin: "top left", width: 794 }}>
              <InvoiceDocument invoice={previewInvoice} />
            </div>
          </div>
        </div>
      </div>

      {downloadPreview && (
        <InvoicePreviewModal invoice={downloadPreview} onClose={() => setDownloadPreview(null)} />
      )}
    </div>
  );
}
