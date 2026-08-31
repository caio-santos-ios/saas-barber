using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace api_barber.Services
{
    public class MailHandler(IConfiguration config)
    {
        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? config["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? config["Smtp:Port"] ?? "587");
            var username = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? config["Smtp:Username"] ?? "";
            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? config["Smtp:Password"] ?? "";
            var fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? config["Smtp:FromName"] ?? "Na Régua";
            var fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? config["Smtp:FromEmail"] ?? username;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine($"[SMTP WARNING] SMTP_USERNAME ou SMTP_PASSWORD não configurados no .env. E-mail para {toEmail} não foi enviado.");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
