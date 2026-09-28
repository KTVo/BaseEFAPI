using BaseEFAPI.MVCS.Models.SignIn;

namespace BaseEFAPI.MVCS.Services.SignIn.Implementations;

public sealed class SignInService : ISignInService
{
    public async Task<SignInResponseModel> SignInUserAsync(SignInRequestModel model)
    {
        // NULL CHECKS
        if (model == null) { throw new ArgumentNullException("SignInRequestModel is null."); }
        if (string.IsNullOrEmpty(model.Email)) { throw new ArgumentNullException("Email is null!"); }
        if (string.IsNullOrEmpty(model.Password)) { throw new ArgumentNullException("Password is null!"); }

        
    }
}
