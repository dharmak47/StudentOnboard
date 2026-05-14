using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Student_Onboarding_Platform.Models.Settings;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> settings, IHttpClientFactory httpClientFactory, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose)
    {
        var subject = $"Your Verification Code - {_settings.FromName}";
        var body = $@"
            <h2>Verification Code</h2>
            <p>Your OTP for <strong>{purpose}</strong> is:</p>
            <h1 style='color: #4CAF50; letter-spacing: 5px;'>{otpCode}</h1>
            <p>This code expires in 5 minutes. Do not share it with anyone.</p>
            <p>If you did not request this, please ignore this email.</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string otpCode)
    {
        var subject = $"Password Reset Request - {_settings.FromName}";
        var body = $@"
            <h2>Password Reset</h2>
            <p>You have requested to reset your password. Use the following code:</p>
            <h1 style='color: #FF5722; letter-spacing: 5px;'>{otpCode}</h1>
            <p>This code expires in 5 minutes. Do not share it with anyone.</p>
            <p>If you did not request this, please secure your account immediately.</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string firstName)
    {
        var subject = $"Welcome to {_settings.FromName}!";
        var body = $@"
            <h2>Welcome, {firstName}!</h2>
            <p>Your account has been successfully verified.</p>
            <p>You can now log in and start your onboarding journey.</p>
            <p>Best regards,<br/>{_settings.FromName} Team</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPendingApprovalEmailAsync(string toEmail, string firstName)
    {
        var subject = $"Email Verified - Awaiting Approval - {_settings.FromName}";
        var body = $@"
            <h2>Email Verified Successfully, {firstName}!</h2>
            <p>Your email has been verified. Your account is now pending admin approval.</p>
            <p>You will receive a notification once your account has been reviewed by our team.</p>
            <p>Thank you for your patience.</p>
            <p>Best regards,<br/>{_settings.FromName} Team</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendApprovalEmailAsync(string toEmail, string firstName)
    {
        var subject = $"Account Approved - Welcome to {_settings.FromName}!";
        var body = $@"
            <h2>Congratulations, {firstName}!</h2>
            <p>Your account has been approved by our admin team.</p>
            <p>You can now log in and start your onboarding journey.</p>
            <p>Best regards,<br/>{_settings.FromName} Team</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendDenialEmailAsync(string toEmail, string firstName, string? reason)
    {
        var subject = $"Account Registration Update - {_settings.FromName}";
        var reasonText = string.IsNullOrEmpty(reason)
            ? "No specific reason was provided."
            : reason;

        var body = $@"
            <h2>Registration Update, {firstName}</h2>
            <p>We regret to inform you that your account registration has been denied.</p>
            <p><strong>Reason:</strong> {reasonText}</p>
            <p>If you believe this is an error, please contact our support team.</p>
            <p>Best regards,<br/>{_settings.FromName} Team</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendCourseRegistrationEmailAsync(string toEmail, string firstName, string courseName)
    {
        var subject = $"Course Registration Confirmed - {_settings.FromName}";
        var body = $@"
            <h2>Course Registration Successful, {firstName}!</h2>
            <p>You have been registered for the following course:</p>
            <h3 style='color: #2196F3;'>{courseName}</h3>
            <p>Please check your dashboard for payment details and course information.</p>
            <p>Best regards,<br/>{_settings.FromName} Team</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        // Use SendGrid HTTP API when hosted on SendGrid (Render blocks SMTP port 587)
        if (_settings.Host.Contains("sendgrid", StringComparison.OrdinalIgnoreCase))
        {
            await SendViaSendGridApiAsync(to, subject, htmlBody);
            return;
        }

        // Fall back to SMTP for local dev (e.g., Gmail)
        await SendViaSmtpAsync(to, subject, htmlBody);
    }

    private async Task SendViaSendGridApiAsync(string to, string subject, string htmlBody)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Password);

        var payload = new
        {
            personalizations = new[] { new { to = new[] { new { email = to } } } },
            from = new { email = _settings.FromEmail, name = _settings.FromName },
            subject,
            content = new[] { new { type = "text/html", value = htmlBody } }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("https://api.sendgrid.com/v3/mail/send", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent via SendGrid API to {Email} with subject '{Subject}'", to, subject);
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("SendGrid API error {StatusCode} for {Email}: {Response}", response.StatusCode, to, responseBody);
                throw new Exception($"SendGrid API returned {response.StatusCode}: {responseBody}");
            }
        }
        catch (Exception ex) when (ex is not Exception { Message: var m } || !m.StartsWith("SendGrid API returned"))
        {
            _logger.LogError(ex, "Failed to send email via SendGrid API to {Email}", to);
            throw;
        }
    }

    private async Task SendViaSmtpAsync(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            var secureOption = _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(_settings.Host, _settings.Port, secureOption);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            _logger.LogInformation("Email sent via SMTP to {Email} with subject '{Subject}'", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via {Host}:{Port} with subject '{Subject}'",
                to, _settings.Host, _settings.Port, subject);
            throw;
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true);
        }
    }
}
