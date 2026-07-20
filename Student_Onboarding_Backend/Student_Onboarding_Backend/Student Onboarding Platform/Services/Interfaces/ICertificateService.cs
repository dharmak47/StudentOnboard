namespace Student_Onboarding_Platform.Services.Interfaces;

public interface ICertificateService
{
    byte[] GenerateCertificate(string studentName, string courseName, DateTime completionDate, string verificationId);
}
