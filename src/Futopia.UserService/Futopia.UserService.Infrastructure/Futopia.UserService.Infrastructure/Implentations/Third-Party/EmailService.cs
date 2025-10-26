using Futopia.UserService.Application.Abstractions.Third_Party;
using Futopia.UserService.Application.DTOs.Email;
using Futopia.UserService.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
namespace Futopia.UserService.Infrastructure.Implentations.Third_Party;
public class EmailService : IEmailService
{
    private readonly EmailServiceOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailServiceOptions> options,
        ILogger<EmailService> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateOptions();
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        return await SendEmailAsync(to, subject, body, null, null, isHtml);
    }

    public async Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        string[]? cc = null,
        string[]? bcc = null,
        bool isHtml = true)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email cannot be null or empty", nameof(to));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or empty", nameof(subject));

        try
        {
            using var mailMessage = CreateMailMessage(to, subject, body, isHtml);

            if (cc != null && cc.Length > 0)
            {
                foreach (var ccEmail in cc.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    mailMessage.CC.Add(ccEmail);
                }
            }

            if (bcc != null && bcc.Length > 0)
            {
                foreach (var bccEmail in bcc.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    mailMessage.Bcc.Add(bccEmail);
                }
            }

            await SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendEmailWithAttachmentsAsync(
        string to,
        string subject,
        string body,
        IEnumerable<EmailAttachment> attachments,
        bool isHtml = true)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email cannot be null or empty", nameof(to));

        if (attachments == null || !attachments.Any())
            throw new ArgumentException("Attachments cannot be null or empty", nameof(attachments));

        try
        {
            using var mailMessage = CreateMailMessage(to, subject, body, isHtml);

            foreach (var attachment in attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
                mailMessage.Attachments.Add(mailAttachment);
            }

            await SendMailAsync(mailMessage);

            _logger.LogInformation("Email with {Count} attachments sent successfully to {To}",
                attachments.Count(), to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with attachments to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendBulkEmailAsync(
        IEnumerable<string> recipients,
        string subject,
        string body,
        bool isHtml = true)
    {
        if (recipients == null || !recipients.Any())
            throw new ArgumentException("Recipients cannot be null or empty", nameof(recipients));

        var successCount = 0;
        var failureCount = 0;

        foreach (var recipient in recipients.Where(r => !string.IsNullOrWhiteSpace(r)))
        {
            var result = await SendEmailAsync(recipient, subject, body, isHtml);
            if (result)
                successCount++;
            else
                failureCount++;

            // Small delay to avoid overwhelming SMTP server
            await Task.Delay(100);
        }

        _logger.LogInformation("Bulk email completed: {Success} succeeded, {Failed} failed",
            successCount, failureCount);

        return failureCount == 0;
    }

    private MailMessage CreateMailMessage(string to, string subject, string body, bool isHtml)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        mailMessage.To.Add(to);
        return mailMessage;
    }

    private async Task SendMailAsync(MailMessage mailMessage)
    {
        using var smtpClient = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password),
            Timeout = _options.TimeoutSeconds * 1000
        };

        await smtpClient.SendMailAsync(mailMessage);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
            throw new InvalidOperationException("EmailServiceOptions.SmtpHost cannot be null or empty");

        if (_options.SmtpPort <= 0)
            throw new InvalidOperationException("EmailServiceOptions.SmtpPort must be greater than 0");

        if (string.IsNullOrWhiteSpace(_options.Username))
            throw new InvalidOperationException("EmailServiceOptions.Username cannot be null or empty");

        if (string.IsNullOrWhiteSpace(_options.Password))
            throw new InvalidOperationException("EmailServiceOptions.Password cannot be null or empty");

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
            throw new InvalidOperationException("EmailServiceOptions.FromEmail cannot be null or empty");

        if (string.IsNullOrWhiteSpace(_options.FromName))
            throw new InvalidOperationException("EmailServiceOptions.FromName cannot be null or empty");
    }
}
