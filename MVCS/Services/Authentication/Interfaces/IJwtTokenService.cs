using BaseEFAPI.MVCS.Models.Authorization;

namespace BaseEFAPI.MVCS.Services.Authentication.Interfaces;

public interface IJwtTokenService
{
    Task<JwtTokenResponse> GenerateJwtTokenAsync(JwtTokenModel jwtToken);
    DecryptedJweTokenResponse DecryptJWEToken(string tokenString);
}
