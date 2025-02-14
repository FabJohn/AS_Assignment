using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");

        var smtpClient = new SmtpClient
        {
            Host = emailSettings["SmtpServer"],
            Port = int.Parse(emailSettings["SmtpPort"]),
            EnableSsl = bool.Parse(emailSettings["EnableSsl"]),
            Credentials = new NetworkCredential(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"])
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(emailSettings["FromEmail"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}