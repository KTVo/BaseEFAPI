using BaseEFAPI.MVCS.Models.Authorization;
using BaseEFAPI.MVCS.Models.SignIn;
using BaseEFAPI.MVCS.Services.Authentication.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BaseEFAPI.MVCS.Services.SignIn.Implementations;

public sealed class SignInService(IJwtTokenService jwtTokenService, IUserRepository userRepository) : ISignInService
{
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IPasswordHasher<ApplicationUserModel> _passwordHasher = new PasswordHasher<ApplicationUserModel>();

    public async Task<SignInResponseModel> SignInUserAsync(SignInRequestModel model)
    {
        // NULL CHECKS
        if (model == null) { throw new ArgumentNullException("SignInRequestModel is null."); }
        if (string.IsNullOrEmpty(model.Email)) { throw new ArgumentNullException("Email is null!"); }
        if (string.IsNullOrEmpty(model.UserName)) { throw new ArgumentNullException("Username is null!"); }
        if (string.IsNullOrEmpty(model.Password)) { throw new ArgumentNullException("Password is null!"); }

        // GET USER FROM DATABASE
        ApplicationUserResponse user = await _userRepository.GetUserByEmailAsync(model.Email);

        if (user != null && user.IsSuccess == true && user.User == null)
        {
            user = await _userRepository.GetUserByUsernameAsync(model.UserName);
        }

        // CHECK IF USER EXISTS AND RETURN FAILURE RESPONSE IF NOT
        if (user == null) { return new() { IsSuccess = false, Message = ExternalMessages.EmailIsNotFound }; }
        if (user.IsSuccess == false) { return new() { IsSuccess = false, Message = user.Message }; }
        if (user.User == null) { return new() { IsSuccess = false, Message = ExternalMessages.EmailIsNotFound }; }
        if (string.IsNullOrEmpty(user.User.PasswordHash)) { return new() { IsSuccess = false, Message = ExternalMessages.PasswordIsInvalid }; }

        // CHECK IF PASSWORD IS VALID AND RETURN FAILURE RESPONSE IF NOT
        PasswordVerificationResult passwordValidationResult;
        try
        {
            passwordValidationResult = _passwordHasher.VerifyHashedPassword(user.User, user.User.PasswordHash, model.Password);
        }
        catch (FormatException)
        {
            // Invalid stored hashes cannot authenticate a user.
            return new() { IsSuccess = false, Message = ExternalMessages.PasswordIsInvalid };
        }

        if (passwordValidationResult == PasswordVerificationResult.Failed) { return new() { IsSuccess = false, Message = ExternalMessages.PasswordIsInvalid }; }

        JwtTokenResponse jwtTokenResponse = await _jwtTokenService.GenerateJwtTokenAsync(new JwtTokenModel
        {
            Subject = Guid.NewGuid().ToString(),
            UserName = user.User.UserName ?? user.User.Email,
            Email = user.User.Email,
            UserType = user.User.UserType
        });

        return new SignInResponseModel
        {
            IsSuccess = jwtTokenResponse.IsSuccess,
            Token = jwtTokenResponse.Token,
            Message = jwtTokenResponse.Message
        };
    }
}
