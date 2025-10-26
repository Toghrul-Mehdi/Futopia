using Futopia.UserService.Application.Abstractions.Third_Party;
using Futopia.UserService.Application.DTOs.Email;
using Futopia.UserService.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using Futopia.UserService.Application.ResponceObject.Enums;
using Futopia.UserService.Application.ResponceObject;

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

    public async Task<Response> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        => await SendEmailAsync(to, subject, body, null, null, isHtml);

    public async Task<Response> SendEmailAsync(
        string to,
        string subject,
        string body,
        string[]? cc = null,
        string[]? bcc = null,
        bool isHtml = true)
    {
        if (string.IsNullOrWhiteSpace(to))
            return new Response(ResponseStatusCode.Error, "Recipient email cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(subject))
            return new Response(ResponseStatusCode.Error, "Subject cannot be null or empty.");

        try
        {
            using var mailMessage = CreateMailMessage(to, subject, body, isHtml);

            // Optional CC
            if (cc != null)
                foreach (var ccEmail in cc.Where(e => !string.IsNullOrWhiteSpace(e)))
                    mailMessage.CC.Add(ccEmail);

            // Optional BCC
            if (bcc != null)
                foreach (var bccEmail in bcc.Where(e => !string.IsNullOrWhiteSpace(e)))
                    mailMessage.Bcc.Add(bccEmail);

            _logger.LogInformation("Attempting to send email to {Recipient}", to);
            await SendMailAsync(mailMessage);

            _logger.LogInformation("✅ Email sent successfully to {Recipient}", to);
            return new Response(ResponseStatusCode.Success, "Email sent successfully.");
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP error while sending email to {Recipient}. Host: {Host}, Port: {Port}",
                to, _options.SmtpHost, _options.SmtpPort);

            return new Response(ResponseStatusCode.Error, $"SMTP error: {smtpEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while sending email to {Recipient}", to);
            return new Response(ResponseStatusCode.Error, $"Failed to send email: {ex.Message}");
        }
    }

    public async Task<Response> SendEmailWithAttachmentsAsync(
        string to,
        string subject,
        string body,
        IEnumerable<EmailAttachmentDto> attachments,
        bool isHtml = true)
    {
        if (string.IsNullOrWhiteSpace(to))
            return new Response(ResponseStatusCode.Error, "Recipient email cannot be null or empty.");

        if (attachments == null || !attachments.Any())
            return new Response(ResponseStatusCode.Error, "Attachments cannot be null or empty.");

        try
        {
            using var mailMessage = CreateMailMessage(to, subject, body, isHtml);

            foreach (var attachment in attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
                mailMessage.Attachments.Add(mailAttachment);
            }

            _logger.LogInformation("Attempting to send email with {Count} attachments to {Recipient}",
                attachments.Count(), to);

            await SendMailAsync(mailMessage);

            _logger.LogInformation("✅ Email with attachments sent successfully to {Recipient}", to);
            return new Response(ResponseStatusCode.Success, "Email with attachments sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with attachments to {Recipient}", to);
            return new Response(ResponseStatusCode.Error, $"Failed to send email with attachments: {ex.Message}");
        }
    }

    public async Task<Response> SendBulkEmailAsync(
        IEnumerable<string> recipients,
        string subject,
        string body,
        bool isHtml = true)
    {
        if (recipients == null || !recipients.Any())
            return new Response(ResponseStatusCode.Error, "Recipients cannot be null or empty.");

        var successCount = 0;
        var failureCount = 0;

        _logger.LogInformation("🚀 Starting bulk email send. Total recipients: {Count}", recipients.Count());

        foreach (var recipient in recipients.Where(r => !string.IsNullOrWhiteSpace(r)))
        {
            var result = await SendEmailAsync(recipient, subject, body, isHtml);
            if (result.ResponseStatusCode == ResponseStatusCode.Success)
                successCount++;
            else
                failureCount++;

            await Task.Delay(100);
        }

        _logger.LogInformation("📊 Bulk email summary: {Success} succeeded, {Failed} failed",
            successCount, failureCount);

        if (failureCount == 0)
            return new Response(ResponseStatusCode.Success, $"All {successCount} emails sent successfully.");

        if (successCount == 0)
            return new Response(ResponseStatusCode.Error, $"All {failureCount} emails failed to send.");

        return new Response(ResponseStatusCode.Warning,
            $"Bulk email partially completed: {successCount} succeeded, {failureCount} failed.");
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
