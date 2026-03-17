using System.Net;
using System.Net.Mail;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Exceptions;
using Microsoft.Extensions.Options;

namespace CupidLearn.Infrastructure.Email;

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(string toEmail, string subject, string bodyText, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) ||
            string.IsNullOrWhiteSpace(_options.User) ||
            string.IsNullOrWhiteSpace(_options.Pass) ||
            string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            throw new ServiceUnavailableException("Email service is not configured");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = bodyText,
            IsBodyHtml = false,
        };

        message.To.Add(new MailAddress(toEmail));

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.User, _options.Pass),
        };

        ct.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, ct);
    }
}
