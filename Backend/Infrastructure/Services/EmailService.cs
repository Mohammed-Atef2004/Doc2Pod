using Domain.Interfaces.Services;
using Domain.Settings;
using Domain.SharedKernel;
using Domain.Users.Errors;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services
{
    public sealed class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailOptions, ILogger<EmailService> logger)
        {
            _emailSettings = emailOptions.Value;
            _logger = logger;
        }

        public async Task<Result> SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(to))
                return Result.Failure(EmailErrors.Empty);

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Doc2Pod Support", _emailSettings.Email));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls, ct);
                await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password, ct);
                await smtp.SendAsync(message, ct);
                await smtp.DisconnectAsync(true, ct);

                _logger.LogInformation("Email sent successfully to {Email}", to);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}: {Message}", to, ex.Message);
                return Result.Failure(new Error("Email.SendFailed", ex.Message));
            }
        }

        public async Task<Result> SendConfirmationEmailAsync(string email, string name, string confirmationLink, CancellationToken ct = default)
        {
            var body = GetConfirmationHtml(name, confirmationLink);
            return await SendEmailAsync(email, "Confirm Your Email - Doc2Pod", body, ct);
        }

        public async Task<Result> SendPasswordResetEmailAsync(string email, string name, string token, CancellationToken ct = default)
        {
            var body = GetResetPasswordHtml(name, token);
            return await SendEmailAsync(email, "Reset Password - Doc2Pod", body, ct);
        }

        public async Task<Result> SendEmailChangeWarningAsync(string email, string name, CancellationToken ct = default)
        {
            var body = GetEmailChangeWarningHtml(name);
            return await SendEmailAsync(email, "Security Alert: Email Change Requested - Doc2Pod", body, ct);
        }

        public async Task<Result> SendEmailChangeConfirmationAsync(string email, string name, string link, CancellationToken ct = default)
        {
            var body = GetEmailChangeConfirmationHtml(name, link);
            return await SendEmailAsync(email, "Confirm Your New Email Address - Doc2Pod", body, ct);
        }

        public async Task<Result> SendActivationReminderEmailAsync(string email, string name, string confirmationLink, CancellationToken ct = default)
        {
            var body = GetActivationReminderHtml(name, confirmationLink);
            return await SendEmailAsync(email, "Activate Your Account - Doc2Pod", body, ct);
        }

        private string GetEmailLayout(string content) => $@"
        <div dir='ltr' style='background-color: #0a0a0a; padding: 40px 20px; font-family: ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
            <div style='max-width: 600px; margin: 0 auto; background-color: #121212; border: 1px solid rgba(238, 220, 168, 0.2); border-radius: 16px; overflow: hidden; box-shadow: 0 25px 50px rgba(0,0,0,0.5);'>
        
                <div style='background: linear-gradient(135deg, #eedca8 0%, #d4af37 100%); padding: 35px 30px; text-align: center;'>
                    <h1 style='color: #1a1a1a; margin: 0; font-size: 28px; font-weight: 800; letter-spacing: 2px; text-transform: uppercase;'>Doc2Pod</h1>
                    <p style='color: #1a1a1a; margin: 5px 0 0 0; font-size: 13px; font-weight: 600; opacity: 0.8;'>AI-Powered Educational Podcasts</p>
                </div>

                <div style='padding: 40px 35px; color: #ffffff;'>
                    {content}
                </div>

                <div style='background-color: #0f0f0f; padding: 25px; text-align: center; font-size: 12px; color: #666; border-top: 1px solid rgba(238, 220, 168, 0.1);'>
                    <p style='margin: 0;'>This is an automated security notification from Doc2Pod AI System.</p>
                    <p style='margin: 5px 0 0 0; color: #eedca8; opacity: 0.7;'>© {DateTime.Now.Year} Doc2Pod Team. Cairo, Egypt.</p>
                </div>
            </div>
        </div>";


        private string GetConfirmationHtml(string name, string link) => GetEmailLayout($@"
        <h2 style='color: #eedca8; margin-top: 0; font-size: 24px;'>Welcome to the Future, {name}!</h2>
        <p style='color: #ccc;'>We're thrilled to have you join <b>Doc2Pod</b>. Your journey of turning PDFs into natural Egyptian Arabic podcasts starts here.</p>
        <p style='color: #ccc;'>Please verify your email to activate your account:</p>
    
        <div style='text-align: center; margin: 35px 0;'>
            <a href='{link}' style='background: linear-gradient(135deg, #eedca8 0%, #d4af37 100%); color: #1a1a1a; padding: 14px 35px; text-decoration: none; border-radius: 8px; font-weight: 700; display: inline-block; font-size: 14px; text-transform: uppercase; letter-spacing: 1px; box-shadow: 0 4px 15px rgba(212, 175, 55, 0.2);'>
                Verify My Account
            </a>
        </div>
    
        <p style='font-size: 12px; color: #666; text-align: center;'>If the button doesn't work, copy this link:<br/>
        <span style='color: #eedca8; opacity: 0.8; word-break: break-all;'>{link}</span></p>");


        private string GetActivationReminderHtml(string name, string link) => GetEmailLayout($@"
        <h2 style='color: #eedca8; margin-top: 0;'>Account Activation Required</h2>
        <p style='color: #ccc;'>Hi {name}, it looks like you're trying to access your account, but your email hasn't been verified yet.</p>
    
        <div style='text-align: center; margin: 35px 0;'>
            <a href='{link}' style='background: linear-gradient(135deg, #eedca8 0%, #d4af37 100%); color: #1a1a1a; padding: 14px 35px; text-decoration: none; border-radius: 8px; font-weight: 700; display: inline-block; font-size: 14px; text-transform: uppercase;'>
                Activate My Account
            </a>
        </div>
        <p style='color: #888; font-size: 13px;'>Once verified, you can proceed with your request normally.</p>");


        private string GetResetPasswordHtml(string name, string link) => GetEmailLayout($@"
        <h2 style='color: #eedca8; margin-top: 0;'>Reset Your Password</h2>
        <p style='color: #ccc;'>Hi {name}, we received a request to reset your password. No worries!</p>
    
        <div style='text-align: center; margin: 35px 0;'>
            <a href='{link}' style='background: linear-gradient(135deg, #eedca8 0%, #d4af37 100%); color: #1a1a1a; padding: 14px 35px; text-decoration: none; border-radius: 8px; font-weight: 700; display: inline-block; font-size: 14px; text-transform: uppercase;'>
                Reset Password
            </a>
        </div>
        <p style='color: #ff4d4d; font-size: 12px; opacity: 0.8;'>If you didn't request this, please ignore this email.</p>");


        private string GetEmailChangeConfirmationHtml(string name, string link) => GetEmailLayout($@"
        <h2 style='color: #eedca8; margin-top: 0;'>Confirm Your New Email</h2>
        <p style='color: #ccc;'>Hi {name},</p>
        <p style='color: #ccc;'>You're almost there! Please click the button below to confirm your new email address and complete the update:</p>
    
        <div style='text-align: center; margin: 35px 0;'>
            <a href='{link}' style='background: linear-gradient(135deg, #eedca8 0%, #d4af37 100%); color: #1a1a1a; padding: 14px 35px; text-decoration: none; border-radius: 8px; font-weight: 700; display: inline-block; font-size: 14px; text-transform: uppercase; letter-spacing: 1px; box-shadow: 0 4px 15px rgba(212, 175, 55, 0.2);'>
                Confirm New Email
            </a>
        </div>
        <p style='color: #888; font-size: 12px;'>If you didn't request this change, please contact our support team.</p>");

        private string GetEmailChangeWarningHtml(string name) => GetEmailLayout($@"
        <h2 style='color: #800020; margin-top: 0; font-weight: 800;'>Security Alert!</h2>
        <p style='color: #ccc;'>Hi {name},</p>
        <p style='color: #ccc;'>A request was recently made to change the email address associated with your <b>Doc2Pod</b> account.</p>
    
        <div style='background: rgba(128, 0, 32, 0.1); border-left: 4px solid #800020; padding: 20px; border-radius: 8px; margin: 25px 0;'>
            <b style='color: #800020; display: block; margin-bottom: 8px; font-size: 16px;'>⚠️ If this was NOT you:</b> 
            <span style='color: #bbb; font-size: 14px; line-height: 1.5;'>Please secure your account by changing your password immediately and contacting our support team to freeze any suspicious activity.</span>
        </div>
    
        <p style='color: #666; font-size: 13px;'>If this was you, you can safely ignore this security warning.</p>");
    }
}