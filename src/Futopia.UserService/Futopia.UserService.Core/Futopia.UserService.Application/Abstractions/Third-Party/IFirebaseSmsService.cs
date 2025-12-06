namespace Futopia.UserService.Application.Abstractions.Third_Party;
public interface IFirebaseSmsService
{
    Task<Response> SendOtpCodeAsync(string phoneNumber);
}