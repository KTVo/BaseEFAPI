using System.ComponentModel.DataAnnotations.Schema;

[Table("ApplicationUser")]
public sealed class ApplicationUserModel
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public string? Username { get; set; }
    public required string Email { get; set; }
    public string? HashedPassword { get; set; }
    public required string UserType { get; set; }
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}