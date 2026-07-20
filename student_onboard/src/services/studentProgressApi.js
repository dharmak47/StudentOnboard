import { studentApi } from './api';

export const studentProgressApi = {
  /**
   * Get detailed progress for a specific course enrollment
   */
  getProgress(registrationId) {
    return studentApi.get(`/api/Student/progress/${registrationId}`);
  },

  /**
   * Update progress for a course
   */
  updateProgress(registrationId, payload) {
    return studentApi.put(`/api/Student/progress/${registrationId}`, payload);
  },

  /**
   * Get progress summary for the current student
   */
  getProgressSummary() {
    return studentApi.get('/api/Student/progress/summary');
  }
};
