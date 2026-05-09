namespace ResipWeb.Areas.Admin.Repository;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody);
}
