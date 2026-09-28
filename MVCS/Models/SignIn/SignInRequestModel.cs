using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseEFAPI.MVCS.Models.SignIn;

public sealed class SignInRequestModel : BaseRequestModel
{
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public required string Password { get; set; }
}
