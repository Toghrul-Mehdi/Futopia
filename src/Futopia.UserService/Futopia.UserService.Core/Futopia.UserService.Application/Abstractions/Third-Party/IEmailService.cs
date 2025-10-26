using Futopia.UserService.Application.DTOs.Email;
using System.Threading.Tasks;

namespace Futopia.UserService.Application.Abstractions.Third_Party;
public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<bool> SendEmailAsync(string to, string subject, string body, string[]? cc = null, string[]? bcc = null, bool isHtml = true);
    Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<EmailAttachment> attachments, bool isHtml = true);
    Task<bool> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = true);
}
