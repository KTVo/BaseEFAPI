using BaseEFAPI.MVCS.Services.Context;
using Microsoft.EntityFrameworkCore;

public sealed class UserRepository : IUserRepository
{
    private readonly RegistrationDbContext _dBcontext;

    public UserRepository(RegistrationDbContext context)
    {
        _dBcontext = context;
    }

    /// <summary>
    /// Validates that the required services are initialized and available for use.
    /// </summary>
    /// <returns></returns>
    public bool ValidateServices()
    {
        if (_dBcontext == null) { throw new ArgumentNullException("RegistrationDbContext is not initialized."); }

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
    public async Task<SignUpResponseModel> AddSignUpAsync(ApplicationUserModel user)
    {
        // NULL CHECKS
        if (user == null) { throw new ArgumentNullException("User model is null."); }
        if (string.IsNullOrEmpty(user.Username)) { throw new ArgumentNullException("Username is null!"); }
        if (string.IsNullOrEmpty(user.Email)) { throw new ArgumentNullException("Email is null!"); }
        if (string.IsNullOrEmpty(user.HashedPassword)) { throw new ArgumentNullException("HashedPassword is null!"); }
        if (string.IsNullOrEmpty(user.UserType)) { throw new ArgumentNullException("UserType is null!"); }

        try
        {

            // CHECK IF USER ALREADY EXISTS BY EMAIL
            ApplicationUserResponse existingUserEmail = await GetUserByEmailAsync(user.Email);

            // IF USER ALREADY EXISTS BY EMAIL, RETURN FAILURE RESPONSE
            if (existingUserEmail.User != null)
            {
                return new SignUpResponseModel
                {
                    IsSuccess = false,
                    Message = "User with this email already exists!"
                };
            }
                
            // IF QUERY FAILED, RETURN FAILURE RESPONSE
            if (existingUserEmail.IsSuccess == false)
            {
                return new SignUpResponseModel
                {
                    IsSuccess = false,
                    Message = existingUserEmail.Message
                };

            }
        

            // CHECK IF USER ALREADY EXISTS BY USERNAME
            ApplicationUserResponse existingUserUsername = await GetUserByUsernameAsync(user.Username);

            // IF USER ALREADY EXISTS BY USERNAME, RETURN FAILURE RESPONSE
            if (existingUserUsername.User != null)
            {
                return new SignUpResponseModel
                {
                    IsSuccess = false,
                    Message = "User with this username already exists!"
                };
            }

            // IF QUERY FAILED, RETURN FAILURE RESPONSE
            if (existingUserUsername.IsSuccess == false)
            {
                return new SignUpResponseModel
                {
                    IsSuccess = false,
                    Message = existingUserUsername.Message
                };

            }


            // ADD USER TO DATABASE
            await _dBcontext.ApplicationUser.AddAsync(user);
            // SAVE CHANGES TO DATABASE
            await _dBcontext.SaveChangesAsync();

            return new SignUpResponseModel
            {
                IsSuccess = true,
                Message = "User registered successfully."
            };
        }
        catch (Exception ex)
        {
            return new SignUpResponseModel
            {
                IsSuccess = false,
                Message = $"Error occurred while adding user: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Retrieves a user from the database by their email address.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception> <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<ApplicationUserResponse> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email)) { throw new ArgumentNullException("Email is null or empty!"); }

        try
        {
            ApplicationUserModel? user = await _dBcontext.ApplicationUser.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new ApplicationUserResponse
                {
                    IsSuccess = true,
                    Message = "User not found!"
                };
            }

            return new()
            {
                IsSuccess = true,
                Message = "User retrieved successfully.",
                User = user
            };
        }
        catch (Exception ex)
        {
            return new ApplicationUserResponse
            {
                IsSuccess = false,
                Message = $"Error occurred while retrieving user: {ex.Message}"
            };
        }

    }
    
    /// <summary>
    /// Retrieves a user from the database by their username.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
     public async Task<ApplicationUserResponse> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrEmpty(username)) { throw new ArgumentNullException("Username is null or empty!"); }

        try
        {
            ApplicationUserModel? user = await _dBcontext.ApplicationUser.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                return new ApplicationUserResponse
                {
                    IsSuccess = true,
                    Message = "User not found!"
                };
            }

            return new()
            {
                IsSuccess = true,
                Message = "User retrieved successfully.",
                User = user
            };
        }
        catch (Exception ex)
        {
            return new ApplicationUserResponse
            {
                IsSuccess = false,
                Message = $"Error occurred while retrieving user: {ex.Message}"
            };
        }

    }
}