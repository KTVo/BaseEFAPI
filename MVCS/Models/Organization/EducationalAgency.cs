namespace MVCS.Models.Organization;

public sealed class EducationalAgency
{
    public Guid Id { get; set; }
    public string? CountyCode { get; set; }
    public string? DistrictCode { get; set; }
    public string? SchoolCode { get; set; }
 
    public string? OrganizationType { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? ActiveStatus { get; set; }
    public bool? IsActive { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }

    public List<Personnel> Administrators { get; set; } = new();
}