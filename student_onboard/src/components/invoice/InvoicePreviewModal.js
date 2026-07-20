// src/components/invoice/InvoicePreviewModal.js
// Responsive, scrollable modal that previews an invoice and downloads it as a PDF.
import React, { useRef, useState, useEffect, useCallback } from "react";
import InvoiceDocument from "./InvoiceDocument";
import { downloadInvoicePdf } from "../../utils/downloadInvoicePdf";
import { useToast } from "../../context/ToastContext";

export default function InvoicePreviewModal({ invoice, onClose, autoDownload = false }) {
  const docRef = useRef(null);
  const toast = useToast();
  const [downloading, setDownloading] = useState(false);

  const handleDownload = useCallback(async () => {
    setDownloading(true);
    try {
      await downloadInvoicePdf(docRef.current, invoice?.invoiceNumber);
      toast.success("Invoice downloaded.");
    } catch (err) {
      toast.error(err.message || "Failed to generate PDF.");
    } finally {
      setDownloading(false);
    }
  }, [invoice, toast]);

  // Optionally trigger the download automatically once the document has rendered.
  useEffect(() => {
    if (!autoDownload || !invoice) return;
    const id = requestAnimationFrame(() => {
      setTimeout(() => handleDownload(), 150);
    });
    return () => cancelAnimationFrame(id);
  }, [autoDownload, invoice, handleDownload]);

  if (!invoice) return null;

  return (
    <div
      onClick={onClose}
      style={{
        position: "fixed",
        inset: 0,
        background: "rgba(26,26,62,0.55)",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        zIndex: 1000,
        padding: 20,
        overflowY: "auto",
      }}
    >
      {/* Toolbar */}
      <div
        onClick={(e) => e.stopPropagation()}
        style={{
          width: "min(834px, 100%)",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          background: "var(--surface)",
          borderRadius: 12,
          padding: "12px 16px",
          marginBottom: 14,
          boxShadow: "0 8px 30px rgba(0,0,0,0.18)",
        }}
      >
        <div style={{ fontFamily: "var(--font-display)", fontWeight: 700, color: "var(--text-primary)" }}>
          Invoice {invoice.invoiceNumber}
        </div>
        <div style={{ display: "flex", gap: 10 }}>
          <button
            onClick={handleDownload}
            disabled={downloading}
            style={{
              background: "var(--primary)",
              color: "#fff",
              border: "none",
              borderRadius: 8,
              padding: "9px 18px",
              cursor: downloading ? "wait" : "pointer",
              fontFamily: "var(--font-display)",
              fontWeight: 700,
              fontSize: "0.85rem",
              opacity: downloading ? 0.7 : 1,
            }}
          >
            {downloading ? "Generating PDF…" : "Download PDF"}
          </button>
          <button
            onClick={onClose}
            style={{
              background: "var(--surface-3)",
              color: "var(--text-primary)",
              border: "none",
              borderRadius: 8,
              padding: "9px 16px",
              cursor: "pointer",
              fontFamily: "var(--font-display)",
              fontWeight: 600,
              fontSize: "0.85rem",
            }}
          >
            Close
          </button>
        </div>
      </div>

      {/* Document (horizontally scrollable on small screens) */}
      <div
        onClick={(e) => e.stopPropagation()}
        style={{
          maxWidth: "100%",
          overflowX: "auto",
          borderRadius: 12,
          boxShadow: "0 12px 40px rgba(0,0,0,0.25)",
          background: "#fff",
        }}
      >
        <InvoiceDocument ref={docRef} invoice={invoice} />
      </div>
    </div>
  );
}
