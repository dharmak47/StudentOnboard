import React, { useState, useEffect } from 'react';
import { adminAnalyticsApi } from '../../services/adminAnalyticsApi';
import { useToast } from '../../context/ToastContext';
import { AnalyticsChart } from '../../components/admin/AnalyticsChart';
import './AnalyticsDashboardPage.css';

const AnalyticsDashboardPage = () => {
  const [activeTab, setActiveTab] = useState('overview');
  const [metrics, setMetrics] = useState(null);
  const [studentProgress, setStudentProgress] = useState(null);
  const [loading, setLoading] = useState(true);
  const [dateRange, setDateRange] = useState({
    startMonth: getLastThreeMonthsStart(),
    endMonth: new Date().toISOString().split('T')[0]
  });
  const toast = useToast();

  useEffect(() => {
    fetchAnalytics();
  }, [dateRange]);

  const fetchAnalytics = async () => {
    try {
      setLoading(true);
      const result = await adminAnalyticsApi.getMonthlyAnalytics(
        dateRange.startMonth,
        dateRange.endMonth
      );
      if (result.data) {
        setMetrics(result.data);
      }
    } catch (error) {
      toast.error('Failed to load analytics');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const fetchStudentProgress = async () => {
    try {
      const result = await adminAnalyticsApi.getStudentProgress(null, 1, 50);
      if (result.data) {
        setStudentProgress(result.data);
      }
    } catch (error) {
      toast.error('Failed to load student progress');
      console.error(error);
    }
  };

  const handleTabChange = (tab) => {
    setActiveTab(tab);
    if (tab === 'progress' && !studentProgress) {
      fetchStudentProgress();
    }
  };

  if (loading) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: 400 }}>
        <div style={{ textAlign: 'center', color: 'var(--text-muted)' }}>Loading analytics...</div>
      </div>
    );
  }

  const currentMetrics = metrics?.months?.[0]?.metrics || {};

  return (
    <div className="page">
      <h1 style={{ fontFamily: 'var(--font-display)', fontSize: '1.8rem', fontWeight: 800, marginBottom: 24 }}>
        Analytics & Reports
      </h1>

      {/* Date Range Selector */}
      <div className="date-selector" style={{ marginBottom: 24 }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 12 }}>
          <div>
            <label style={{ display: 'block', fontSize: '0.9rem', fontWeight: 600, marginBottom: 8 }}>
              Start Month
            </label>
            <input
              type="date"
              value={dateRange.startMonth}
              onChange={(e) => setDateRange({ ...dateRange, startMonth: e.target.value })}
              style={{
                width: '100%',
                padding: '10px',
                border: '1px solid var(--border)',
                borderRadius: '6px',
                fontSize: '0.9rem'
              }}
            />
          </div>
          <div>
            <label style={{ display: 'block', fontSize: '0.9rem', fontWeight: 600, marginBottom: 8 }}>
              End Month
            </label>
            <input
              type="date"
              value={dateRange.endMonth}
              onChange={(e) => setDateRange({ ...dateRange, endMonth: e.target.value })}
              style={{
                width: '100%',
                padding: '10px',
                border: '1px solid var(--border)',
                borderRadius: '6px',
                fontSize: '0.9rem'
              }}
            />
          </div>
        </div>
      </div>

      {/* Tab Navigation */}
      <div className="tab-navigation" style={{ marginBottom: 24, borderBottom: '1px solid var(--border)' }}>
        {[
          { id: 'overview', label: 'Overview' },
          { id: 'progress', label: 'Student Progress' }
        ].map((tab) => (
          <button
            key={tab.id}
            onClick={() => handleTabChange(tab.id)}
            style={{
              padding: '12px 20px',
              fontSize: '0.95rem',
              fontWeight: activeTab === tab.id ? 600 : 500,
              color: activeTab === tab.id ? 'var(--primary)' : 'var(--text-muted)',
              background: 'none',
              border: 'none',
              borderBottom: activeTab === tab.id ? '2px solid var(--primary)' : 'none',
              cursor: 'pointer',
              transition: 'all 0.2s ease'
            }}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Overview Tab */}
      {activeTab === 'overview' && metrics && (
        <div>
          {/* KPI Cards */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 16, marginBottom: 24 }}>
            {[
              { label: 'New Enrollments', value: currentMetrics.newEnrollments || 0, icon: '📚' },
              { label: 'Completions', value: currentMetrics.completedCourses || 0, icon: '✅' },
              { label: 'Total Revenue', value: `₹${currentMetrics.totalRevenue || 0}`, icon: '💰' },
              { label: 'Active Students', value: currentMetrics.activeStudents || 0, icon: '👥' }
            ].map((kpi) => (
              <div key={kpi.label} className="card kpi-card" style={{ padding: 20 }}>
                <div style={{ fontSize: 24, marginBottom: 8 }}>{kpi.icon}</div>
                <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 8 }}>
                  {kpi.label}
                </div>
                <div style={{ fontSize: '1.8rem', fontWeight: 700, color: 'var(--text-primary)' }}>
                  {kpi.value}
                </div>
              </div>
            ))}
          </div>

          {/* Charts */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(400px, 1fr))', gap: 16 }}>
            <AnalyticsChart
              type="line"
              title="Enrollment Trend"
              data={{
                labels: metrics.months?.map(m => m.month.split(' ')[0]) || [],
                datasets: [{
                  label: 'New Enrollments',
                  data: metrics.months?.map(m => m.metrics.newEnrollments) || [],
                  borderColor: '#4CAF50',
                  backgroundColor: 'rgba(76, 175, 80, 0.1)'
                }]
              }}
            />

            <AnalyticsChart
              type="bar"
              title="Course Completions"
              data={{
                labels: metrics.months?.map(m => m.month.split(' ')[0]) || [],
                datasets: [{
                  label: 'Completions',
                  data: metrics.months?.map(m => m.metrics.completedCourses) || [],
                  backgroundColor: '#2196F3'
                }]
              }}
            />

            <AnalyticsChart
              type="pie"
              title="Revenue by Status"
              data={{
                labels: ['Completed', 'Pending'],
                datasets: [{
                  data: [
                    currentMetrics.paymentscompleted || 0,
                    currentMetrics.paymentsPending || 0
                  ],
                  backgroundColor: ['#4CAF50', '#FFC107']
                }]
              }}
            />

            <AnalyticsChart
              type="bar"
              title="Student Status"
              data={{
                labels: ['Approved', 'Pending'],
                datasets: [{
                  data: [
                    currentMetrics.approvedStudents || 0,
                    currentMetrics.pendingApprovals || 0
                  ],
                  backgroundColor: ['#4CAF50', '#FF9800']
                }]
              }}
            />
          </div>

          {/* Summary Statistics */}
          {metrics.summary && (
            <div className="card" style={{ padding: 20, marginTop: 24 }}>
              <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: 16 }}>
                Summary Statistics
              </h3>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 16 }}>
                {[
                  { label: 'Total Enrollments', value: metrics.summary.totalNewEnrollments },
                  { label: 'Total Completions', value: metrics.summary.totalCompletions },
                  { label: 'Total Revenue', value: `₹${metrics.summary.totalRevenue}` },
                  { label: 'Avg Monthly Growth', value: `${metrics.summary.averageMonthlyGrowth.toFixed(1)}` }
                ].map((stat) => (
                  <div key={stat.label}>
                    <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>
                      {stat.label}
                    </div>
                    <div style={{ fontSize: '1.4rem', fontWeight: 700, color: 'var(--primary)' }}>
                      {stat.value}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* Student Progress Tab */}
      {activeTab === 'progress' && studentProgress && (
        <div>
          {/* Summary Cards */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 16, marginBottom: 24 }}>
            {[
              { label: 'On Track', value: studentProgress.summary?.onTrackStudents || 0, color: '#4CAF50' },
              { label: 'At Risk', value: studentProgress.summary?.atRiskStudents || 0, color: '#FF9800' },
              { label: 'Behind Schedule', value: studentProgress.summary?.behindScheduleStudents || 0, color: '#F44336' }
            ].map((stat) => (
              <div key={stat.label} className="card" style={{ padding: 20, borderLeft: `4px solid ${stat.color}` }}>
                <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 8 }}>
                  {stat.label}
                </div>
                <div style={{ fontSize: '1.8rem', fontWeight: 700, color: 'var(--text-primary)' }}>
                  {stat.value}
                </div>
              </div>
            ))}
          </div>

          {/* Student Progress Table */}
          <div className="card" style={{ padding: 20 }}>
            <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: 16 }}>
              Student Progress Details
            </h3>
            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ borderBottom: '2px solid var(--border)' }}>
                    <th style={{ textAlign: 'left', padding: '12px', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-muted)' }}>
                      Student Name
                    </th>
                    <th style={{ textAlign: 'left', padding: '12px', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-muted)' }}>
                      Email
                    </th>
                    <th style={{ textAlign: 'left', padding: '12px', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-muted)' }}>
                      Overall Progress
                    </th>
                    <th style={{ textAlign: 'left', padding: '12px', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-muted)' }}>
                      Status
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {studentProgress.students?.map((student, idx) => {
                    const statusColor = student.overallProgress >= 70 ? '#4CAF50' :
                                       student.overallProgress >= 30 ? '#FF9800' : '#F44336';
                    const statusLabel = student.overallProgress >= 70 ? 'On Track' :
                                       student.overallProgress >= 30 ? 'At Risk' : 'Behind';

                    return (
                      <tr key={idx} style={{ borderBottom: '1px solid var(--border-light)' }}>
                        <td style={{ padding: '12px', fontSize: '0.9rem', color: 'var(--text-secondary)' }}>
                          {student.studentName}
                        </td>
                        <td style={{ padding: '12px', fontSize: '0.9rem', color: 'var(--text-secondary)' }}>
                          {student.email}
                        </td>
                        <td style={{ padding: '12px' }}>
                          <div style={{ fontSize: '0.9rem', fontWeight: 600, color: 'var(--text-primary)' }}>
                            {student.overallProgress.toFixed(1)}%
                          </div>
                        </td>
                        <td style={{ padding: '12px' }}>
                          <span style={{
                            display: 'inline-block',
                            padding: '4px 12px',
                            borderRadius: '12px',
                            fontSize: '0.8rem',
                            fontWeight: 600,
                            backgroundColor: `${statusColor}20`,
                            color: statusColor
                          }}>
                            {statusLabel}
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

function getLastThreeMonthsStart() {
  const date = new Date();
  date.setMonth(date.getMonth() - 2);
  return date.toISOString().split('T')[0];
}

export default AnalyticsDashboardPage;
