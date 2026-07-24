import React, { useState } from "react";
import { studentApi, enquiriesApi } from "../services/api";
import { useToast } from "../context/ToastContext";

export default function StudentHelpPage() {
  const toast = useToast();
  const [activeTab, setActiveTab] = useState("faq");
  const [loading, setLoading] = useState(false);
  const [faqs, setFaqs] = useState([]);
  const [formData, setFormData] = useState({ name: "", email: "", subject: "", message: "" });

  React.useEffect(() => {
    const fetch = async () => {
      try {
        const res = await studentApi.getFaqs();
        if (res.data) {
          setFaqs(res.data);
        }
      } catch (err) {
        console.log("FAQs not available");
      }
    };
    fetch();
  }, []);

  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!formData.name || !formData.email || !formData.subject || !formData.message) {
      toast.error("Please fill in all fields.");
      return;
    }

    setLoading(true);
    try {
      await enquiriesApi.submit({
        name: formData.name,
        email: formData.email,
        message: `[${formData.subject}] ${formData.message}`
      });
      setFormData({ name: "", email: "", subject: "", message: "" });
      toast.success("Message sent successfully! We will get back to you soon.");
    } catch (err) {
      toast.error(err.message || "Failed to send message.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page">
      <h1 style={{ fontFamily: "var(--font-display)", fontSize: "1.8rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: 32 }}>
        ❓ Help & Support
      </h1>

      {/* Tabs */}
      <div style={{ display: "flex", gap: 8, marginBottom: 24, borderBottom: "1px solid var(--border)", paddingBottom: 0 }}>
        {[
          { id: "faq", label: "FAQs", icon: "❓" },
          { id: "contact", label: "Contact Us", icon: "💬" },
        ].map((t) => (
          <button
            key={t.id}
            onClick={() => setActiveTab(t.id)}
            style={{
              padding: "12px 16px", border: "none", background: "transparent", cursor: "pointer",
              fontFamily: "var(--font-display)", fontWeight: activeTab === t.id ? 700 : 500,
              color: activeTab === t.id ? "var(--primary)" : "var(--text-muted)",
              borderBottom: activeTab === t.id ? "2px solid var(--primary)" : "none",
              fontSize: "0.95rem", transition: "var(--transition)",
            }}
          >
            {t.icon} {t.label}
          </button>
        ))}
      </div>

      {/* FAQ Tab */}
      {activeTab === "faq" && (
        <div>
          {faqs && faqs.length > 0 ? (
            <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
              {faqs.map((faq, idx) => (
                <details key={idx} style={{
                  border: "1px solid var(--border-light)",
                  borderRadius: 8,
                  padding: 16,
                  cursor: "pointer",
                  transition: "var(--transition)"
                }}>
                  <summary style={{
                    fontFamily: "var(--font-display)",
                    fontWeight: 600,
                    fontSize: "0.95rem",
                    color: "var(--text-primary)",
                    userSelect: "none"
                  }}>
                    {faq.question}
                  </summary>
                  <div style={{ marginTop: 12, color: "var(--text-secondary)", fontSize: "0.9rem", lineHeight: 1.6 }}>
                    {faq.answer}
                  </div>
                </details>
              ))}
            </div>
          ) : (
            <div style={{ textAlign: "center", padding: 40 }}>
              <div style={{ fontSize: 48, marginBottom: 16 }}>📚</div>
              <p style={{ color: "var(--text-muted)" }}>No FAQs available at the moment.</p>
            </div>
          )}
        </div>
      )}

      {/* Contact Tab */}
      {activeTab === "contact" && (
        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 24 }}>
          {/* Contact Form */}
          <div className="card animate-fadeUp" style={{ padding: 24 }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 20 }}>
              Send us a Message
            </h3>
            <form onSubmit={handleSendMessage} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
              <div>
                <label className="label">Name</label>
                <input
                  className="input-field"
                  type="text"
                  placeholder="Your name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                />
              </div>

              <div>
                <label className="label">Email</label>
                <input
                  className="input-field"
                  type="email"
                  placeholder="your@email.com"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                />
              </div>

              <div>
                <label className="label">Subject</label>
                <input
                  className="input-field"
                  type="text"
                  placeholder="Subject"
                  value={formData.subject}
                  onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
                />
              </div>

              <div>
                <label className="label">Message</label>
                <textarea
                  className="input-field"
                  placeholder="Your message..."
                  rows={5}
                  value={formData.message}
                  onChange={(e) => setFormData({ ...formData, message: e.target.value })}
                  style={{ fontFamily: "var(--font-body)", resize: "vertical" }}
                />
              </div>

              <button type="submit" className="btn-primary" disabled={loading}>
                {loading ? "Sending..." : "Send Message"}
              </button>
            </form>
          </div>

          {/* Contact Info */}
          <div>
            <div className="card animate-fadeUp" style={{ padding: 24, marginBottom: 16 }}>
              <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 16 }}>
                Contact Information
              </h3>
              <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
                <div>
                  <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginBottom: 4 }}>Email</div>
                  <div style={{ color: "var(--text-primary)", fontWeight: 600 }}>support@education.com</div>
                </div>
                <div>
                  <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginBottom: 4 }}>Phone</div>
                  <div style={{ color: "var(--text-primary)", fontWeight: 600 }}>+1 (555) 000-0000</div>
                </div>
                <div>
                  <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginBottom: 4 }}>Address</div>
                  <div style={{ color: "var(--text-primary)", fontWeight: 600 }}>123 Education Lane, Learning City, LC 12345</div>
                </div>
              </div>
            </div>

            <div className="card animate-fadeUp" style={{ padding: 24 }}>
              <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1.05rem", fontWeight: 700, color: "var(--text-primary)", marginBottom: 16 }}>
                Response Time
              </h3>
              <p style={{ color: "var(--text-secondary)", lineHeight: 1.6 }}>
                We typically respond to support inquiries within 24 business hours. For urgent matters, please call us directly.
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
