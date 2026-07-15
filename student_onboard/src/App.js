// src/App.js
import React, { useState, useEffect } from "react";
import { AuthProvider, useAuth } from "./context/AuthContext";
import { ToastProvider, useToast } from "./context/ToastContext";
import Sidebar from "./components/layout/Sidebar";
import Topbar  from "./components/layout/Topbar";
import LoginPage     from "./pages/LoginPage";
import DashboardPage from "./pages/DashboardPage";
import StudentsPage       from "./pages/StudentsPage";
import RegistrationsPage  from "./pages/RegistrationsPage";
import CoursesPage        from "./pages/CoursesPage";
import EnquiriesPage from "./pages/EnquiriesPage";
import NotificationsPage  from "./pages/NotificationsPage";
import FaqsPage           from "./pages/FaqsPage";
import SettingsPage       from "./pages/SettingsPage";

import "./styles/globals.css";

const PAGES = {
  dashboard:      DashboardPage,
  students:       StudentsPage,
  registrations:  RegistrationsPage,
  courses:        CoursesPage,
  enquiries:     EnquiriesPage,
  notifications:  NotificationsPage,
  faqs:           FaqsPage,
  settings:       SettingsPage,
};

function AppShell() {
  const { isAuthenticated, logout } = useAuth();
  const toast = useToast();
  const [page, setPage] = useState("dashboard");

  // Listen for 401 unauthorised events dispatched by api.js
  useEffect(() => {
    const handle = () => {
      toast.error("Session expired. Please log in again.");
      logout();
    };
    window.addEventListener("edu:unauthorized", handle);
    return () => window.removeEventListener("edu:unauthorized", handle);
  }, [logout, toast]);

  if (!isAuthenticated) return <LoginPage />;

  const PageComponent = PAGES[page] || DashboardPage;

  return (
    <div style={{ display: "flex", height: "100vh", overflow: "hidden", background: "var(--surface-2)" }}>
      <Sidebar active={page} onChange={setPage} />
      <div style={{ flex: 1, display: "flex", flexDirection: "column", overflow: "hidden" }}>
        <Topbar page={page} />
        <main key={page} style={{ flex: 1, overflowY: "auto", padding: 32, animation: "fadeUp 0.3s ease both" }}>
          <PageComponent />
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
