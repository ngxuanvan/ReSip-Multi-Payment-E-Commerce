using MailKit.Security;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using ResipWeb.Models;


namespace ResipWeb.Areas.Admin.Repository;

public class EmailSender : IEmailSender
{
    private readonly EmailOptions _opt;
    public EmailSender(IOptions<EmailOptions> opt) => _opt = opt.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_opt.FromName, _opt.From));
        msg.To.Add(MailboxAddress.Parse(toEmail));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_opt.User, _opt.Password);
        await smtp.SendAsync(msg);
        await smtp.DisconnectAsync(true);
    }
}
