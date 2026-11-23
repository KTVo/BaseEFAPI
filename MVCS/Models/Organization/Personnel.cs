namespace MVCS.Models.Organization;

public sealed class Personnel
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PhoneExtension { get; set; }
    public string? Suffix { get; set; }

    public Guid? EducationalAgencyId { get; set; }
}