using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Entities;
using SmartTask.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Shared.Services
{
    public class EmailService :IEmailService
    {
        private readonly MailSettings _email;
        public EmailService(IOptions<MailSettings> email)
        {
            _email = email.Value;
        }
        public async Task SendEmailAsync(string to, string subject,string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_email.SenderEmail, _email.SenderName),
                Body = body,
                Subject = subject
            };
            message.To.Add(to);

            using var client = new SmtpClient(_email.SmtpHost, _email.SmtpPort)
            {
                Credentials = new NetworkCredential(_email.Username, _email.Password),
                EnableSsl = _email.EnableSsl
            };
            await client.SendMailAsync(message);
        }
    }
}
