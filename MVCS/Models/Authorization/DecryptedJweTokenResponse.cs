namespace BaseEFAPI.MVCS.Models.Authorization;
public sealed class DecryptedJweTokenResponse : BaseResponseModel
{
    public List<DecryptedJweClaim> Claims { get; set; } = new List<DecryptedJweClaim>();
}
