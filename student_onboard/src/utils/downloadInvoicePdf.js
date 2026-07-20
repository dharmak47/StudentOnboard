// src/utils/downloadInvoicePdf.js
// Renders a DOM node (the on-screen invoice) to a print-ready A4 PDF using
// html2canvas + jsPDF, slicing tall content across multiple pages so the PDF
// matches the preview pixel-for-pixel.
import html2canvas from "html2canvas";
import { jsPDF } from "jspdf";

const sanitize = (name) =>
  String(name || "invoice").replace(/[^a-zA-Z0-9-_]+/g, "-").replace(/^-+|-+$/g, "");

/**
 * @param {HTMLElement} node       The invoice DOM node to capture.
 * @param {string}      invoiceNo  Invoice number used for the file name.
 */
export async function downloadInvoicePdf(node, invoiceNo) {
  if (!node) throw new Error("Nothing to export.");

  const canvas = await html2canvas(node, {
    scale: 2,
    useCORS: true,
    backgroundColor: "#ffffff",
    logging: false,
    windowWidth: node.scrollWidth,
  });

  const imgData = canvas.toDataURL("image/png");
  const pdf = new jsPDF({ orientation: "portrait", unit: "mm", format: "a4" });

  const pageW = pdf.internal.pageSize.getWidth();   // 210mm
  const pageH = pdf.internal.pageSize.getHeight();  // 297mm
  const imgW = pageW;
  const imgH = (canvas.height * imgW) / canvas.width;

  let heightLeft = imgH;
  let position = 0;

  pdf.addImage(imgData, "PNG", 0, position, imgW, imgH);
  heightLeft -= pageH;

  while (heightLeft > 0) {
    position = heightLeft - imgH;
    pdf.addPage();
    pdf.addImage(imgData, "PNG", 0, position, imgW, imgH);
    heightLeft -= pageH;
  }

  pdf.save(`Invoice-${sanitize(invoiceNo)}.pdf`);
}
