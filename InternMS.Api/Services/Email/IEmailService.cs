using System;
using InternMS.Api.Services.Email;

namespace InternMS.Api.Services.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}