namespace Student_Onboarding_Platform.Models.DTOs.Admin;

public class AdminDashboardResponse
{
    public int TotalStudents { get; set; }
    public int PendingApprovals { get; set; }
    public int ApprovedStudents { get; set; }
    public int DeniedStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalRegistrations { get; set; }
}
