using BaseEFAPI.Helpers.Messages;
using BaseEFAPI.MVCS.Models.Authorization;
using BaseEFAPI.MVCS.Models.SignIn;
using BaseEFAPI.MVCS.Services.Authentication.Interfaces;
using BaseEFAPI.MVCS.Services.SignIn.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseEFAPI.MVCS.Controllers.SignIn;

[ApiController]
[Route("api/v1/signin")]
[AllowAnonymous]
public class SignInController(IJwtTokenService jwtTokenService, ISignInService signInService, ILogger<SignInController> logger) : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    private readonly ISignInService _signInService = signInService ?? throw new ArgumentNullException(nameof(signInService));
    private readonly ILogger<SignInController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPost]
    [Route("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequestModel request)
    {
        // NULL CHECKS
        if (request == null)
        {
#pragma warning disable CA2254 // Template should be a static expression
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignInController", methodName: "SignIn", requestBody: request, extraInfo: ExternalMessages.RequestBodyIsNull));
#pragma warning restore CA2254 // Template should be a static expression

            return BadRequest(ExternalMessages.RequestBodyIsNull);
        }
        if (string.IsNullOrEmpty(request.UserName) == true)
        {
#pragma warning disable CA2254 // Template should be a static expression
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignInController", methodName: "SignIn", requestBody: request, extraInfo: ExternalMessages.UserNameIsNull));
#pragma warning restore CA2254 // Template should be a static expression
            return BadRequest(ExternalMessages.UserNameIsNull);
        }

        if (string.IsNullOrEmpty(request.Password) == true)
        {
#pragma warning disable CA2254 // Template should be a static expression
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignInController", methodName: "SignIn", requestBody: request, extraInfo: ExternalMessages.PasswordIsNull));
#pragma warning restore CA2254 // Template should be a static expression
            return BadRequest(ExternalMessages.PasswordIsNull);
        }

        SignInResponseModel response = await _signInService.SignInUserAsync(request);

        if (response.IsSuccess == false)
        {
#pragma warning disable CA2254 // Template should be a static expression
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignInController", methodName: "SignIn", requestBody: request, extraInfo: ExternalMessages.UserNameIsNull));
#pragma warning restore CA2254 // Template should be a static expression

            return StatusCode(StatusCodes.Status500InternalServerError, response.Message);
        }

        return Ok(response);
    }
    
    [HttpPost]
    [Route("test/decrypt/jwe/token")]
    public async Task<IActionResult> DecryptJWEToken([FromBody] string tokenString)
    {
        if (string.IsNullOrEmpty(tokenString) == true)
        {
#pragma warning disable CA2254 // Template should be a static expression
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignInController", methodName: "DecryptJWEToken", requestBody: tokenString, extraInfo: ExternalMessages.TokenStringIsNull));
#pragma warning restore CA2254 // Template should be a static expression
            return BadRequest(ExternalMessages.TokenStringIsNull);
        }

        DecryptedJweTokenResponse response = _jwtTokenService.DecryptJWEToken(tokenString);

        if (response.IsSuccess == false)
        {

#pragma warning disable CS8604 // Possible null reference argument.
            _logger.LogWarning(MessageGenerator.RequestErrorMessage(className: "SignInController", methodName: "DecryptJWEToken", requestBody: tokenString, extraInfo: response.Message));
#pragma warning restore CS8604 // Possible null reference argument.
            return StatusCode(StatusCodes.Status500InternalServerError, response.Message);
        }

        return Ok(response);
    }



}
