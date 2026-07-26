public sealed class SignUpRequestModel : BaseRequestModel
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? HashedPassword { get; set; }
    public string? UserType { get; set; }
}