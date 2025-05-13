using System;
using System.Threading.Tasks;
using API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace API.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["Email:SmtpServer"];
            _smtpPort = int.Parse(configuration["Email:SmtpPort"]);
            _smtpUsername = configuration["Email:Username"];
            _smtpPassword = configuration["Email:Password"];
            _fromEmail = configuration["Email:FromEmail"];
            _fromName = configuration["Email:FromName"];
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_fromEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_smtpUsername, _smtpPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}