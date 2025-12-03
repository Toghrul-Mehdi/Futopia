using Futopia.UserService.Application.Abstractions.Third_Party;
using Futopia.UserService.Application.ResponceObject.Enums;
using Microsoft.Extensions.Logging;

namespace Futopia.UserService.Persistence.Implementations.Third_Party;

// IEmailService implementasiyasının yanında yerləşə bilər
public class FirebaseSmsService : IFirebaseSmsService
{
    private readonly ILogger<FirebaseSmsService> _logger;

    public FirebaseSmsService(ILogger<FirebaseSmsService> logger)
    {
        _logger = logger;        
    }

    /// <summary>
    /// Bu metod telefon doğrulamasını başlatmaq üçün Backend-in sadəcə uğurlu cavab qaytarmasını təmin edir.
    /// Qeyd: Real OTP göndərilməsi Frontend (Web/Mobile) tərəfindən idarə olunmalıdır!
    /// </summary>
    public async Task<Response> SendOtpCodeAsync(string phoneNumber)
    {       

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return new Response(ResponseStatusCode.Error, "Telefon nömrəsi daxil edilməyib.");
        }                

        _logger.LogInformation($"Firebase OTP göndərilməsi üçün sorğu alındı: {phoneNumber}");       

        return new Response(ResponseStatusCode.Success, "OTP göndərilməsi üçün hazırıq. Zəhmət olmasa, tətbiqdəki adımları izləyin.");
    }

    
   

    public Task<Response> VerifyOtpCodeAsync(string phoneNumber, string otpCode, string verificationId)
    {
        throw new NotImplementedException();
    }
}