using Futopia.UserService.Application.Abstractions.Service;
using Futopia.UserService.Application.Abstractions.Third_Party;
using Futopia.UserService.Application.DTOs.Auth;
using Futopia.UserService.Application.Options;
using Futopia.UserService.Application.ResponceObject.Enums;
using Futopia.UserService.Domain.Entities;
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
    public AuthenticationService(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailService emailService,
        ITokenService tokenService,
        IOptions<TokenServiceOptions> tokenServiceOptions,
        IMemoryCache cache,
        IFirebaseSmsService firebaseSmsService) 
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
        _tokenService = tokenService;
        _tokenServiceOptions = tokenServiceOptions.Value;
        _cache = cache;
        _firebaseSmsService = firebaseSmsService;
    }
    public async Task<Response> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
            return new Response(ResponseStatusCode.Error, "Invalid email or password.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
            return new Response(ResponseStatusCode.Error, "Invalid email or password.");

        if(!user.EmailConfirmed)
            return new Response(ResponseStatusCode.Error, "Email is not confirmed.");

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

        var responseData = new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
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
        if (!registerDto.AcceptTerms)
            return new Response(ResponseStatusCode.Error, "You must accept the terms.");

        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            return new Response(ResponseStatusCode.Error, "Email already exists.");

        if (registerDto.Password != registerDto.ConfirmPassword)
            return new Response(ResponseStatusCode.Error, "Passwords do not match.");

        var user = new AppUser
        {
            FullName = $"{registerDto.Name} {registerDto.Surname}",
            UserName = registerDto.Email,
            Email = registerDto.Email,
            NormalizedUserName = registerDto.Email.ToUpper(),
            NormalizedEmail = registerDto.Email.ToUpper(),
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new Response(ResponseStatusCode.Error, $"User creation failed: {errors}");
        }

        if (!await _roleManager.RoleExistsAsync("User"))
            await _roleManager.CreateAsync(new IdentityRole("User"));

        await _userManager.AddToRoleAsync(user, "User");

        var verificationCode = new Random().Next(100000, 999999).ToString();

        _cache.Set($"email_verify_{user.Email}", verificationCode, TimeSpan.FromMinutes(10));

        var emailBody = $@"
        <h3>Welcome to Futopia!</h3>
        <p>Use the following code to verify your email address:</p>
        <h2>{verificationCode}</h2>
        <p>This code expires in 10 minutes.</p>
        ";

        await _emailService.SendEmailAsync(user.Email, "Verify your Futopia account", emailBody, true);

        return new Response(ResponseStatusCode.Success, "Verification code sent to your email.");
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
    public async Task<Response> SendOtpCodeAsync(string phoneNumber)
    {        
        var response = await _firebaseSmsService.SendOtpCodeAsync(phoneNumber);
        if (response.ResponseStatusCode==ResponseStatusCode.Success)
        {
            return new Response(ResponseStatusCode.Success, "OTP code sent successfully.");
        }
        else
        {
            return new Response(ResponseStatusCode.Error, "Failed to send OTP code.");
        }
    }
}
