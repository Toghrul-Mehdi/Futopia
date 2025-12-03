namespace Futopia.UserService.Application.Abstractions.Third_Party;
public interface IFirebaseSmsService
{
    Task<Response> VerifyOtpCodeAsync(string phoneNumber, string otpCode, string verificationId);
    Task<Response> SendOtpCodeAsync(string phoneNumber);
}