using Futopia.UserService.Application.Abstractions.Third_Party;
using Futopia.UserService.Application.Options;
using Futopia.UserService.Application.ResponceObject.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RestSharp;
namespace Futopia.UserService.Infrastructure.Services;
public class FirebaseSmsService : IFirebaseSmsService
{
    private readonly InfobipOptions _settings;
    private readonly IMemoryCache _cache; // ⬅️ Keş interfeysi

    // 💡 Constructor yeniləndi
    public FirebaseSmsService(IOptions<InfobipOptions> options, IMemoryCache cache)
    {
        _settings = options.Value;
        _cache = cache;
    }

    public async Task<Response> SendOtpCodeAsync(string phoneNumber)
    {
        // 1. OTP kodu
        var otpCode = new Random().Next(100000, 999999).ToString();
        var messageText = $"Futopia təsdiqləmə kodu: {otpCode}. Kodu heç kimlə paylaşmayın.";

        // 🔑 Keş Açarı (Cache Key) müəyyənləşdirilməsi
        // Telefon nömrəsi hər istifadəçi üçün unikal açar rolunu oynayır.
        var cacheKey = $"phone_otp_{phoneNumber}";
        var expirationTime = TimeSpan.FromMinutes(5); // Kodu 5 dəqiqə keşdə saxlayırıq.

        // 🧠 OTP kodunu keş yaddaşına yazmaq
        _cache.Set(cacheKey, otpCode, expirationTime);

        // 2. RestSharp Client ayarları
        var options = new RestClientOptions(_settings.BaseUrl)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        var client = new RestClient(options);
        var request = new RestRequest("/sms/2/text/advanced", Method.Post);

        request.AddHeader("Authorization", $"App {_settings.ApiKey}");
        request.AddHeader("Accept", "application/json");

        // GÖVDƏ
        var body = new
        {
            messages = new[]
            {
                new
                {
                    destinations = new[]
                    {
                        new { to = phoneNumber }
                    },
                    from = _settings.SenderId,
                    text = messageText
                }
            }
        };

        request.AddJsonBody(body);

        try
        {
            var response = await client.ExecutePostAsync(request);

            if (response.IsSuccessful)
            {
                return new Response(
                    ResponseStatusCode.Success,
                    $"OTP kodu {phoneNumber} nömrəsinə uğurla göndərildi və {expirationTime.TotalMinutes} dəqiqəliyə keşdə saxlandı."
                );
            }

            // SMS göndərişi uğursuz olarsa, keşdəki kodu silmək daha yaxşı olar (optional)
            _cache.Remove(cacheKey);

            return new Response(
                ResponseStatusCode.Error,
                $"SMS göndərilərkən xəta baş verdi. Status: {response.StatusCode}. Cavab: {response.Content}"
            );
        }
        catch (Exception ex)
        {
            // Xəta baş verərsə, keşdəki kodu silmək daha yaxşı olar (optional)
            _cache.Remove(cacheKey);

            return new Response(
                ResponseStatusCode.Error,
                $"SMS göndərilərkən istisna baş verdi: {ex.Message}"
            );
        }
    }

    // 💡 Bu metod indi OTP kodunu keşdən yoxlayacaq
    public Task<Response> VerifyOtpCodeAsync(string phoneNumber, string otpCode)
    {
        var cacheKey = $"phone_otp_{phoneNumber}";

        if (!_cache.TryGetValue(cacheKey, out string storedOtpCode))
        {
            return Task.FromResult(new Response(ResponseStatusCode.Error, "Təsdiqləmə kodu tapılmadı və ya vaxtı bitib."));
        }

        if (storedOtpCode == otpCode)
        {
            // Uğurlu təsdiqdən sonra kodu keşdən silirik
            _cache.Remove(cacheKey);
            return Task.FromResult(new Response(ResponseStatusCode.Success, "Telefon nömrəsi uğurla təsdiqləndi."));
        }
        else
        {
            return Task.FromResult(new Response(ResponseStatusCode.Error, "Yanlış təsdiqləmə kodu."));
        }
    }
}