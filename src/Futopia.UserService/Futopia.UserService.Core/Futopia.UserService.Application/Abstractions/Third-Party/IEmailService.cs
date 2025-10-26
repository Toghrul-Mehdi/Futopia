using Futopia.UserService.Application.DTOs.Email;
using Futopia.UserService.Application.ResponceObject;
using System.Threading.Tasks;

namespace Futopia.UserService.Application.Abstractions.Third_Party;
public interface IEmailService
{
    Task<Response> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<Response> SendEmailAsync(string to, string subject, string body, string[]? cc = null, string[]? bcc = null, bool isHtml = true);
    Task<Response> SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<EmailAttachmentDto> attachments, bool isHtml = true);
    Task<Response> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = true);
}
