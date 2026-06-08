using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.AuthModels;

namespace PRN232.LMS.Services.Business;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
    private readonly IConfiguration _configuration;

    public AuthService(
        IGenericRepository<User> userRepo,
        IGenericRepository<RefreshToken> refreshTokenRepo,
        IConfiguration configuration)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _configuration = configuration;
    }

    public async Task<AuthResponseData?> LoginAsync(LoginRequest request)
    {
        var users = await _userRepo.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase));

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        return await GenerateAuthTokensAsync(user);
    }

    public async Task<AuthResponseData?> RefreshTokenAsync(string refreshToken)
    {
        // Find the token
        var tokens = await _refreshTokenRepo.GetAllAsync();
        var tokenEntity = tokens.FirstOrDefault(t => t.Token == refreshToken);

        if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiryDate <= DateTime.UtcNow)
        {
            return null;
        }

        // Fetch user details
        var user = await _userRepo.GetByIdAsync(tokenEntity.UserId);
        if (user == null)
        {
            return null;
        }

        // Delete old token
        await _refreshTokenRepo.DeleteAsync(tokenEntity.TokenId);

        // Generate new token pair
        return await GenerateAuthTokensAsync(user);
    }

    private async Task<AuthResponseData> GenerateAuthTokensAsync(User user)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? "AntigravitySuperSecretDefaultKey1234567890!!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "LmsServer";
        var audience = _configuration["JwtSettings:Audience"] ?? "LmsClient";
        var expiryMinutesStr = _configuration["JwtSettings:ExpiryMinutes"] ?? "60";
        
        if (!int.TryParse(expiryMinutesStr, out var expiryMinutes))
        {
            expiryMinutes = 60;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: tokenExpiry,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Generate Refresh Token
        var refreshTokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(refreshTokenBytes);
        }
        var refreshToken = Convert.ToBase64String(refreshTokenBytes);

        // Save Refresh Token to database
        var newRefreshToken = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.UserId,
            ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh Token lasts for 7 days
            IsRevoked = false,
            CreatedDate = DateTime.UtcNow
        };

        await _refreshTokenRepo.AddAsync(newRefreshToken);

        return new AuthResponseData
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiryMinutes * 60
        };
    }
}
