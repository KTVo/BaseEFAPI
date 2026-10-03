using BaseEFAPI.Helpers.Messages;
using BaseEFAPI.MVCS.Services.Registration.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseEFAPI.MVCS.Controllers.SignUp;

[ApiController]
[Route("api/v1/signup")]
[AllowAnonymous]
public class SignUpController(IRegistrationService registrationService, ILogger<SignUpController> logger) : ControllerBase
{
    private readonly IRegistrationService _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
    private readonly ILogger<SignUpController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequestModel request)
    {
        // NULL CHECKS
        if (request == null) { return BadRequest(ExternalMessages.RequestBodyIsNull); }

        if (string.IsNullOrEmpty(request.Username) == true)
        {
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignUpController", methodName: "SignUp", requestBody: request, extraInfo: ExternalMessages.UserNameIsNull));
            return BadRequest(ExternalMessages.UserNameIsNull);
        }
        if (string.IsNullOrEmpty(request.Email) == true)
        {
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignUpController", methodName: "SignUp", requestBody: request, extraInfo: ExternalMessages.EmailIsNull));
            return BadRequest(ExternalMessages.EmailIsNull);
        }
        if (string.IsNullOrEmpty(request.Password) == true)
        {
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignUpController", methodName: "SignUp", requestBody: request, extraInfo: ExternalMessages.PasswordIsNull));
            return BadRequest(ExternalMessages.PasswordIsNull);
        }
        if (string.IsNullOrEmpty(request.UserType) == true)
        {
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignUpController", methodName: "SignUp", requestBody: request, extraInfo: ExternalMessages.UserTypeIsNull));
            return BadRequest(ExternalMessages.UserTypeIsNull);
        }

        SignUpResponseModel response = await _registrationService.RegisterUserAsync(new ApplicationUserModel
        {
            UserName = request.Username,
            Email = request.Email,
            PasswordHash = request.Password,
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
