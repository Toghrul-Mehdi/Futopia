using FluentValidation;
using FluentValidation.AspNetCore;
using Futopia.UserService.Application.DTOs.Auth;
using Futopia.UserService.Domain.Entities;
using Futopia.UserService.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Futopia.UserService.Persistence;
public static class ServiceRegistration
{
    public static void AddSQLServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext əlavə olunur
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("FutopiaUserServiceDatabase")));

        // Identity servisləri əlavə olunur
        services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    }
    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining(typeof(RegisterDto));
        return services;
    }
}
