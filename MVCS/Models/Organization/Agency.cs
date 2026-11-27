namespace MVCS.Models.Organization;

// Captured details belonging to one administrator snapshot; no independent identity.
public sealed class Agency
{
    public string? CDSCode { get; set; }
    public string? OrganizationType { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? ActiveStatus { get; set; }
    public bool? IsActive { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
}
