using BaseEFAPI.Helpers.Encryption;
using BaseEFAPI.MVCS.Services.Registration.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseEFAPI.MVCS.Controllers.SignUp;

[ApiController]
[Route("api/v1/signup")]
[AllowAnonymous]
public class SignUpController(IRegistrationService registrationService) : ControllerBase
{
    private readonly IRegistrationService _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequestModel request)
    {
        // NULL CHECKS
        if (request == null) { return BadRequest(ExternalMessages.RequestBodyIsNull); }
        if (string.IsNullOrEmpty(request.Username) == true ||
            string.IsNullOrEmpty(request.Email) == true ||
            string.IsNullOrEmpty(request.HashedPassword) == true ||
            string.IsNullOrEmpty(request.UserType) == true)
        {
            return BadRequest(ExternalMessages.InvalidRequest);
        }

        request.HashedPassword = PasswordEncryption.HashPassword(request.HashedPassword);


        SignUpResponseModel response = await _registrationService.RegisterUserAsync(new ApplicationUserModel
        {
            UserName = request.Username,
            Email = request.Email,
            PasswordHash = request.HashedPassword,
            UserType = request.UserType,
            CreatedAt = DateTime.UtcNow
        });

        if (response.IsSuccess == false)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response.Message);
        }

        return Ok(response);
    }
}
