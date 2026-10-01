using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BaseEFAPI.MVCS.Models.Authorization;
using BaseEFAPI.MVCS.Services.Authentication.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace BaseEFAPI.MVCS.Services.Authentication.Implementations;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    // PRIVATE CLASS VARIABLES
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

   

    public async Task<JwtTokenResponse> GenerateJwtTokenAsync(JwtTokenModel jwtToken)
    {
        string? userId = jwtToken.Subject;
        string? userName = jwtToken.UserName;
        string? userEmail = jwtToken.Email;
        string? userType = jwtToken.UserType;

        // NULL CHECKS
        if (string.IsNullOrEmpty(userId) == true) { return new() {IsSuccess = false, Message = ExternalMessages.UserNameIsNull}; }
        if (string.IsNullOrEmpty(userName) == true) { return new() {IsSuccess = false, Message = ExternalMessages.UserNameIsNull}; }
        if (string.IsNullOrEmpty(userEmail) == true) { return new() {IsSuccess = false, Message = ExternalMessages.EmailIsNull}; }
        if (string.IsNullOrEmpty(userType) == true) { return new() {IsSuccess = false, Message = ExternalMessages.UserTypeIsNull}; }

        if (string.IsNullOrEmpty(_configuration["Authentication:JwtSettings:SecretKey"]) == true) { throw new ArgumentNullException(ExternalMessages.SecretKeyIsNull); }
        if (string.IsNullOrEmpty(_configuration["Authentication:JwtSettings:Issuer"]) == true) { throw new ArgumentNullException(ExternalMessages.IssuerIsNull); }
        if (string.IsNullOrEmpty(_configuration["Authentication:JwtSettings:Audience"]) == true) { throw new ArgumentNullException(ExternalMessages.AudienceIsNull); }


        // GET JWT TOKEN CONFIGURATION FROM APPSETTINGS.JSON
        JwtTokenSettingsModel jwtSettings = new JwtTokenSettingsModel
        {
            SecretKey = _configuration["Authentication:JwtSettings:SecretKey"],
            Issuer = _configuration["Authentication:JwtSettings:Issuer"],
            Audience = _configuration["Authentication:JwtSettings:Audience"],
            ExpiryInMinutes = int.Parse(_configuration["Authentication:JwtSettings:ExpiryInMinutes"] ?? "60")
        };

        List<Claim> claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, userEmail),
            new(JwtRegisteredClaimNames.Typ, userType),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

#pragma warning disable CS8604 // Possible null reference argument.
        SymmetricSecurityKey key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
#pragma warning restore CS8604 // Possible null reference argument.

        SigningCredentials credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryInMinutes),
            signingCredentials: credentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new JwtTokenResponse { Token = tokenString, IsSuccess = true, Message = "JWT token generated successfully." };
    }
}
