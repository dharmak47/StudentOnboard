import { adminApi } from './api';

/**
 * API service for course completion management
 * Handles marking courses as complete and retrieving incomplete registrations
 */

/**
 * Mark a course registration as complete
 * @param {Object} completionData - Completion details
 * @param {string} completionData.registrationId - Registration ID to mark complete
 * @param {number} completionData.grade - Optional grade/score (0-100)
 * @param {string} completionData.adminNotes - Optional admin notes
 * @param {string} completionData.completionDate - Optional completion date (ISO format)
 * @param {number} completionData.completionPercentage - Optional completion percentage (0-100)
 * @param {boolean} completionData.markAsComplete - Whether to set isCompleted flag to true
 * @returns {Promise} API response with completion details
 */
export const markCourseComplete = async (completionData) => {
  try {
    const response = await adminApi.post('/api/Admin/course-completion/mark-complete', {
      registrationId: completionData.registrationId,
      grade: completionData.grade || null,
      adminNotes: completionData.adminNotes || null,
      completionDate: completionData.completionDate || null,
      completionPercentage: completionData.completionPercentage || null,
      markAsComplete: completionData.markAsComplete || false
    });
    return response;
  } catch (error) {
    console.error('Error marking course complete:', error);
    throw error;
  }
};

/**
 * Get incomplete course registrations for a specific course
 * @param {string} courseId - Course ID to fetch registrations for
 * @param {number} page - Page number (default: 1)
 * @param {number} pageSize - Page size (default: 20)
 * @returns {Promise} API response with list of incomplete registrations
 */
export const getIncompleteRegistrations = async (courseId, page = 1, pageSize = 20) => {
  try {
    const response = await adminApi.get('/api/Admin/course-completion/incomplete?courseId=' + courseId + '&page=' + page + '&pageSize=' + pageSize);
    return response;
  } catch (error) {
    console.error('Error fetching incomplete registrations:', error);
    throw error;
  }
};
