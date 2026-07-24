import { adminApi } from './api';

export const adminAnalyticsApi = {
  /**
   * Get monthly analytics data for a date range
   */
  getMonthlyAnalytics(startMonth, endMonth) {
    const params = new URLSearchParams();
    if (startMonth) params.append('startMonth', startMonth);
    if (endMonth) params.append('endMonth', endMonth);

    const queryString = params.toString();
    const endpoint = `/api/admin/analytics/monthly${queryString ? '?' + queryString : ''}`;
    return adminApi.get(endpoint);
  },

  /**
   * Get student progress analytics with pagination
   */
  getStudentProgress(courseId, page = 1, pageSize = 20) {
    const params = new URLSearchParams();
    if (courseId) params.append('courseId', courseId);
    params.append('page', page);
    params.append('pageSize', pageSize);

    const queryString = params.toString();
    const endpoint = `/api/admin/analytics/student-progress${queryString ? '?' + queryString : ''}`;
    return adminApi.get(endpoint);
  },

  /**
   * Generate monthly report for a specific month
   */
  generateMonthlyReport(yearMonth) {
    const params = new URLSearchParams();
    if (yearMonth) params.append('yearMonth', yearMonth);

    const queryString = params.toString();
    const endpoint = `/api/admin/analytics/generate-report${queryString ? '?' + queryString : ''}`;
    return adminApi.post(endpoint, {});
  },

  /**
   * Regenerate all analytics data
   */
  regenerateAllAnalytics() {
    return adminApi.post('/api/admin/analytics/regenerate-all', {});
  }
};
