using Microsoft.Extensions.Options;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Entities;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class MailService : IMailService
{
    private readonly MailSettings _mailSettings;

    public MailService(IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        var message = new MailMessage();
        message.From = new MailAddress(_mailSettings.SenderEmail, _mailSettings.SenderName);
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true; 

        using (var smtp = new SmtpClient(_mailSettings.SmtpHost, _mailSettings.SmtpPort))
        {
            smtp.Credentials = new NetworkCredential(_mailSettings.Username, _mailSettings.Password);
            smtp.EnableSsl = _mailSettings.EnableSsl;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

            await smtp.SendMailAsync(message);
        }
    }
}