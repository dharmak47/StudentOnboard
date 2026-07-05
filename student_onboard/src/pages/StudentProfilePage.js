import React, { useState, useEffect } from "react";
import { studentApi } from "../services/api";
import { useAuth } from "../context/AuthContext";
import { useToast } from "../context/ToastContext";

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

export default function StudentProfilePage() {
  const { updateUser } = useAuth();
  const toast = useToast();
  const [loading, setLoading] = useState(true);
  const [picLoading, setPicLoading] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  const fileInputRef = React.useRef(null);

  const [profilePic, setProfilePic] = useState(() => localStorage.getItem("edu_user_pic") || "");
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    phoneNumber: "",
    dateOfBirth: "",
    address: "",
    education: "",
  });

  // Fetch profile on mount
  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      try {
        const res = await studentApi.getProfile();
        const data = res.data;
        setForm({
          firstName: data.firstName || "",
          lastName: data.lastName || "",
          phoneNumber: data.phoneNumber || "",
          dateOfBirth: data.dateOfBirth ? data.dateOfBirth.split("T")[0] : "",
          address: data.address || "",
          education: data.education || "",
        });
        if (data.profilePhotoUrl) {
          setProfilePic(data.profilePhotoUrl);
        }
      } catch (err) {
        toast.error(err.message || "Failed to load profile.");
      } finally {
        setLoading(false);
      }
    };
    fetch();
  }, [toast]);

  const handleProfilePicChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      toast.error("Image must be under 2MB.");
      return;
    }

    setPicLoading(true);
    try {
      const res = await studentApi.uploadPhoto(file);
      const photoUrl = res.data;
      localStorage.setItem("edu_user_pic", photoUrl);
      setProfilePic(photoUrl);
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
    toast.success("Profile picture removed.");
  };

  const handleSaveProfile = async (e) => {
    e.preventDefault();
    setFormLoading(true);
    try {
      await studentApi.updateProfile(form);
      updateUser({
        name: `${form.firstName} ${form.lastName}`,
      });
      toast.success("Profile updated successfully!");
    } catch (err) {
      toast.error(err.message || "Failed to update profile.");
    } finally {
      setFormLoading(false);
    }
  };

  if (loading) {
    return (
      <div style={{ display: "flex", alignItems: "center", justifyContent: "center", minHeight: 400 }}>
        <div style={{ textAlign: "center", color: "var(--text-muted)" }}>Loading...</div>
      </div>
    );
  }

  return (
    <div className="page" style={{ maxWidth: 640 }}>
      {/* Profile Picture */}
      <Section title="Profile Picture" description="Upload a profile photo for your account.">
        <div style={{ display: "flex", alignItems: "center", gap: 24 }}>
          <div style={{ position: "relative" }}>
            {profilePic ? (
              <img src={profilePic} alt="Profile" style={{ width: 80, height: 80, borderRadius: "50%", objectFit: "cover", border: "3px solid var(--primary)" }} />
            ) : (
              <div style={{ width: 80, height: 80, borderRadius: "50%", background: "linear-gradient(135deg, #5B5BD6, #8B8FD4)", display: "flex", alignItems: "center", justifyContent: "center", color: "#fff", fontFamily: "var(--font-display)", fontWeight: 800, fontSize: "1.4rem" }}>
                {form.firstName?.[0]}{form.lastName?.[0]}
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

      {/* Personal Information */}
      <Section title="Personal Information" description="Update your profile information.">
        <form onSubmit={handleSaveProfile} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
            {[
              ["First Name", "firstName"],
              ["Last Name", "lastName"],
            ].map(([label, key]) => (
              <div key={key}>
                <label className="label">{label}</label>
                <input
                  className="input-field"
                  type="text"
                  value={form[key]}
                  onChange={(e) => setForm({ ...form, [key]: e.target.value })}
                  placeholder={label}
                />
              </div>
            ))}
          </div>

          {[
            ["Phone Number", "phoneNumber", "tel"],
            ["Date of Birth", "dateOfBirth", "date"],
          ].map(([label, key, type]) => (
            <div key={key}>
              <label className="label">{label}</label>
              <input
                className="input-field"
                type={type}
                value={form[key]}
                onChange={(e) => setForm({ ...form, [key]: e.target.value })}
                placeholder={label}
              />
            </div>
          ))}

          <div>
            <label className="label">Address</label>
            <textarea
              className="input-field"
              value={form.address}
              onChange={(e) => setForm({ ...form, address: e.target.value })}
              placeholder="Address"
              rows={3}
              style={{ fontFamily: "var(--font-body)", resize: "vertical" }}
            />
          </div>

          <div>
            <label className="label">Education</label>
            <input
              className="input-field"
              type="text"
              value={form.education}
              onChange={(e) => setForm({ ...form, education: e.target.value })}
              placeholder="Education (e.g., Bachelor's Degree)"
            />
          </div>

          <div style={{ display: "flex", justifyContent: "flex-end", marginTop: 4 }}>
            <button type="submit" className="btn-primary" disabled={formLoading}>
              {formLoading ? "Saving..." : "Save Changes"}
            </button>
          </div>
        </form>
      </Section>
    </div>
  );
}
