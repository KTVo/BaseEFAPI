using BaseEFAPI.Helpers.Messages;
using BaseEFAPI.MVCS.Models.SignIn;
using BaseEFAPI.MVCS.Services.SignIn.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseEFAPI.MVCS.Controllers.SignIn;

[ApiController]
[Route("api/v1/signin")]
[AllowAnonymous]
public class SignInController(ISignInService signInService, ILogger<SignInController> logger) : ControllerBase
{
    private readonly ISignInService _signInService = signInService ?? throw new ArgumentNullException(nameof(signInService));
    private readonly ILogger<SignInController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPost]
    [Route("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequestModel request)
    {
        // NULL CHECKS
        if (request == null) {
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignInController", methodName: "SignIn", requestBody: request, extraInfo: ExternalMessages.RequestBodyIsNull));

            return BadRequest(ExternalMessages.RequestBodyIsNull); 
            }
        if (string.IsNullOrEmpty(request.UserName) == true)
        {
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignInController", methodName: "SignIn", requestBody: request, extraInfo: ExternalMessages.UserNameIsNull));
            return BadRequest(ExternalMessages.UserNameIsNull);
        }

        if (string.IsNullOrEmpty(request.Password) == true)
        {
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignInController", methodName: "SignIn", requestBody: request, extraInfo: ExternalMessages.PasswordIsNull));
            return BadRequest(ExternalMessages.PasswordIsNull);
        }

        SignInResponseModel response = await _signInService.SignInUserAsync(request);

        if (response.IsSuccess == false)
        {
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignInController", methodName: "SignIn", requestBody: request, extraInfo: ExternalMessages.UserNameIsNull));

            return StatusCode(StatusCodes.Status500InternalServerError, response.Message);
        }

        return Ok(response);
    }



}
