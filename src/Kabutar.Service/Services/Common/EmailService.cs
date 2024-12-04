using Kabutar.Service.DTOs.Common;
using Kabutar.Service.Interfaces.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;

namespace Kabutar.Service.Services.Common
{
    public class EmailService : IEmailService
    {
        private readonly IConfigurationSection _config;

        public EmailService(IConfiguration configuration)
        {
            _config = configuration.GetSection("Email");
        }

        public async Task SendAsync(EmailMessage message)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["EmailAddress"]));
            email.To.Add(MailboxAddress.Parse(message.To));
            email.Subject = message.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: 'Arial', sans-serif;
            background-color: #f7f7f7;
            margin: 0;
            padding: 0;
            color: #333;
        }}
        .email-container {{
            max-width: 600px;
            margin: 40px auto;
            background: #ffffff;
            border: 1px solid #dddddd;
            overflow: hidden;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }}
        .email-header {{
            background-color: #007bff;
            color: #ffffff;
            padding: 20px;
            text-align: center;
            border-radius: 7px 7px 0 0;
        }}
        .email-body {{
            padding: 20px;
            line-height: 1.5;
        }}
        .email-body p {{
            margin: 20px 0;
        }}
        .otp-code {{
            font-size: 20px;
            font-weight: bold;
            display: inline-block;
            margin: 10px 0;
            padding: 10px 15px;
            border-radius: 5px;
            background-color: #e9ecef;
            border: 1px solid #cccccc;
            color: #007bff;
        }}
        .footer {{
            font-size: 12px;
            text-align: center;
            color: #aaaaaa;
            padding: 20px;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>Your OTP Code</h1>
        </div>
        <div class='email-body'>
            <p>Hello,</p>
            <p>Your One-Time Password (OTP) for accessing your account is:</p>
            <p class='otp-code'>{message.Body}</p>
            <p>Please enter this code on the website to proceed. Remember, this code is valid for only 15 minutes.</p>
        </div>
        <div class='footer'>
            <p>If you did not request this code, please ignore this email or contact support.</p>
        </div>
    </div>
</body>
</html>" };

            using (var smtp = new SmtpClient())
            {
                try
                {
                    await smtp.ConnectAsync(_config["Host"], 587, SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(_config["EmailAddress"], _config["Password"]);
                    await smtp.SendAsync(email);
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    throw new InvalidOperationException("Failed to send email.", ex);
                }
                finally
                {
                    await smtp.DisconnectAsync(true);
                }
            }
        }
    }
}
