using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace api_barber.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
    }

    public class EmailService(IConfiguration config) : IEmailService
    {
        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var host = config["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(config["Smtp:Port"] ?? "587");
            var username = config["Smtp:Username"] ?? "";
            var password = config["Smtp:Password"] ?? "";
            var fromName = config["Smtp:FromName"] ?? "SaaS Barbearia";
            var fromEmail = config["Smtp:FromEmail"] ?? username;

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
