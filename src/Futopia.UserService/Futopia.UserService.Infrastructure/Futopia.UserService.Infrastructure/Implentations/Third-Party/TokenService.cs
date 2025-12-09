using Futopia.UserService.Application.Abstractions.Third_Party;
using Futopia.UserService.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace Futopia.UserService.Infrastructure.Implentations.Third_Party;
public class TokenService : ITokenService
{
    private readonly TokenServiceOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    public TokenService(IOptions<TokenServiceOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _tokenHandler = new JwtSecurityTokenHandler();

        ValidateOptions();
    }
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        if (claims == null || !claims.Any())
            throw new ArgumentException("Claims cannot be null or empty", nameof(claims));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            ValidateLifetime = false, // We're validating an expired token
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(false);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            _tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InvalidOperationException("TokenServiceOptions.SecretKey cannot be null or empty");

        if (_options.SecretKey.Length < 32)
            throw new InvalidOperationException("TokenServiceOptions.SecretKey must be at least 32 characters");

        if (string.IsNullOrWhiteSpace(_options.Issuer))
            throw new InvalidOperationException("TokenServiceOptions.Issuer cannot be null or empty");

        if (string.IsNullOrWhiteSpace(_options.Audience))
            throw new InvalidOperationException("TokenServiceOptions.Audience cannot be null or empty");

        if (_options.AccessTokenExpirationMinutes <= 0)
            throw new InvalidOperationException("TokenServiceOptions.AccessTokenExpirationMinutes must be greater than 0");

        if (_options.RefreshTokenExpirationDays <= 0)
            throw new InvalidOperationException("TokenServiceOptions.RefreshTokenExpirationDays must be greater than 0");
    }
}
