using BaseEFAPI.MVCS.Services.Context;
using BaseEFAPI.MVCS.Services.Registration.Interfaces;

public sealed class RegistrationService : IRegistrationService
{
    private readonly IUserRepository _userRepository;

    public RegistrationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        ValidateServices();
    } 

    private bool ValidateServices()
    {
        if (_userRepository == null) { throw new ArgumentNullException("UserRepository is not initialized."); }

        return true;
    }

    /// <summary>
    /// Creates a new user in the database.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception> <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<SignUpResponseModel> RegisterUserAsync(ApplicationUserModel model)
    {
        // NULL CHECKS
        if (model == null) { throw new ArgumentNullException("User model is null."); }
        if (string.IsNullOrEmpty(model.UserName)) { throw new ArgumentNullException("Username is null!"); }
        if (string.IsNullOrEmpty(model.Email)) { throw new ArgumentNullException("Email is null!"); }
        if (string.IsNullOrEmpty(model.PasswordHash)) { throw new ArgumentNullException("HashedPassword is null!"); }
        if (string.IsNullOrEmpty(model.UserType)) { throw new ArgumentNullException("UserType is null!"); }
        
        // ADD USER TO DATABASE
        SignUpResponseModel response = await _userRepository.AddUserAsync(model);

        return response;
    }
}