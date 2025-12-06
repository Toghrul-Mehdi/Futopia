using Futopia.UserService.Application.Abstractions.Third_Party;
using Futopia.UserService.Application.Options;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Futopia.UserService.Infrastructure.Services;

public class FirebaseSmsService : IFirebaseSmsService
{
    private readonly InfobipOptions _settings;

    public FirebaseSmsService(IOptions<InfobipOptions> options)
    {
        _settings = options.Value;
    }

    public async Task<Response> SendOtpCodeAsync(string phoneNumber)
    {
        // 1. OTP kodu
        var otpCode = new Random().Next(100000, 999999).ToString();
        var messageText = $"Futopia təsdiqləmə kodu: {otpCode}. Kodu heç kimlə paylaşmayın.";

        // 2. RestSharp Client ayarları
        var options = new RestClientOptions(_settings.BaseUrl)
        {
            Timeout = TimeSpan.FromSeconds(30) // daha təhlükəsiz
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
                from = _settings.SenderId, // !!! BURADA DÜZGÜN SENDER ID OLDUĞUNDAN ƏMİN OL
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
                    Application.ResponceObject.Enums.ResponseStatusCode.Success,
                    $"OTP kodu {phoneNumber} nömrəsinə göndərildi. Cavab: {response.Content}"
                );
            }

            return new Response(
                Application.ResponceObject.Enums.ResponseStatusCode.Error,
                $"SMS göndərilərkən xəta baş verdi. Status: {response.StatusCode}. Cavab: {response.Content}"
            );
        }
        catch (Exception ex)
        {
            return new Response(
                Application.ResponceObject.Enums.ResponseStatusCode.Error,
                $"SMS göndərilərkən istisna baş verdi: {ex.Message}"
            );
        }
    }

}
