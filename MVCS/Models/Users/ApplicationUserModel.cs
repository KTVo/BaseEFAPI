using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

[Table("ApplicationUser")]
public sealed class ApplicationUserModel : IdentityUser
{
    public required string UserType { get; set; }
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
