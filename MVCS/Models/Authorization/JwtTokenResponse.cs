namespace BaseEFAPI.MVCS.Models.Authorization;

    public sealed class JwtTokenResponse : BaseResponseModel
    {
        public string? Token { get; set; }
    }
