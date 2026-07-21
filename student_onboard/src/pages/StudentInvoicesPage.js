// src/pages/StudentInvoicesPage.js
// Student "Payment History" — lists invoices (read-only) with preview + PDF download.
import React, { useState, useEffect } from "react";
import { invoicesApi } from "../services/api";
import { useToast } from "../context/ToastContext";
import InvoicePreviewModal from "../components/invoice/InvoicePreviewModal";

const STATUS_STYLE = {
  Paid: { bg: "#DCFCE7", color: "#15803D" },
  Partial: { bg: "#DBEAFE", color: "#1D4ED8" },
  Pending: { bg: "#FEE2E2", color: "#B91C1C" },
  Refunded: { bg: "#FEF3C7", color: "#92400E" },
};

const money = (v, sym = "₹") =>
  `${sym}${Number(v || 0).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;

export default function StudentInvoicesPage() {
  const toast = useToast();
  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [preview, setPreview] = useState(null); // { invoice, autoDownload }
  const [busyId, setBusyId] = useState(null);

  useEffect(() => {
    (async () => {
      try {
        const res = await invoicesApi.listMine();
        setInvoices(res.data || []);
      } catch (err) {
        toast.error(err.message || "Failed to load invoices.");
      } finally {
        setLoading(false);
      }
    })();
  }, [toast]);

  const open = async (id, autoDownload) => {
    setBusyId(id);
    try {
      const res = await invoicesApi.getMine(id);
      setPreview({ invoice: res.data, autoDownload });
    } catch (err) {
      toast.error(err.message || "Failed to load invoice.");
    } finally {
      setBusyId(null);
    }
  };

  return (
    <div className="page">
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ fontFamily: "var(--font-display)", fontSize: "1.4rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: 4 }}>
          Payment History
        </h2>
        <p style={{ color: "var(--text-muted)", fontSize: "0.85rem" }}>
          Download invoices for your payments. This invoice serves as proof of payment.
        </p>
      </div>

      {loading ? (
        <div style={{ textAlign: "center", color: "var(--text-muted)", padding: 40 }}>Loading…</div>
      ) : invoices.length === 0 ? (
        <div style={{ textAlign: "center", padding: 60 }}>
          <div style={{ fontSize: 48, marginBottom: 16 }}>🧾</div>
          <p style={{ color: "var(--text-muted)" }}>No invoices yet. Invoices appear here once a payment is recorded.</p>
        </div>
      ) : (
        <div className="card" style={{ padding: 20, overflowX: "auto" }}>
          <table style={{ width: "100%", borderCollapse: "collapse", minWidth: 640 }}>
            <thead>
              <tr style={{ borderBottom: "1px solid var(--border)" }}>
                {["Invoice", "Course", "Amount", "Status", "Date", ""].map((h) => (
                  <th
                    key={h}
                    style={{
                      textAlign: h === "Amount" ? "right" : "left",
                      padding: "12px 8px",
                      fontSize: "0.72rem",
                      fontWeight: 700,
                      color: "var(--text-muted)",
                      textTransform: "uppercase",
                      letterSpacing: "0.05em",
                    }}
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {invoices.map((inv, i) => {
                const st = STATUS_STYLE[inv.paymentStatus] || STATUS_STYLE.Paid;
                return (
                  <tr key={inv.id} style={{ borderBottom: i < invoices.length - 1 ? "1px solid var(--border-light)" : "none" }}>
                    <td style={{ padding: "12px 8px", fontSize: "0.85rem", fontWeight: 700, color: "var(--text-primary)" }}>
                      {inv.invoiceNumber}
                    </td>
                    <td style={{ padding: "12px 8px", fontSize: "0.85rem", color: "var(--text-secondary)" }}>
                      {inv.courseSummary || "—"}
                    </td>
                    <td style={{ padding: "12px 8px", fontSize: "0.85rem", fontWeight: 600, color: "var(--text-primary)", textAlign: "right" }}>
                      {money(inv.grandTotal, inv.currencySymbol)}
                    </td>
                    <td style={{ padding: "12px 8px" }}>
                      <span style={{ background: st.bg, color: st.color, borderRadius: 20, padding: "4px 10px", fontSize: "0.72rem", fontWeight: 700 }}>
                        {inv.paymentStatus}
                      </span>
                    </td>
                    <td style={{ padding: "12px 8px", fontSize: "0.83rem", color: "var(--text-muted)" }}>
                      {inv.invoiceDate ? new Date(inv.invoiceDate).toLocaleDateString() : "—"}
                    </td>
                    <td style={{ padding: "12px 8px", textAlign: "right", whiteSpace: "nowrap" }}>
                      <button
                        onClick={() => open(inv.id, false)}
                        disabled={busyId === inv.id}
                        style={{
                          background: "var(--surface-3)", color: "var(--primary)", border: "none",
                          borderRadius: 8, padding: "7px 12px", cursor: "pointer", fontWeight: 600,
                          fontSize: "0.78rem", marginRight: 8,
                        }}
                      >
                        View
                      </button>
                      <button
                        onClick={() => open(inv.id, true)}
                        disabled={busyId === inv.id}
                        style={{
                          background: "var(--primary)", color: "#fff", border: "none",
                          borderRadius: 8, padding: "7px 12px", cursor: "pointer", fontWeight: 700,
                          fontSize: "0.78rem",
                        }}
                      >
                        {busyId === inv.id ? "…" : "Download"}
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {preview && (
        <InvoicePreviewModal
          invoice={preview.invoice}
          autoDownload={preview.autoDownload}
          onClose={() => setPreview(null)}
        />
      )}
    </div>
  );
}
