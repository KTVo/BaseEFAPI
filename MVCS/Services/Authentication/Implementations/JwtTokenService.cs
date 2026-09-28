using BaseEFAPI.MVCS.Models.Authorization;
using BaseEFAPI.MVCS.Services.Authentication.Interfaces;

namespace BaseEFAPI.MVCS.Services.Authentication.Implementations;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    // PRIVATE CLASS VARIABLES
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    
    public async Task<string> GenerateJwtTokenAsync(string userId, string userName, string userEmail, string userType)
    {
        // NULL CHECKS
        if (string.IsNullOrEmpty(userId)) { throw new ArgumentNullException("UserId is null or empty!"); }
        if (string.IsNullOrEmpty(userName)) { throw new ArgumentNullException("UserName is null or empty!"); }
        if (string.IsNullOrEmpty(userEmail)) { throw new ArgumentNullException("UserEmail is null or empty!"); }
        if (string.IsNullOrEmpty(userType)) { throw new ArgumentNullException("UserType is null or empty!"); }

        /*
        Issuer
        Subject
        ExpireOn
        CreatedOn
        EncryptedSecretKey == jti
        
        
        */

        // GET JWT TOKEN CONFIGURATION FROM APPSETTINGS.JSON
        JwtTokenSettingsModel jwtSettings = new JwtTokenSettingsModel
        {
            SecretKey = _configuration["Authorization:JwtSettings:SecretKey"] ?? throw new ArgumentNullException("SecretKey is null");
            Issuer = _configuration["Authorization:JwtSettings:Issuer"] ?? throw new ArgumentNullException("Issuer is null or empty!"),
            Audience = _configuration["Authorization:JwtSettings:Audience"] ?? throw new ArgumentNullException("Audience is null or empty!"),
            ExpiryInMinutes = int.Parse(_configuration["Authorization:JwtSettings:ExpiryInMinutes"] ?? "60")
        };

        // GENERATE JWT TOKEN USING CONFIGURATION AND USER INFORMATION
        JwtTokenModel jwtToken = new JwtTokenModel
        {
            Issuer = jwtSettings.Issuer,
            Subject = userId,
            ExpireOn = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryInMinutes),
            CreatedOn = DateTime.UtcNow,
            EncryptedSecretKey = jwtSettings.SecretKey,
            JTI = Guid.NewGuid().ToString()
        };
        return jwtToken
    }
}
