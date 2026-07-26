public interface IUserRepository
{
    Task<SignUpResponseModel> AddSignUpAsync(ApplicationUserModel user);
    Task<ApplicationUserResponse> GetUserByEmailAsync(string email);
    Task<ApplicationUserResponse> GetUserByUsernameAsync(string username);
}