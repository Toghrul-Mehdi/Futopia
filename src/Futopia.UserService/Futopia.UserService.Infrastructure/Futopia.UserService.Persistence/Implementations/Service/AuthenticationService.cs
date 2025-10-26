using AutoMapper;
using Futopia.UserService.Application.Abstractions.Service;
using Futopia.UserService.Application.Abstractions.Third_Party;
using Futopia.UserService.Application.DTOs.Auth;
using Futopia.UserService.Application.ResponceObject;
using Futopia.UserService.Application.ResponceObject.Enums;
using Futopia.UserService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
namespace Futopia.UserService.Persistence.Implementations.Service;
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailService _emailService;
    public AuthenticationService(UserManager<AppUser> userManager,RoleManager<IdentityRole> roleManager,IEmailService emailService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
    }
    public async Task<Response> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return new Response(ResponseStatusCode.Error, "Invalid email or password.");
        }
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            return new Response(ResponseStatusCode.Error, "Invalid email or password.");
        }        
        return new Response(ResponseStatusCode.Success, "Login successful.");
    }

    public async Task<Response> RegisterAsync(RegisterDto registerDto)
    {
        if (!registerDto.AcceptTerms)
            return new Response(ResponseStatusCode.Error, "You must accept the terms.");

        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
        {
            return new Response(ResponseStatusCode.Error, "Email already exists.");
        }

        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return new Response(ResponseStatusCode.Error, "Passwords do not match.");
        }

        var user = new AppUser
        {
            FullName = $"{registerDto.Name} {registerDto.Surname}",
            UserName = registerDto.Email,
            Email = registerDto.Email,
            NormalizedUserName = registerDto.Email.ToUpper(),
            NormalizedEmail = registerDto.Email.ToUpper()
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new Response(ResponseStatusCode.Error, $"User creation failed: {errors}");
        }

        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        await _userManager.AddToRoleAsync(user, "User");

        await _emailService.SendEmailAsync(user.Email, "Welcome to Futopia", "Thank you for registering!",true);

        return new Response(ResponseStatusCode.Success, "User registered successfully.");
    }



}
