import React, { useState, useEffect } from 'react';
import {
  getIncompleteRegistrations,
  markCourseComplete
} from '../../services/courseCompletionApi';
import { coursesApi, certificatesApi } from '../../services/api';
import { useToast } from '../../context/ToastContext';
import './CourseCompletionPage.css';

/**
 * Course Completion Management Page
 * Allows admins to mark student courses as complete with grades and notes
 */
const CourseCompletionPage = () => {
  const [courses, setCourses] = useState([]);
  const [selectedCourse, setSelectedCourse] = useState(null);
  const [registrations, setRegistrations] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [pagination, setPagination] = useState({ page: 1, pageSize: 20 });
  const [selectedRegistrations, setSelectedRegistrations] = useState(new Set());
  const [downloadingId, setDownloadingId] = useState(null);
  const toast = useToast();

  // Modal state for marking course complete
  const [showModal, setShowModal] = useState(false);
  const [currentRegistration, setCurrentRegistration] = useState(null);
  const [completionForm, setCompletionForm] = useState({
    grade: '',
    adminNotes: '',
    completionDate: new Date().toISOString().split('T')[0],
    completionPercentage: '',
    markAsComplete: false
  });

  // Load courses on mount
  useEffect(() => {
    const loadCourses = async () => {
      try {
        setLoading(true);
        const response = await coursesApi.getAll();
        if (response.data && Array.isArray(response.data)) {
          setCourses(response.data);
        }
        setError(null);
      } catch (err) {
        console.error('Error loading courses:', err);
        setError('Failed to load courses. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    loadCourses();
  }, []);

  // Load incomplete registrations when course is selected
  useEffect(() => {
    if (selectedCourse) {
      const fetchRegistrations = async () => {
        await loadIncompleteRegistrations(1);
      };
      fetchRegistrations();
    }
  }, [selectedCourse]);

  const loadIncompleteRegistrations = async (page) => {
    try {
      setLoading(true);
      const response = await getIncompleteRegistrations(selectedCourse.id, page, pagination.pageSize);

      if (response.success && response.data) {
        setRegistrations(response.data.items || []);
        setPagination({
          page: response.data.page,
          pageSize: response.data.pageSize,
          totalCount: response.data.totalCount,
          totalPages: response.data.totalPages
        });
      } else {
        setError(response.message || 'Failed to load registrations');
      }
      setError(null);
    } catch (err) {
      console.error('Error loading registrations:', err);
      setError('Failed to load course registrations. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleCourseSelect = (course) => {
    setSelectedCourse(course);
    setSelectedRegistrations(new Set());
    setSuccess(null);
    setError(null);
  };

  const handleSelectRegistration = (registrationId) => {
    const newSelected = new Set(selectedRegistrations);
    if (newSelected.has(registrationId)) {
      newSelected.delete(registrationId);
    } else {
      newSelected.add(registrationId);
    }
    setSelectedRegistrations(newSelected);
  };

  const handleSelectAll = () => {
    if (selectedRegistrations.size === registrations.length) {
      setSelectedRegistrations(new Set());
    } else {
      setSelectedRegistrations(new Set(registrations.map(r => r.registrationId)));
    }
  };

  const handleDownloadCert = async (regId) => {
    setDownloadingId(regId);
    try {
      await certificatesApi.download(regId);
      toast.success("Certificate downloaded successfully.");
    } catch (err) {
      toast.error(err.message || "Failed to download certificate.");
    } finally {
      setDownloadingId(null);
    }
  };

  const openCompletionModal = (registration) => {
    setCurrentRegistration(registration);
    setCompletionForm({
      grade: '',
      adminNotes: '',
      completionDate: new Date().toISOString().split('T')[0],
      completionPercentage: '',
      markAsComplete: false
    });
    setShowModal(true);
  };

  const closeCompletionModal = () => {
    setShowModal(false);
    setCurrentRegistration(null);
    setCompletionForm({
      grade: '',
      adminNotes: '',
      completionDate: new Date().toISOString().split('T')[0],
      completionPercentage: '',
      markAsComplete: false
    });
  };

  const handleFormChange = (field, value) => {
    setCompletionForm(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmitCompletion = async () => {
    if (!currentRegistration) return;

    try {
      setLoading(true);
      const grade = completionForm.grade ? parseFloat(completionForm.grade) : null;
      let completionPercentage = completionForm.completionPercentage ? parseFloat(completionForm.completionPercentage) : null;

      if (grade !== null && (grade < 0 || grade > 100)) {
        setError('Grade must be between 0 and 100.');
        setLoading(false);
        return;
      }

      if (completionPercentage !== null && (completionPercentage < 0 || completionPercentage > 100)) {
        setError('Completion percentage must be between 0 and 100.');
        setLoading(false);
        return;
      }

      // If marking as complete, force percentage to 100 if not already specified
      if (completionForm.markAsComplete && completionPercentage === null) {
        completionPercentage = 100;
      }

      const response = await markCourseComplete({
        registrationId: currentRegistration.registrationId,
        grade,
        adminNotes: completionForm.adminNotes || null,
        completionDate: completionForm.markAsComplete ? completionForm.completionDate : null,
        completionPercentage,
        markAsComplete: completionForm.markAsComplete
      });

      if (response.success) {
        const actionMessage = completionForm.markAsComplete
          ? `Course completed and closed for ${currentRegistration.studentName} at ${completionPercentage}%`
          : `Progress updated to ${completionPercentage}% for ${currentRegistration.studentName} (course remains active)`;
        setSuccess(actionMessage);
        
        // Auto-download certificate if completion is 100% or marked as complete
        if (completionPercentage >= 100 || completionForm.markAsComplete) {
          toast.success(`Progress is 100%. Auto-downloading certificate for ${currentRegistration.studentName}...`);
          // Use setTimeout to ensure the toast is seen and state resolves
          setTimeout(() => {
             handleDownloadCert(currentRegistration.registrationId);
          }, 500);
        }

        closeCompletionModal();
        loadIncompleteRegistrations(pagination.page);
      } else {
        setError(response.message || 'Failed to update course');
      }
      setLoading(false);
    } catch (err) {
      console.error('Error marking course complete:', err);
      setError('Failed to mark course complete. Please try again.');
      setLoading(false);
    }
  };

  const handlePreviousPage = () => {
    if (pagination.page > 1) {
      loadIncompleteRegistrations(pagination.page - 1);
    }
  };

  const handleNextPage = () => {
    if (pagination.page < pagination.totalPages) {
      loadIncompleteRegistrations(pagination.page + 1);
    }
  };

  const getStatusClass = (registration) => {
    if (registration.isAtRisk) return 'at-risk';
    if (registration.progressPercentage >= 80) return 'on-track';
    return 'behind';
  };

  return (
    <div className="course-completion-page">
      <div className="page-header">
        <h1>Course Completion Management</h1>
        <p>Mark student courses as complete and assign grades</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="completion-container">
        {/* Course Selection */}
        <div className="course-selector-section">
          <h2>Select a Course</h2>
          {loading && !selectedCourse ? (
            <div className="loading-spinner">Loading courses...</div>
          ) : (
            <div className="course-grid">
              {courses.length === 0 ? (
                <p className="no-data">No courses available</p>
              ) : (
                courses.map(course => (
                  <div
                    key={course.id}
                    className={`course-card ${selectedCourse?.id === course.id ? 'active' : ''}`}
                    onClick={() => handleCourseSelect(course)}
                  >
                    <h3>{course.title}</h3>
                    <p className="course-description">{course.description}</p>
                    <span className="course-code">{course.courseCode}</span>
                  </div>
                ))
              )}
            </div>
          )}
        </div>

        {/* Incomplete Registrations */}
        {selectedCourse && (
          <div className="registrations-section">
            <div className="section-header">
              <h2>Incomplete Registrations: {selectedCourse.title}</h2>
              <span className="count-badge">{pagination.totalCount || 0} students</span>
            </div>

            {loading ? (
              <div className="loading-spinner">Loading registrations...</div>
            ) : registrations.length === 0 ? (
              <div className="no-data-container">
                <p>No incomplete registrations for this course</p>
              </div>
            ) : (
              <>
                <table className="registrations-table">
                  <thead>
                    <tr>
                      <th>
                        <input
                          type="checkbox"
                          checked={selectedRegistrations.size === registrations.length && registrations.length > 0}
                          onChange={handleSelectAll}
                          aria-label="Select all registrations"
                        />
                      </th>
                      <th>Student Name</th>
                      <th>Email</th>
                      <th>Progress</th>
                      <th>Status</th>
                      <th>Enrolled Date</th>
                      <th>Expected Completion</th>
                      <th>Payment Status</th>
                      <th>Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {registrations.map(registration => (
                      <tr key={registration.registrationId} className={getStatusClass(registration)}>
                        <td>
                          <input
                            type="checkbox"
                            checked={selectedRegistrations.has(registration.registrationId)}
                            onChange={() => handleSelectRegistration(registration.registrationId)}
                            aria-label={`Select ${registration.studentName}`}
                          />
                        </td>
                        <td className="student-name">{registration.studentName}</td>
                        <td>{registration.studentEmail}</td>
                        <td>
                          <div className="progress-cell">
                            <div className="progress-bar">
                              <div
                                className="progress-fill"
                                style={{ width: `${registration.progressPercentage}%` }}
                              />
                            </div>
                            <span className="progress-text">{Math.round(registration.progressPercentage)}%</span>
                          </div>
                        </td>
                        <td>
                          <span className={`status-badge ${getStatusClass(registration)}`}>
                            {registration.isAtRisk ? 'At Risk' : registration.progressPercentage >= 80 ? 'On Track' : 'Behind'}
                          </span>
                        </td>
                        <td>{new Date(registration.enrolledDate).toLocaleDateString()}</td>
                        <td>
                          {registration.expectedCompletionDate
                            ? new Date(registration.expectedCompletionDate).toLocaleDateString()
                            : 'N/A'}
                        </td>
                        <td>{registration.paymentStatus}</td>
                        <td style={{ display: 'flex', gap: '8px' }}>
                          <button
                            className="btn-complete"
                            onClick={() => openCompletionModal(registration)}
                            disabled={loading}
                          >
                            Mark Complete
                          </button>
                          {registration.progressPercentage >= 100 && (
                            <button
                              className="btn-complete"
                              style={{ backgroundColor: 'var(--primary)', color: 'white' }}
                              onClick={() => handleDownloadCert(registration.registrationId)}
                              disabled={downloadingId === registration.registrationId || loading}
                              title="Download Certificate"
                            >
                              {downloadingId === registration.registrationId ? "⏳" : "📥"}
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>

                {/* Pagination */}
                {pagination.totalPages > 1 && (
                  <div className="pagination">
                    <button
                      onClick={handlePreviousPage}
                      disabled={pagination.page === 1}
                      className="btn-pagination"
                    >
                      ← Previous
                    </button>
                    <span className="page-info">
                      Page {pagination.page} of {pagination.totalPages}
                    </span>
                    <button
                      onClick={handleNextPage}
                      disabled={pagination.page === pagination.totalPages}
                      className="btn-pagination"
                    >
                      Next →
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        )}
      </div>

      {/* Completion Modal */}
      {showModal && currentRegistration && (
        <div className="modal-overlay" onClick={closeCompletionModal}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Mark Course as Complete</h2>
              <button className="close-btn" onClick={closeCompletionModal}>×</button>
            </div>

            <div className="modal-body">
              <div className="student-info">
                <p><strong>Student:</strong> {currentRegistration.studentName}</p>
                <p><strong>Email:</strong> {currentRegistration.studentEmail}</p>
                <p><strong>Course:</strong> {selectedCourse?.title}</p>
                <p><strong>Progress:</strong> {Math.round(currentRegistration.progressPercentage)}%</p>
              </div>

              <div className="form-group">
                <label htmlFor="completionPercentage">Completion Percentage (0-100%)</label>
                <input
                  id="completionPercentage"
                  type="number"
                  min="0"
                  max="100"
                  step="1"
                  placeholder="Enter completion percentage (e.g., 100)"
                  value={completionForm.completionPercentage}
                  onChange={e => handleFormChange('completionPercentage', e.target.value)}
                  disabled={loading}
                />
                <small style={{ display: 'block', marginTop: '5px', color: '#666' }}>
                  Updates the student's course progress percentage
                </small>
              </div>

              <div style={{ padding: '12px', backgroundColor: completionForm.markAsComplete ? '#DCFCE7' : '#FEF3C7', border: `1px solid ${completionForm.markAsComplete ? '#86EFAC' : '#FCD34D'}`, borderRadius: '6px', marginBottom: '16px' }}>
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: '10px', marginBottom: '12px' }}>
                  <input
                    id="markAsComplete"
                    type="checkbox"
                    checked={completionForm.markAsComplete}
                    onChange={e => handleFormChange('markAsComplete', e.target.checked)}
                    disabled={loading}
                    style={{ width: '18px', height: '18px', cursor: 'pointer', marginTop: '2px', flexShrink: 0 }}
                  />
                  <label htmlFor="markAsComplete" style={{ margin: 0, cursor: 'pointer', flex: 1 }}>
                    <strong style={{ color: completionForm.markAsComplete ? '#15803D' : '#92400E' }}>Mark Course as Fully Complete</strong>
                    <br />
                    <small style={{ color: completionForm.markAsComplete ? '#15803D' : '#92400E' }}>Check this to set isCompleted = true and close the course for the student</small>
                  </label>
                </div>

                {/* Action summary */}
                <div style={{ padding: '10px', backgroundColor: 'rgba(255,255,255,0.6)', borderRadius: '4px', fontSize: '0.85rem', color: '#333' }}>
                  <strong>What will happen:</strong>
                  <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
                    {completionForm.markAsComplete ? (
                      <>
                        <li>Set isCompleted = <strong style={{ color: '#15803D' }}>TRUE</strong></li>
                        <li>Set progress to <strong style={{ color: '#15803D' }}>{completionForm.completionPercentage || '100'}%</strong></li>
                        <li>Set completion date to <strong style={{ color: '#15803D' }}>{completionForm.completionDate}</strong></li>
                        <li>Course will be marked as closed</li>
                      </>
                    ) : (
                      <>
                        <li>Keep isCompleted = <strong style={{ color: '#92400E' }}>FALSE</strong></li>
                        <li>Update progress to <strong style={{ color: '#92400E' }}>{completionForm.completionPercentage || '(no change)'}%</strong></li>
                        <li>Course remains active for the student</li>
                      </>
                    )}
                  </ul>
                </div>
              </div>

              <div className="form-group">
                <label htmlFor="grade">Grade/Score (0-100)</label>
                <input
                  id="grade"
                  type="number"
                  min="0"
                  max="100"
                  step="0.5"
                  placeholder="Enter grade (optional)"
                  value={completionForm.grade}
                  onChange={e => handleFormChange('grade', e.target.value)}
                  disabled={loading}
                />
              </div>

              <div className="form-group">
                <label htmlFor="completionDate">Completion Date</label>
                <input
                  id="completionDate"
                  type="date"
                  value={completionForm.completionDate}
                  onChange={e => handleFormChange('completionDate', e.target.value)}
                  disabled={loading}
                />
              </div>

              <div className="form-group">
                <label htmlFor="adminNotes">Admin Notes</label>
                <textarea
                  id="adminNotes"
                  placeholder="Add any notes about this completion..."
                  value={completionForm.adminNotes}
                  onChange={e => handleFormChange('adminNotes', e.target.value)}
                  rows="4"
                  disabled={loading}
                />
              </div>
            </div>

            <div className="modal-footer">
              <button
                className="btn-secondary"
                onClick={closeCompletionModal}
                disabled={loading}
              >
                Cancel
              </button>
              <button
                className="btn-primary"
                onClick={handleSubmitCompletion}
                disabled={loading}
                title={completionForm.markAsComplete ? 'Mark course as complete and close it' : 'Update progress percentage only'}
              >
                {loading ? (completionForm.markAsComplete ? 'Completing Course...' : 'Updating Progress...') : (completionForm.markAsComplete ? '✓ Complete Course' : '📊 Update Progress')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CourseCompletionPage;
