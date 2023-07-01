using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ApplicationCore.Entities.User;
using ApplicationCore.Interfaces;
using Infrastructure.Exceptions;
using Infrastructure.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class IdentityTokenClaimService : ITokenClaimsService
{
    private readonly IConfiguration _configuration;
    private readonly AppIdentityDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityTokenClaimService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        AppIdentityDbContext context
    )
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<string> GetTokenAsync(string userName)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JWTSecretKey"]!);
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            throw new UserNotFoundException($"User with username {userName} not found!");
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim> { new(ClaimTypes.Name, userName) };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims.ToArray()),
            Expires = DateTime.UtcNow.AddHours(5),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public Task<string> GetUserNameFromTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWTSecretKey"]!);
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };
        tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
        var jwtToken = (JwtSecurityToken)validatedToken;
        var userName = jwtToken.Claims.First(x => x.Type == "unique_name").Value;
        return Task.FromResult(userName);
    }

    public async Task<string> GenerateRefreshTokenAsync(string userName)
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);
        var user = await _userManager.FindByNameAsync(userName);
        _context.RefreshTokens.Add(
            new RefreshToken
            {
                Token = refreshToken,
                ApplicationUser = user,
                ValidUntil = DateTime.UtcNow.AddDays(2)
            }
        );
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<bool> CheckRefreshTokenAsync(string? userName, string refreshToken)
    {
        var user = await _userManager.FindByNameAsync(userName);
        var token = _context.RefreshTokens
            .AsEnumerable()
            .Where(x => x.ApplicationUser.Equals(user) && x.Token.Equals(refreshToken))
            .DefaultIfEmpty(null)
            .FirstOrDefault();
        return token != null;
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string expiredToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["JWTSecretKey"]!)
            ),
            ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(
            expiredToken,
            tokenValidationParameters,
            out var securityToken
        );
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (
            jwtSecurityToken == null
            || !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase
            )
        )
            throw new SecurityTokenException("Invalid token");
        return principal;
    }
}
