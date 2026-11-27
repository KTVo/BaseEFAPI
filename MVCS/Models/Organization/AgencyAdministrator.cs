namespace MVCS.Models.Organization;

public sealed class AgencyAdministrator
{
    public Guid Id { get; set; }
    public Agency Agency { get; set; } = new();
    public Person Person { get; set; } = new();
    public DateTime? RecordedAtUtc { get; set; }
}
