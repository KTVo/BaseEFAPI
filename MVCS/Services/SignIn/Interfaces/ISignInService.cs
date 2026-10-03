using BaseEFAPI.MVCS.Models.SignIn;

namespace BaseEFAPI.MVCS.Services.SignIn.Implementations;

public interface ISignInService
{
    Task<SignInResponseModel> SignInUserAsync(SignInRequestModel model);
}
