namespace BaseEFAPI.MVCS.Models.SignIn;

public sealed class SignInResponseModel : BaseResponseModel
{
    public ApplicationUserModel? User { get; set; }
    public string? Token { get; set; }
}
