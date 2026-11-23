namespace MVCS.Models.Organization;

public sealed class AgencyAdministrator
{
    public Guid Id { get; set; }

    // Agency
    public string? CDSCode { get; set; }
    public string? OrganizationType { get; set; }
    public string? AgencyName { get; set; }
    public string? AgencyAddress { get; set; }
    public string? ActiveStatus { get; set; }
    public bool? IsActive { get; set; }
    public string? Website { get; set; }
    public string? AgencyEmail { get; set; }

    // Person
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Suffix { get; set; }
    public string? Title { get; set; }
    public string? PersonEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PhoneExtension { get; set; }

    public DateTime? TimeStampUTC { get; set; }
}