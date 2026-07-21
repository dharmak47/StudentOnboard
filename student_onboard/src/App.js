// src/App.js
import React, { useState, useEffect } from "react";
import { AuthProvider, useAuth } from "./context/AuthContext";
import { ToastProvider, useToast } from "./context/ToastContext";
import Sidebar from "./components/layout/Sidebar";
import Topbar  from "./components/layout/Topbar";
import LoginPage     from "./pages/LoginPage";
import SignupPage    from "./pages/SignupPage";
import OtpVerificationPage from "./pages/OtpVerificationPage";
import DashboardPage from "./pages/DashboardPage";
import StudentsPage       from "./pages/StudentsPage";
import RegistrationsPage  from "./pages/RegistrationsPage";
import CoursesPage        from "./pages/CoursesPage";
import EnquiriesPage from "./pages/EnquiriesPage";
import NotificationsPage  from "./pages/NotificationsPage";
import FaqsPage           from "./pages/FaqsPage";
import InvoicesPage       from "./pages/InvoicesPage";
import SettingsPage       from "./pages/SettingsPage";
import AnalyticsDashboardPage from "./pages/admin/AnalyticsDashboardPage";
import CourseCompletionPage from "./pages/admin/CourseCompletionPage";
import StudentDashboardPage      from "./pages/StudentDashboardPage";
import StudentCoursesPage        from "./pages/StudentCoursesPage";
import StudentInvoicesPage       from "./pages/StudentInvoicesPage";
import StudentProfilePage        from "./pages/StudentProfilePage";
import StudentNotificationsPage  from "./pages/StudentNotificationsPage";
import StudentCertificatesPage   from "./pages/StudentCertificatesPage";
import StudentPaymentHistoryPage from "./pages/StudentPaymentHistoryPage";
import StudentProgressTrackingPage from "./pages/StudentProgressTrackingPage";
import StudentHelpPage           from "./pages/StudentHelpPage";
import ApprovalPendingPage       from "./pages/ApprovalPendingPage";

import "./styles/globals.css";

const ADMIN_PAGES = {
  dashboard:           DashboardPage,
  students:            StudentsPage,
  registrations:       RegistrationsPage,
  courses:             CoursesPage,
  "course-completion": CourseCompletionPage,
  analytics:           AnalyticsDashboardPage,
  enquiries:           EnquiriesPage,
  invoices:            InvoicesPage,
  notifications:       NotificationsPage,
  faqs:                FaqsPage,
  settings:            SettingsPage,
};

const STUDENT_PAGES = {
  dashboard:      StudentDashboardPage,
  courses:        StudentCoursesPage,
  invoices:       StudentInvoicesPage,
  progress:       StudentProgressTrackingPage,
  certificates:   StudentCertificatesPage,
  payments:       StudentPaymentHistoryPage,
  notifications:  StudentNotificationsPage,
  help:           StudentHelpPage,
  profile:        StudentProfilePage,
};

function AppShell() {
  const { isAuthenticated, logout, isAdmin, isStudent, isPending } = useAuth();
  const toast = useToast();
  const [page, setPage] = useState("dashboard");
  const [authView, setAuthView] = useState("login"); // "login" | "signup" | "otp"
  const [otpEmail, setOtpEmail] = useState("");

  // Listen for 401 unauthorised events dispatched by api.js
  useEffect(() => {
    const handle = () => {
      toast.error("Session expired. Please log in again.");
      logout();
    };
    window.addEventListener("edu:unauthorized", handle);
    return () => window.removeEventListener("edu:unauthorized", handle);
  }, [logout, toast]);

  // Auth flow routing
  if (!isAuthenticated) {
    if (authView === "signup") {
      return (
        <SignupPage
          onBack={() => setAuthView("login")}
          onSuccess={(email) => {
            setOtpEmail(email);
            setAuthView("otp");
          }}
        />
      );
    }
    if (authView === "otp") {
      return (
        <OtpVerificationPage
          email={otpEmail}
          onBack={() => setAuthView("signup")}
          onSuccess={() => setAuthView("login")}
        />
      );
    }
    return <LoginPage onSignup={() => setAuthView("signup")} />;
  }

  if (isStudent && isPending) return <ApprovalPendingPage />;

  const PAGES = isAdmin ? ADMIN_PAGES : STUDENT_PAGES;
  const PageComponent = PAGES[page] || (isAdmin ? DashboardPage : StudentDashboardPage);

  return (
    <div style={{ display: "flex", height: "100vh", overflow: "hidden", background: "var(--surface-2)" }}>
      <Sidebar active={page} onChange={setPage} />
      <div style={{ flex: 1, display: "flex", flexDirection: "column", overflow: "hidden" }}>
        <Topbar page={page} />
        <main key={page} style={{ flex: 1, overflowY: "auto", padding: 32, animation: "fadeUp 0.3s ease both" }}>
          <PageComponent onNavigate={setPage} />
        </main>
      </div>
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <ToastProvider>
        <AppShell />
      </ToastProvider>
    </AuthProvider>
  );
}
