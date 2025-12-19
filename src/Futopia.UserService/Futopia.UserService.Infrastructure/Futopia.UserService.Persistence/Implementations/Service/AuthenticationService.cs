using Futopia.UserService.Application.Abstractions.Repository;
using Futopia.UserService.Application.Abstractions.Service;
using Futopia.UserService.Application.Abstractions.Third_Party;
using Futopia.UserService.Application.DTOs.Auth;
using Futopia.UserService.Application.Options;
using Futopia.UserService.Application.ResponceObject.Enums;
using Futopia.UserService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Claims;
namespace Futopia.UserService.Persistence.Implementations.Service;
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly TokenServiceOptions _tokenServiceOptions;
    private readonly IMemoryCache _cache;
    private readonly IFirebaseSmsService _firebaseSmsService;
    private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailService emailService,
        ITokenService tokenService,
        IOptions<TokenServiceOptions> tokenServiceOptions,
        IMemoryCache cache,
        IFirebaseSmsService firebaseSmsService,
        IUserRefreshTokenRepository userRefreshTokenRepository,
        IHttpContextAccessor httpContextAccessor
)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
        _tokenService = tokenService;
        _tokenServiceOptions = tokenServiceOptions.Value;
        _cache = cache;
        _firebaseSmsService = firebaseSmsService;
        _userRefreshTokenRepository = userRefreshTokenRepository;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<Response> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
            return new Response(ResponseStatusCode.Error, "Invalid email or password.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
            return new Response(ResponseStatusCode.Error, "Invalid email or password.");

        /*if(!user.EmailConfirmed)
            return new Response(ResponseStatusCode.Error, "Email is not confirmed.");*/

        // Claims hazırlayırıq
        var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.UserName ?? "")
    };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            userClaims.Add(new Claim(ClaimTypes.Role, role));
        }
        var accessToken = _tokenService.GenerateAccessToken(userClaims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Refresh token-i bazaya yazırıq

        var userrefreshtoken = new UserRefreshToken
        {
            RefreshToken = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _userRefreshTokenRepository.AddAsync(userrefreshtoken);
        await _userRefreshTokenRepository.SaveChangesAsync();

        var cookieoptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieoptions);
        

        var responseData = new
        {
            AccessToken = accessToken,            
            ExpiresIn = _tokenServiceOptions.AccessTokenExpirationMinutes * 60 // saniyə olaraq
        };

        return new Response(ResponseStatusCode.Success, "Login successful.")
        {
            Data = responseData
        };
    }

    //Register
    public async Task<Response> RegisterAsync(RegisterDto registerDto)
    {
        // 1. Validasiyalar
        if (!registerDto.AcceptTerms)
            return new Response(ResponseStatusCode.Error, "You must accept the terms.");

        // E-poçt yoxlanışı
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            return new Response(ResponseStatusCode.Error, "Email already exists.");        

        if (registerDto.Password != registerDto.ConfirmPassword)
            return new Response(ResponseStatusCode.Error, "Passwords do not match.");

        // 2. İstifadəçi Obyektinin Yaradılması
        var user = new AppUser
        {
            FullName = $"{registerDto.Name} {registerDto.Surname}",
            UserName = registerDto.Email, // Login üçün email istifadə olunur
            Email = registerDto.Email,
            PhoneNumber = registerDto.Phone, // ⬅️ Telefon nömrəsi mənimsədildi
            NormalizedUserName = registerDto.Email.ToUpper(),
            NormalizedEmail = registerDto.Email.ToUpper(),
            EmailConfirmed = false,
            PhoneNumberConfirmed = false // ⬅️ Hələ təsdiqlənməyib
        };

        // 3. Bazaya Yazılması
        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new Response(ResponseStatusCode.Error, $"User creation failed: {errors}");
        }

        // 4. Rolun Təyin Edilməsi
        if (!await _roleManager.RoleExistsAsync("User"))
            await _roleManager.CreateAsync(new IdentityRole("User"));

        await _userManager.AddToRoleAsync(user, "User");        
        var smsResponse = await _firebaseSmsService.SendOtpCodeAsync(user.PhoneNumber);

        if (smsResponse.ResponseStatusCode != ResponseStatusCode.Success)
        {            
            return new Response(ResponseStatusCode.Error, $"User created, but SMS failed: {smsResponse.Message}");
        }

        return new Response(ResponseStatusCode.Success, "Registration successful. OTP code sent to your phone.");
    }

  
    public async Task<Response> VerifyMobileAsync(VerifyNumberDto verifyDto)
    {      

        var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == verifyDto.PhoneNumber);

        if (user == null)
            return new Response(ResponseStatusCode.Error, "User with this phone number not found.");

        if (user.PhoneNumberConfirmed)
            return new Response(ResponseStatusCode.Success, "Phone number is already confirmed.");

        var verifyResponse = await _firebaseSmsService.VerifyOtpCodeAsync(verifyDto.PhoneNumber, verifyDto.OtpCode);

        if (verifyResponse.ResponseStatusCode != ResponseStatusCode.Success)
        {
            return verifyResponse;
        }

        user.PhoneNumberConfirmed = true;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return new Response(ResponseStatusCode.Error, "Database update failed.");

        return new Response(ResponseStatusCode.Success, "Phone number verified successfully.");
    }
    //Email dogrualama
    public async Task<Response> ConfirmEmailAsync(string email, string code)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new Response(ResponseStatusCode.Error, "User not found.");

        if (user.EmailConfirmed)
            return new Response(ResponseStatusCode.Error, "Email already confirmed.");

        if (!_cache.TryGetValue($"email_verify_{email}", out string? cachedCode))
            return new Response(ResponseStatusCode.Error, "Verification code expired or not found.");

        if (cachedCode != code)
            return new Response(ResponseStatusCode.Error, "Invalid verification code.");

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        _cache.Remove($"email_verify_{email}");

        return new Response(ResponseStatusCode.Success, "Email confirmed successfully.");
    }

    public async Task<Response> RefreshTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return new Response(ResponseStatusCode.Error, "Invalid request.");

        if (!httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return new Response(ResponseStatusCode.Error, "Refresh token not found.");

        var storedToken = await _userRefreshTokenRepository.GetByTokenAsync(refreshToken);
        if (storedToken == null)
            return new Response(ResponseStatusCode.Error, "Invalid refresh token.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return new Response(ResponseStatusCode.Error, "Refresh token expired.");

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user == null)
            return new Response(ResponseStatusCode.Error, "User not found.");

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.UserName ?? "")
    };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var newAccessToken = _tokenService.GenerateAccessToken(claims);

        return new Response(ResponseStatusCode.Success, "Token refreshed successfully.")
        {
            Data = new
            {
                AccessToken = newAccessToken,
                ExpiresIn = _tokenServiceOptions.AccessTokenExpirationMinutes * 60
            }
        };
    }


    public async Task<Response> LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return new Response(ResponseStatusCode.Error, "Invalid request.");

        if (httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            var storedToken = await _userRefreshTokenRepository.GetByTokenAsync(refreshToken);
            if (storedToken != null)
            {
                await _userRefreshTokenRepository.DeleteAsync(storedToken);
                await _userRefreshTokenRepository.SaveChangesAsync();
            }

            httpContext.Response.Cookies.Delete("refreshToken");
        }

        return new Response(ResponseStatusCode.Success, "Logout successfully.");
    }

}
