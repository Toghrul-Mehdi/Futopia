using Futopia.UserService.Application.Abstractions.Third_Party;
using Futopia.UserService.Application.Options;
using Futopia.UserService.Infrastructure.Implentations.Third_Party;
using Futopia.UserService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Futopia.UserService.Infrastructure;
public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TokenServiceOptions>(configuration.GetSection("TokenServiceOptions"));
        services.AddScoped<ITokenService, TokenService>();
        services.Configure<EmailServiceOptions>(configuration.GetSection("EmailServiceOptions"));
        services.AddScoped<IEmailService, EmailService>();
        services.Configure<InfobipOptions>(configuration.GetSection("Infobip"));
        services.AddScoped<IFirebaseSmsService, FirebaseSmsService>();
        return services;
    }
}