using Kabutar.Service.DTOs.Common;
using Kabutar.Service.Interfaces.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace Kabutar.Service.Services.Common;

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
        email.From.Add(MailboxAddress.Parse(_config["EmailAddress"]!));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;
        email.Body = new TextPart(TextFormat.Html)
        {
            Text = GenerateBeautifulHtml(message.Body)
        };

        using var smtp = new SmtpClient();
        try
        {
            int port = int.Parse(_config["Port"] ?? "587");
            await smtp.ConnectAsync(_config["Host"], port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["EmailAddress"], _config["Password"]);
            await smtp.SendAsync(email);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Email yuborishda xatolik yuz berdi.", ex);
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }

    private string GenerateBeautifulHtml(string code)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Kabutar Verification Code</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f5f5f5;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background-color: #007bff;
            color: white;
            padding: 20px;
            border-radius: 10px 10px 0 0;
            text-align: center;
        }}
        .code {{
            font-size: 24px;
            font-weight: bold;
            background: #e9ecef;
            padding: 12px 20px;
            border-radius: 6px;
            display: inline-block;
            margin: 20px 0;
            color: #007bff;
            letter-spacing: 2px;
        }}
        .footer {{
            text-align: center;
            font-size: 12px;
            color: #999;
            margin-top: 30px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Kabutar OTP Code</h2>
        </div>
        <p>Salom,</p>
        <p>Quyida sizning tasdiqlash kodlaringiz:</p>
        <p class='code'>{code}</p>
        <p>Bu kod 15 daqiqa amal qiladi. Kodni hech kimga bermang.</p>
        <div class='footer'>
            <p>Agar bu so‘rov siz tomonidan yuborilmagan bo‘lsa, iltimos, e’tiborsiz qoldiring.</p>
        </div>
    </div>
</body>
</html>";
    }
}
