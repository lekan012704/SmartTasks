using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection; 
using SmartTask.Application.Interfaces;
using System.Text;

public static class EmailJobService
{
    public static async Task SendPasswordResetEmail(
        string email,
        string tokenEncoded,
        string baseUrl,
    [FromServices] IMailService mailService)
    {
        // 1. Construct the complete reset link
        var emailEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(email));
        var resetLink = $"{baseUrl}/reset-password?token={tokenEncoded}&email={emailEncoded}";

        // 2. Construct the email body
        var emailBody = $"<p>You requested a password reset. Please use the following link to reset your password:</p><p><a href='{resetLink}'>Reset Password</a></p>";

        // 3. Send the email 
        await mailService.SendAsync(email, "SmartSeller Password Reset", emailBody);
    }
}