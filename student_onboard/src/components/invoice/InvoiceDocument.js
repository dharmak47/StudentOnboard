// src/components/invoice/InvoiceDocument.js
// Presentational, print-ready A4 invoice. Renders purely from an `invoice` prop
// (the backend InvoiceDto). Uses a fixed corporate palette (independent of the
// app theme) so the html2canvas → jsPDF output is deterministic.
import React from "react";

// ── Fixed invoice palette ──────────────────────────────────────────────────
const ACCENT = "#5B5BD6";
const INK = "#1A1A3E";
const MUTED = "#6B7280";
const LINE = "#E5E7EB";
const HEADER_BG = "#F3F4F6";
const A4_WIDTH = 794; // px ≈ 210mm @96dpi

const STATUS_COLORS = {
  Paid: { bg: "#DCFCE7", color: "#15803D" },
  Partial: { bg: "#DBEAFE", color: "#1D4ED8" },
  Pending: { bg: "#FEE2E2", color: "#B91C1C" },
  Refunded: { bg: "#FEF3C7", color: "#92400E" },
};

// ── Formatting helpers ──────────────────────────────────────────────────────
const fmtMoney = (value, symbol = "₹") => {
  const n = Number(value || 0);
  return `${symbol}${n.toLocaleString("en-IN", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;
};

const fmtDate = (value) => {
  if (!value) return "—";
  const d = new Date(value);
  if (isNaN(d.getTime())) return "—";
  return d.toLocaleDateString(undefined, { day: "2-digit", month: "short", year: "numeric" });
};

const has = (v) => v !== null && v !== undefined && String(v).trim() !== "";

// A labelled detail row, rendered only when the value is present.
const DetailRow = ({ label, value }) =>
  has(value) ? (
    <div style={{ display: "flex", justifyContent: "space-between", gap: 12, marginBottom: 6 }}>
      <span style={{ color: MUTED, fontSize: 12 }}>{label}</span>
      <span style={{ color: INK, fontSize: 12, fontWeight: 600, textAlign: "right" }}>{value}</span>
    </div>
  ) : null;

const SummaryRow = ({ label, value, strong, accent }) => (
  <div
    style={{
      display: "flex",
      justifyContent: "space-between",
      padding: strong ? "10px 0" : "5px 0",
      borderTop: strong ? `2px solid ${ACCENT}` : "none",
    }}
  >
    <span style={{ color: strong ? INK : MUTED, fontSize: strong ? 14 : 12.5, fontWeight: strong ? 700 : 500 }}>
      {label}
    </span>
    <span
      style={{
        color: accent ? ACCENT : INK,
        fontSize: strong ? 15 : 12.5,
        fontWeight: strong ? 800 : 600,
      }}
    >
      {value}
    </span>
  </div>
);

const SectionTitle = ({ children }) => (
  <div
    style={{
      fontSize: 11,
      fontWeight: 700,
      letterSpacing: "0.08em",
      textTransform: "uppercase",
      color: ACCENT,
      marginBottom: 10,
    }}
  >
    {children}
  </div>
);

const th = {
  textAlign: "left",
  padding: "10px 12px",
  fontSize: 11,
  fontWeight: 700,
  color: INK,
  textTransform: "uppercase",
  letterSpacing: "0.04em",
};
const td = { padding: "10px 12px", fontSize: 12.5, color: INK, borderBottom: `1px solid ${LINE}`, verticalAlign: "top" };

const InvoiceDocument = React.forwardRef(({ invoice }, ref) => {
  if (!invoice) return null;

  const org = invoice.organization || {};
  const cust = invoice.customer || {};
  const items = invoice.items || [];
  const sym = invoice.currencySymbol || "₹";
  const status = invoice.paymentStatus || "Paid";
  const statusStyle = STATUS_COLORS[status] || STATUS_COLORS.Paid;

  const orgCityLine = [org.city, org.state, org.postalCode].filter(has).join(", ");
  const year = new Date(invoice.invoiceDate || Date.now()).getFullYear();

  return (
    <div
      ref={ref}
      style={{
        width: A4_WIDTH,
        boxSizing: "border-box",
        background: "#ffffff",
        color: INK,
        fontFamily: "'Plus Jakarta Sans', Arial, Helvetica, sans-serif",
        padding: 48,
        margin: "0 auto",
      }}
    >
      {/* ── Header ── */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: 24 }}>
        <div style={{ display: "flex", gap: 16, alignItems: "flex-start" }}>
          {has(org.logoUrl) ? (
            <img
              src={org.logoUrl}
              alt="Logo"
              crossOrigin="anonymous"
              style={{ width: 56, height: 56, objectFit: "contain", borderRadius: 10 }}
            />
          ) : (
            <div
              style={{
                width: 56,
                height: 56,
                borderRadius: 12,
                background: ACCENT,
                color: "#fff",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                fontSize: 26,
                fontWeight: 800,
              }}
            >
              {(org.name || "S").charAt(0).toUpperCase()}
            </div>
          )}
          <div>
            <div style={{ fontSize: 18, fontWeight: 800, color: INK }}>{org.name || "Organization"}</div>
            <div style={{ fontSize: 12, color: MUTED, lineHeight: 1.6, marginTop: 4 }}>
              {has(org.addressLine1) && <div>{org.addressLine1}</div>}
              {has(org.addressLine2) && <div>{org.addressLine2}</div>}
              {has(orgCityLine) && <div>{orgCityLine}</div>}
              {has(org.country) && <div>{org.country}</div>}
              {has(org.phone) && <div>Phone: {org.phone}</div>}
              {has(org.email) && <div>Email: {org.email}</div>}
              {has(org.website) && <div>{org.website}</div>}
              {has(org.taxRegNo) && <div>GSTIN: {org.taxRegNo}</div>}
            </div>
          </div>
        </div>

        <div style={{ textAlign: "right" }}>
          <div style={{ fontSize: 30, fontWeight: 800, letterSpacing: "0.02em", color: ACCENT }}>INVOICE</div>
          <div style={{ fontSize: 13, fontWeight: 700, color: INK, marginTop: 4 }}>{invoice.invoiceNumber}</div>
          <div
            style={{
              display: "inline-block",
              marginTop: 10,
              padding: "5px 14px",
              borderRadius: 20,
              fontSize: 12,
              fontWeight: 700,
              background: statusStyle.bg,
              color: statusStyle.color,
            }}
          >
            {status}
          </div>
        </div>
      </div>

      <div style={{ height: 3, background: ACCENT, borderRadius: 2, margin: "24px 0" }} />

      {/* ── Bill To + Invoice details ── */}
      <div style={{ display: "flex", justifyContent: "space-between", gap: 32, marginBottom: 28 }}>
        <div style={{ flex: 1 }}>
          <SectionTitle>Bill To</SectionTitle>
          <div style={{ fontSize: 14, fontWeight: 700, color: INK }}>{cust.name || "—"}</div>
          <div style={{ fontSize: 12, color: MUTED, lineHeight: 1.7, marginTop: 4 }}>
            {has(cust.email) && <div>{cust.email}</div>}
            {has(cust.phone) && <div>{cust.phone}</div>}
            {has(cust.billingAddress) && <div>{cust.billingAddress}</div>}
            {has(cust.customerId) && <div>Customer ID: {cust.customerId}</div>}
          </div>
        </div>

        <div style={{ width: 300 }}>
          <SectionTitle>Invoice Details</SectionTitle>
          <DetailRow label="Invoice No." value={invoice.invoiceNumber} />
          <DetailRow label="Receipt No." value={invoice.receiptNumber} />
          <DetailRow label="Invoice Date" value={fmtDate(invoice.invoiceDate)} />
          <DetailRow label="Payment Date" value={invoice.paymentDate ? fmtDate(invoice.paymentDate) : null} />
          <DetailRow label="Order ID" value={invoice.orderId} />
          <DetailRow label="Payment Method" value={invoice.paymentMethod} />
          <DetailRow label="Currency" value={invoice.currencyCode} />
        </div>
      </div>

      {/* ── Line items ── */}
      <table style={{ width: "100%", borderCollapse: "collapse", marginBottom: 20 }}>
        <thead>
          <tr style={{ background: HEADER_BG }}>
            <th style={th}>Description</th>
            <th style={{ ...th, textAlign: "center", width: 60 }}>Qty</th>
            <th style={{ ...th, textAlign: "right", width: 110 }}>Unit Price</th>
            <th style={{ ...th, textAlign: "right", width: 70 }}>Tax</th>
            <th style={{ ...th, textAlign: "right", width: 90 }}>Discount</th>
            <th style={{ ...th, textAlign: "right", width: 110 }}>Total</th>
          </tr>
        </thead>
        <tbody>
          {items.length === 0 ? (
            <tr>
              <td style={{ ...td, textAlign: "center", color: MUTED }} colSpan={6}>
                No items
              </td>
            </tr>
          ) : (
            items.map((it, i) => (
              <tr key={it.id || i}>
                <td style={td}>{it.description}</td>
                <td style={{ ...td, textAlign: "center" }}>{Number(it.quantity)}</td>
                <td style={{ ...td, textAlign: "right" }}>{fmtMoney(it.unitPrice, sym)}</td>
                <td style={{ ...td, textAlign: "right" }}>{Number(it.taxPercent || 0)}%</td>
                <td style={{ ...td, textAlign: "right" }}>{fmtMoney(it.discountAmount, sym)}</td>
                <td style={{ ...td, textAlign: "right", fontWeight: 700 }}>{fmtMoney(it.lineTotal, sym)}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>

      {/* ── Totals ── */}
      <div style={{ display: "flex", justifyContent: "flex-end", marginBottom: 28 }}>
        <div style={{ width: 320 }}>
          <SummaryRow label="Subtotal" value={fmtMoney(invoice.subtotal, sym)} />
          {Number(invoice.discountTotal) > 0 && (
            <SummaryRow label="Discount" value={`- ${fmtMoney(invoice.discountTotal, sym)}`} />
          )}
          {Number(invoice.taxTotal) > 0 && <SummaryRow label="Tax" value={fmtMoney(invoice.taxTotal, sym)} />}
          {Number(invoice.convenienceFee) > 0 && (
            <SummaryRow label="Convenience Fee" value={fmtMoney(invoice.convenienceFee, sym)} />
          )}
          {Number(invoice.platformFee) > 0 && (
            <SummaryRow label="Platform Fee" value={fmtMoney(invoice.platformFee, sym)} />
          )}
          <SummaryRow label="Grand Total" value={fmtMoney(invoice.grandTotal, sym)} strong accent />
          <SummaryRow label="Amount Paid" value={fmtMoney(invoice.amountPaid, sym)} />
          {Number(invoice.balanceDue) > 0 && (
            <SummaryRow label="Balance Due" value={fmtMoney(invoice.balanceDue, sym)} />
          )}
          {Number(invoice.refundAmount) > 0 && (
            <SummaryRow label="Refund" value={fmtMoney(invoice.refundAmount, sym)} />
          )}
        </div>
      </div>

      {/* ── Payment information ── */}
      <div
        style={{
          background: HEADER_BG,
          borderRadius: 10,
          padding: 18,
          marginBottom: 24,
          display: "flex",
          flexWrap: "wrap",
          gap: "6px 32px",
        }}
      >
        <div style={{ width: "100%", marginBottom: 4 }}>
          <SectionTitle>Payment Information</SectionTitle>
        </div>
        {[
          ["Payment Gateway", invoice.paymentGateway],
          ["Transaction ID", invoice.transactionId],
          ["Reference No.", invoice.referenceNumber],
          ["Payment Method", invoice.paymentMethod],
          ["Payment Status", status],
          ["Paid On", invoice.paymentDate ? fmtDate(invoice.paymentDate) : null],
        ]
          .filter(([, v]) => has(v))
          .map(([label, value]) => (
            <div key={label} style={{ minWidth: 200 }}>
              <span style={{ color: MUTED, fontSize: 11.5 }}>{label}: </span>
              <span style={{ color: INK, fontSize: 12, fontWeight: 600 }}>{value}</span>
            </div>
          ))}
      </div>

      {/* ── Notes ── */}
      {has(invoice.notes) && (
        <div style={{ marginBottom: 18 }}>
          <SectionTitle>Notes</SectionTitle>
          <div style={{ fontSize: 12, color: MUTED, lineHeight: 1.7, whiteSpace: "pre-line" }}>{invoice.notes}</div>
        </div>
      )}

      {/* ── Terms ── */}
      {has(invoice.terms) && (
        <div style={{ marginBottom: 24 }}>
          <SectionTitle>Terms &amp; Conditions</SectionTitle>
          <div style={{ fontSize: 11.5, color: MUTED, lineHeight: 1.7, whiteSpace: "pre-line" }}>{invoice.terms}</div>
        </div>
      )}

      {/* ── Footer ── */}
      <div style={{ borderTop: `1px solid ${LINE}`, paddingTop: 16, textAlign: "center" }}>
        <div style={{ fontSize: 12.5, fontWeight: 700, color: INK }}>{org.name || "Organization"}</div>
        <div style={{ fontSize: 11, color: MUTED, marginTop: 4, lineHeight: 1.6 }}>
          {[has(org.email) ? org.email : null, has(org.phone) ? org.phone : null, has(org.website) ? org.website : null]
            .filter(Boolean)
            .join("  •  ")}
          {has(org.addressLine1) && <div>{[org.addressLine1, orgCityLine, org.country].filter(has).join(", ")}</div>}
        </div>
        {has(org.footerNote) && <div style={{ fontSize: 11, color: MUTED, marginTop: 6 }}>{org.footerNote}</div>}
        <div style={{ fontSize: 10.5, color: MUTED, marginTop: 8 }}>
          © {year} {org.name || "Organization"}. This is a computer-generated invoice.
        </div>
      </div>
    </div>
  );
});

InvoiceDocument.displayName = "InvoiceDocument";
export default InvoiceDocument;
