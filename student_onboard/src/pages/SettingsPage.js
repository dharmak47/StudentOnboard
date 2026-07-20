// src/pages/SettingsPage.js
import React, { useState, useEffect } from "react";
import { useAuth } from "../context/AuthContext";
import { useToast } from "../context/ToastContext";
import { authApi, adminProfileApi, invoicesApi } from "../services/api";

function Section({ title, description, children }) {
  return (
    <div className="card animate-fadeUp" style={{ padding: 28, marginBottom: 20 }}>
      <div style={{ marginBottom: 22, paddingBottom: 18, borderBottom: "1px solid var(--border)" }}>
        <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1rem", fontWeight: 700, marginBottom: 4 }}>{title}</h3>
        {description && <p style={{ color: "var(--text-muted)", fontSize: "0.83rem" }}>{description}</p>}
      </div>
      {children}
    </div>
  );
}

export default function SettingsPage() {
  const { user } = useAuth();
  const toast     = useToast();
  const [pwForm, setPwForm]   = useState({ current: "", newPw: "", confirm: "" });
  const [pwLoading, setPwLoading] = useState(false);

  // ── Organization / invoice branding ──────────────────────────────────────
  const [org, setOrg] = useState(null);
  const [orgLoading, setOrgLoading] = useState(true);
  const [orgSaving, setOrgSaving] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const res = await invoicesApi.getOrgSettings();
        setOrg(res.data || {});
      } catch (err) {
        toast.error(err.message || "Failed to load organization settings.");
        setOrg({});
      } finally {
        setOrgLoading(false);
      }
    })();
  }, [toast]);

  const setOrgField = (key, value) => setOrg((o) => ({ ...o, [key]: value }));

  const handleOrgSave = async (e) => {
    e.preventDefault();
    setOrgSaving(true);
    try {
      const payload = {
        orgName: org.orgName || "",
        addressLine1: org.addressLine1 || null,
        addressLine2: org.addressLine2 || null,
        city: org.city || null,
        state: org.state || null,
        postalCode: org.postalCode || null,
        country: org.country || null,
        phone: org.phone || null,
        email: org.email || null,
        website: org.website || null,
        taxRegNo: org.taxRegNo || null,
        logoUrl: org.logoUrl || null,
        currencyCode: org.currencyCode || "INR",
        currencySymbol: org.currencySymbol || "₹",
        defaultTaxPercent: Number(org.defaultTaxPercent) || 0,
        invoicePrefix: org.invoicePrefix || "INV",
        defaultNotes: org.defaultNotes || null,
        defaultTerms: org.defaultTerms || null,
        footerNote: org.footerNote || null,
      };
      const res = await invoicesApi.updateOrgSettings(payload);
      setOrg(res.data || org);
      toast.success("Organization settings saved.");
    } catch (err) {
      toast.error(err.message || "Failed to save organization settings.");
    } finally {
      setOrgSaving(false);
    }
  };

  // Change password — calls POST /api/Auth/change-password
  const handlePasswordSave = async (e) => {
    e.preventDefault();
    if (!pwForm.current)          { toast.error("Enter your current password.");        return; }
    if (pwForm.newPw.length < 6)  { toast.error("New password must be at least 6 characters."); return; }
    if (pwForm.newPw !== pwForm.confirm) { toast.error("Passwords do not match.");      return; }

    setPwLoading(true);
    try {
      await authApi.changePassword(pwForm.current, pwForm.newPw, pwForm.confirm);
      toast.success("Password changed successfully!");
      setPwForm({ current: "", newPw: "", confirm: "" });
    } catch (err) {
      toast.error(err.message || "Failed to change password.");
    } finally {
      setPwLoading(false);
    }
  };

  // Profile picture
  const [profilePic, setProfilePic] = useState(() => localStorage.getItem("edu_user_pic") || user?.profilePic || "");
  const fileInputRef = React.useRef(null);

  const [picLoading, setPicLoading] = useState(false);

  const handleProfilePicChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) { toast.error("Image must be under 2MB."); return; }

    setPicLoading(true);
    try {
      const res = await adminProfileApi.uploadPhoto(file);
      const photoUrl = res.data; // backend returns the uploaded URL in data
      localStorage.setItem("edu_user_pic", photoUrl);
      setProfilePic(photoUrl);
      try {
        const stored = JSON.parse(localStorage.getItem("edu_user"));
        if (stored) {
          stored.profilePic = photoUrl;
          localStorage.setItem("edu_user", JSON.stringify(stored));
        }
      } catch {}
      toast.success("Profile picture updated!");
    } catch (err) {
      toast.error(err.message || "Failed to upload profile picture.");
    } finally {
      setPicLoading(false);
    }
  };

  const removeProfilePic = () => {
    localStorage.removeItem("edu_user_pic");
    setProfilePic("");
    try {
      const stored = JSON.parse(localStorage.getItem("edu_user"));
      if (stored) {
        delete stored.profilePic;
        localStorage.setItem("edu_user", JSON.stringify(stored));
      }
    } catch {}
    toast.success("Profile picture removed.");
  };

  const orgTextFields = [
    ["Organization Name", "orgName"],
    ["Logo URL", "logoUrl"],
    ["Address Line 1", "addressLine1"],
    ["Address Line 2", "addressLine2"],
    ["City", "city"],
    ["State", "state"],
    ["Postal Code", "postalCode"],
    ["Country", "country"],
    ["Phone", "phone"],
    ["Email", "email"],
    ["Website", "website"],
    ["GSTIN / Tax Reg. No.", "taxRegNo"],
    ["Currency Code", "currencyCode"],
    ["Currency Symbol", "currencySymbol"],
    ["Default Tax %", "defaultTaxPercent"],
    ["Invoice Prefix", "invoicePrefix"],
    ["Footer Note", "footerNote"],
  ];

  return (
    <div className="page" style={{ maxWidth: 820 }}>

      {/* Organization / Invoice Branding */}
      <Section title="Organization & Invoice Branding" description="These details appear on every newly generated invoice.">
        {orgLoading || !org ? (
          <div style={{ color: "var(--text-muted)", fontSize: "0.85rem" }}>Loading…</div>
        ) : (
          <form onSubmit={handleOrgSave} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14 }}>
              {orgTextFields.map(([lbl, key]) => (
                <div key={key}>
                  <label className="label">{lbl}</label>
                  <input
                    className="input-field"
                    type={key === "defaultTaxPercent" ? "number" : "text"}
                    value={org[key] ?? ""}
                    onChange={(e) => setOrgField(key, e.target.value)}
                  />
                </div>
              ))}
            </div>
            <div>
              <label className="label">Default Notes</label>
              <textarea className="input-field" style={{ minHeight: 70 }} value={org.defaultNotes ?? ""} onChange={(e) => setOrgField("defaultNotes", e.target.value)} />
            </div>
            <div>
              <label className="label">Default Terms &amp; Conditions</label>
              <textarea className="input-field" style={{ minHeight: 90 }} value={org.defaultTerms ?? ""} onChange={(e) => setOrgField("defaultTerms", e.target.value)} />
            </div>
            <div style={{ display: "flex", justifyContent: "flex-end" }}>
              <button type="submit" className="btn-primary" disabled={orgSaving}>
                {orgSaving ? "Saving..." : "Save Organization Settings"}
              </button>
            </div>
          </form>
        )}
      </Section>

      {/* Profile Picture */}
      <Section title="Profile Picture" description="Upload a profile photo for your admin account.">
        <div style={{ display: "flex", alignItems: "center", gap: 24 }}>
          <div style={{ position: "relative" }}>
            {profilePic ? (
              <img src={profilePic} alt="Profile" style={{ width: 80, height: 80, borderRadius: "50%", objectFit: "cover", border: "3px solid var(--primary)" }} />
            ) : (
              <div style={{ width: 80, height: 80, borderRadius: "50%", background: "linear-gradient(135deg, #5B5BD6, #8B8FD4)", display: "flex", alignItems: "center", justifyContent: "center", color: "#fff", fontFamily: "var(--font-display)", fontWeight: 800, fontSize: "1.4rem" }}>
                {user?.avatar || "US"}
              </div>
            )}
          </div>
          <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
            <input ref={fileInputRef} type="file" accept="image/*" onChange={handleProfilePicChange} style={{ display: "none" }} />
            <button className="btn-primary" onClick={() => fileInputRef.current?.click()} disabled={picLoading} style={{ fontSize: "0.82rem" }}>
              {picLoading ? "Uploading..." : "Upload Photo"}
            </button>
            {profilePic && (
              <button className="btn-ghost" onClick={removeProfilePic} style={{ fontSize: "0.82rem", color: "var(--danger)" }}>
                Remove Photo
              </button>
            )}
            <p style={{ fontSize: "0.75rem", color: "var(--text-muted)", margin: 0 }}>JPG, PNG or GIF. Max 2MB.</p>
          </div>
        </div>
      </Section>

      {/* Change Password */}
      <Section title="Change Password" description="Update your account password.">
        <form onSubmit={handlePasswordSave} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          {[
            ["Current Password",   "current"],
            ["New Password",       "newPw"],
            ["Confirm New Password","confirm"],
          ].map(([label, key]) => (
            <div key={key}>
              <label className="label">{label}</label>
              <input className="input-field" type="password"
                value={pwForm[key]}
                onChange={(e) => setPwForm({ ...pwForm, [key]: e.target.value })}
                placeholder="••••••••" />
            </div>
          ))}
          <div style={{ display: "flex", justifyContent: "flex-end", marginTop: 4 }}>
            <button type="submit" className="btn-primary" disabled={pwLoading}>
              {pwLoading ? "Updating..." : "Update Password"}
            </button>
          </div>
        </form>
      </Section>
    </div>
  );
}
