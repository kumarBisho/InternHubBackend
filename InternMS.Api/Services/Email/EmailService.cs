using System;
using System.Net;
using System.Net.Mail;
using InternMS.Api.Services.Email;

namespace InternMS.Api.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _env;

        public EmailService(ILogger<EmailService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
                var smtpPortStr = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587";
                var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
                var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");

                _logger.LogInformation($"[EMAIL] Attempting to send email to: {toEmail}");
                _logger.LogInformation($"[EMAIL] Subject: {subject}");
                _logger.LogInformation($"[EMAIL] SMTP Config - Host: {smtpHost}, User: {smtpUser}");

                // If SMTP is not configured, log the email instead (development mode)
                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    if (!_env.IsDevelopment())
                    {
                        throw new InvalidOperationException("SMTP is not configured. Set SMTP_HOST, SMTP_PORT, SMTP_USER, and SMTP_PASS in the hosting environment.");
                    }

                    _logger.LogWarning("[EMAIL] SMTP not configured. Running in development mode.");
                    _logger.LogInformation($"[EMAIL_DEV_MODE] To: {toEmail}");
                    _logger.LogInformation($"[EMAIL_DEV_MODE] Subject: {subject}");
                    _logger.LogInformation($"[EMAIL_DEV_MODE] Body: {body}");
                    await Task.CompletedTask;
                    return;
                }

                // Production: Send actual email
                if (!int.TryParse(smtpPortStr, out int smtpPort))
                {
                    smtpPort = 587;
                }

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpUser),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"[EMAIL] Successfully sent email to: {toEmail}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[EMAIL_ERROR] Failed to send email: {ex.Message}");
                _logger.LogError($"[EMAIL_ERROR] Stack trace: {ex.StackTrace}");
                throw new Exception($"Email service error: {ex.Message}", ex);
            }
        }
    }
}