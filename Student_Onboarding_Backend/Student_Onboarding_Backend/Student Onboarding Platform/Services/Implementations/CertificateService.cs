using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class CertificateService : ICertificateService
{
    public CertificateService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateCertificate(string studentName, string courseName, DateTime completionDate, string verificationId)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(16).FontFamily(Fonts.Arial));

                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nutech_logo.png");
                
                page.Content().Border(10).BorderColor("#1E3A8A").Padding(2, Unit.Centimetre).Column(x =>
                {
                    // Header Area
                    x.Item().Row(row => 
                    {
                        if (File.Exists(logoPath))
                        {
                            row.AutoItem().Column(col =>
                            {
                                col.Item().Height(60).Image(logoPath);
                                col.Item().PaddingTop(4).Text("Nu Tech Computer Education")
                                    .FontSize(13).Bold().FontColor("#1E3A8A");
                            });
                        }
                        else 
                        {
                            row.AutoItem().Column(col =>
                            {
                                col.Item().Text("Nu Tech Computer Education").FontSize(18).Bold().FontColor("#1E3A8A");
                            });
                        }
                        
                        row.RelativeItem().AlignRight().Text($"ID: {verificationId}").FontSize(10).FontColor(Colors.Grey.Medium);
                    });

                    x.Item().PaddingTop(0.5f, Unit.Centimetre).AlignCenter().Text("CERTIFICATE OF COMPLETION")
                        .FontSize(40).SemiBold().FontColor("#0F172A");

                    x.Item().PaddingTop(0.5f, Unit.Centimetre).AlignCenter().Text("This is to proudly certify that")
                        .FontSize(18).Italic().FontColor(Colors.Grey.Darken2);

                    x.Item().PaddingTop(0.5f, Unit.Centimetre).AlignCenter().Text(studentName)
                        .FontSize(36).Bold().FontColor("#3B82F6");

                    x.Item().PaddingTop(0.5f, Unit.Centimetre).AlignCenter().Text("has successfully completed the comprehensive program in")
                        .FontSize(18).Italic().FontColor(Colors.Grey.Darken2);

                    x.Item().PaddingTop(0.5f, Unit.Centimetre).AlignCenter().Text(courseName)
                        .FontSize(24).SemiBold().FontColor("#0F172A");

                    x.Item().PaddingTop(1, Unit.Centimetre).AlignCenter().Text($"Awarded on {completionDate:MMMM dd, yyyy}")
                        .FontSize(16).FontColor(Colors.Grey.Darken3);
                        
                    x.Item().PaddingTop(1, Unit.Centimetre).Row(row => 
                    {
                        row.RelativeItem().AlignLeft().Column(col => 
                        {
                            col.Item().PaddingBottom(5).Text("Dharmarajan K").FontSize(18).Italic().FontColor("#1E3A8A");
                            col.Item().Text("_______________________").FontColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(5).Text("Center Head").FontSize(12).FontColor(Colors.Grey.Darken2);
                        });
                        row.RelativeItem().AlignRight().Column(col => 
                        {
                            col.Item().PaddingBottom(5).Text("Malini V").FontSize(18).Italic().FontColor("#1E3A8A");
                            col.Item().Text("_______________________").FontColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(5).Text("Authorized Signatory").FontSize(12).FontColor(Colors.Grey.Darken2);
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
