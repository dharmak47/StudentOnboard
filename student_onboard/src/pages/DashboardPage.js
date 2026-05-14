// src/pages/DashboardPage.js
import React, { useEffect, useState, useCallback } from "react";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from "recharts";
import { useStudentStats } from "../hooks/useStudents";
import { useCourses } from "../hooks/useCourses";
import { analyticsApi } from "../services/api";
import { StatCard, StatusBadge, PageLoader } from "../components/common";

const COLORS = ["#5B5BD6", "#8B8FD4", "#C4C3E8", "#F5D7F0", "#EBBFE5"];

// Fallback data shown when backend is offline
const FALLBACK_TREND = [
  { month: "Oct", students: 18 }, { month: "Nov", students: 27 },
  { month: "Dec", students: 22 }, { month: "Jan", students: 35 },
  { month: "Feb", students: 42 }, { month: "Mar", students: 48 },
];
const FALLBACK_DIST = [
  { name: "Frontend", value: 34 }, { name: "Backend", value: 22 },
  { name: "Design", value: 18 },   { name: "Data", value: 14 },
  { name: "Full Stack", value: 12 },
];

export default function DashboardPage() {
  const { stats, loading: statsLoading, refetchStats } = useStudentStats();
  const { courses, loading: coursesLoading, refetch: refetchCourses } = useCourses();
  const [analytics, setAnalytics] = useState(null);
  const [backendOffline, setBackendOffline] = useState(false);

  const fetchAnalytics = useCallback(() => {
    analyticsApi.overview()
      .then((res) => setAnalytics(res.data))
      .catch((err) => {
        if (err.isNetworkError || err.status === 0) {
          setBackendOffline(true);
          setAnalytics({ enrollmentTrend: FALLBACK_TREND, courseDistribution: FALLBACK_DIST });
        }
      });
  }, []);

  useEffect(() => {
    fetchAnalytics();
    const interval = setInterval(() => {
      refetchStats();
      refetchCourses();
      fetchAnalytics();
    }, 30000); // Auto-refresh every 30s
    return () => clearInterval(interval);
  }, [fetchAnalytics, refetchStats, refetchCourses]);

  if ((statsLoading || coursesLoading) && !backendOffline) return <PageLoader />;

  return (
    <div className="page" style={{ display: "flex", flexDirection: "column", gap: 28 }}>

      {/* Offline banner */}
      {backendOffline && (
        <div style={{ background: "#EEF2FF", border: "1px solid #C7D2FE", borderRadius: 12, padding: "12px 20px", display: "flex", alignItems: "center", gap: 12 }}>
          <span style={{ fontSize: 20 }}>⏳</span>
          <div>
            <strong style={{ color: "#3730A3", fontFamily: "var(--font-display)", fontSize: "0.9rem" }}>Students &amp; Courses APIs coming soon</strong>
            <p style={{ color: "#4338CA", fontSize: "0.8rem", margin: 0 }}>
              Your teammate is building the Students and Courses controllers. Dashboard will show live data once those APIs are ready.
            </p>
          </div>
        </div>
      )}

      {/* Stat Cards */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: 18 }}>
        <StatCard icon="👨‍🎓" label="Total Students" value={stats?.total ?? "—"}    sub="All registered"    accent="#5B5BD6" index={0} />
        <StatCard icon="✅" label="Approved"       value={stats?.approved ?? "—"} sub="Active learners"   accent="#22C55E" index={1} />
        <StatCard icon="⏳" label="Pending Review"  value={stats?.pending ?? "—"}  sub="Awaiting action"   accent="#F59E0B" index={2} />
        <StatCard icon="🚫" label="Blocked"         value={stats?.blocked ?? "—"}  sub="Restricted access" accent="#EF4444" index={3} />
        <StatCard icon="📚" label="Total Courses"   value={courses?.length ?? "—"} sub="In catalogue"      accent="#8B8FD4" index={4} />
      </div>

      {/* Charts Row */}
      {analytics && (
        <div style={{ display: "grid", gridTemplateColumns: "1fr 360px", gap: 18 }}>
          <div className="card animate-fadeUp stagger-2" style={{ padding: 28 }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1rem", fontWeight: 700, marginBottom: 4 }}>Enrollment Trend</h3>
            <p style={{ color: "var(--text-muted)", fontSize: "0.8rem", marginBottom: 22 }}>Monthly student registrations {backendOffline && "(demo data)"}</p>
            <ResponsiveContainer width="100%" height={220}>
              <LineChart data={analytics.enrollmentTrend}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                <XAxis dataKey="month" tick={{ fill: "var(--text-muted)", fontSize: 12, fontFamily: "var(--font-body)" }} axisLine={false} tickLine={false} />
                <YAxis tick={{ fill: "var(--text-muted)", fontSize: 12, fontFamily: "var(--font-body)" }} axisLine={false} tickLine={false} />
                <Tooltip contentStyle={{ background: "var(--surface)", border: "1px solid var(--border)", borderRadius: 10, fontFamily: "var(--font-body)", fontSize: "0.85rem" }} />
                <Line type="monotone" dataKey="students" stroke="#5B5BD6" strokeWidth={3} dot={{ fill: "#5B5BD6", r: 5 }} activeDot={{ r: 7 }} />
              </LineChart>
            </ResponsiveContainer>
          </div>

          <div className="card animate-fadeUp stagger-3" style={{ padding: 28 }}>
            <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1rem", fontWeight: 700, marginBottom: 4 }}>Course Distribution</h3>
            <p style={{ color: "var(--text-muted)", fontSize: "0.8rem", marginBottom: 16 }}>Students by category</p>
            <ResponsiveContainer width="100%" height={220}>
              <PieChart>
                <Pie data={analytics.courseDistribution} cx="50%" cy="45%" outerRadius={80} innerRadius={45} dataKey="value" paddingAngle={3}>
                  {analytics.courseDistribution.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
                </Pie>
                <Legend iconType="circle" iconSize={8} wrapperStyle={{ fontSize: "0.75rem", fontFamily: "var(--font-body)" }} />
                <Tooltip contentStyle={{ background: "var(--surface)", border: "1px solid var(--border)", borderRadius: 10, fontFamily: "var(--font-body)", fontSize: "0.85rem" }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}

      {/* Recent Courses */}
      <div className="card animate-fadeUp stagger-4" style={{ padding: 28 }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 20 }}>
          <h3 style={{ fontFamily: "var(--font-display)", fontSize: "1rem", fontWeight: 700 }}>Recent Courses</h3>
        </div>
        {!courses?.length ? (
          <p style={{ color: "var(--text-muted)", fontSize: "0.875rem" }}>No courses available. Start the backend to load data.</p>
        ) : (
          <div style={{ display: "flex", flexDirection: "column" }}>
            {courses.slice(0, 5).map((c, i) => (
              <div key={c.id} style={{ display: "flex", alignItems: "center", gap: 16, padding: "14px 0", borderBottom: i < courses.length - 1 ? "1px solid var(--border-light)" : "none" }}>
                <div style={{ width: 44, height: 44, borderRadius: 12, background: "var(--surface-3)", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 20, flexShrink: 0 }}>{c.thumbnail}</div>
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontFamily: "var(--font-display)", fontWeight: 600, fontSize: "0.9rem", color: "var(--text-primary)", marginBottom: 2 }}>{c.title}</div>
                  <div style={{ fontSize: "0.78rem", color: "var(--text-muted)" }}>{c.instructor} · {c.duration}</div>
                </div>
                <div style={{ textAlign: "right" }}>
                  <div style={{ fontFamily: "var(--font-display)", fontWeight: 700, fontSize: "0.9rem", color: "var(--text-primary)" }}>₹{c.price?.toLocaleString()}</div>
                  <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{c.studentsCount} students</div>
                </div>
                <StatusBadge status={c.status} />
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}