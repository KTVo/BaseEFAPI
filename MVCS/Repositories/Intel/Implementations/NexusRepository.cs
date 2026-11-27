using BaseEFAPI.MVCS.Models.Organization;
using BaseEFAPI.MVCS.Services.Context;
using Microsoft.EntityFrameworkCore;
using MVCS.Models.Organization;

public sealed class NexusRepository
{
    private readonly NexusDbContext _dBcontext;    
    private readonly ILogger<NexusRepository> _logger;

    public const string AddAgencyAdministratorSuccess = "SUCCESSFULLY ADDED ADMINISTRATOR.";
    public const string AdministratorNotChanged = "ADMINISTRATOR HAS NOT CHANGED!";

    public NexusRepository(NexusDbContext context, ILogger<NexusRepository> logger)
    {
        _dBcontext = context;
        _logger = logger;

        if (ValidateServices() == false)
        {
            throw new InvalidOperationException("Required services are not properly initialized.");
        }
    }

    /// <summary>
    /// VALIDATES SERVICES
    /// </summary>
    /// <returns></returns>
    public bool ValidateServices()
    {
        if (_dBcontext == null) { throw new ArgumentNullException("NexusDbContext is not initialized."); }
        if (_logger == null) { throw new ArgumentNullException("Logger is not initialized."); }

        return true;
    }

    #region PRIVATE_HELPERS
    /// <summary>
    /// GETS THE LATEST RECORDS OF AN AGENCY IN DESCENDING ORDER BY TIME
    /// </summary>
    /// <param name="cdsCode"></param>
    /// <returns></returns>
    private IQueryable<AgencyAdministrator> Latest(string cdsCode) =>
        _dBcontext.AgencyAdministrator.AsNoTracking()
            .Where(x => x.Agency.CDSCode == cdsCode)
            .OrderByDescending(x => x.RecordedAtUtc)
            .ThenByDescending(x => x.Id);
    #endregion PRIVATE_HELPERS

    /// <summary>
    /// APPENDS A ROW WHEN THERE HAS BEEN A CHANGE FROM MOST RECENT RECORD
    /// </summary>
    /// <param name="agencyAdministrator"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<AgencyAdministratorResponse> AddAgencyAdministratorsAsync(
        IList<AgencyAdministrator> agencyAdministrators,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agencyAdministrators);

        if (agencyAdministrators.Count == 0)
            return new() { IsSuccess = true, RecordHasChanged = false };

        // VALIDATE RECORDS
        foreach (AgencyAdministrator administrator in agencyAdministrators)
        {
            if (administrator?.Agency is null ||
                administrator.Person is null ||
                string.IsNullOrWhiteSpace(administrator.Person.FirstName) ||
                string.IsNullOrWhiteSpace(administrator.Person.LastName) ||
                string.IsNullOrWhiteSpace(administrator.Agency.CDSCode))
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Invalid agency or person details."
                };
            }

            administrator.Agency.CDSCode =
                administrator.Agency.CDSCode.Trim();
        }

        try
        {
            // GET DISTINCT CDS CODES
            List<string> cdsCodes = agencyAdministrators
                .Select(x => x.Agency.CDSCode!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // RETRIEVE LATEST RECORD FOR EACH CDS CODE
            List<AgencyAdministrator> latestRecords = await _dBcontext.AgencyAdministrator
                .AsNoTracking()
                .Include(x => x.Agency)
                .Include(x => x.Person)
                .Where(x => cdsCodes.Contains(x.Agency.CDSCode!))
                .GroupBy(x => x.Agency.CDSCode)
                .Select(x => x
                    .OrderByDescending(y => y.RecordedAtUtc)
                    .First())
                .ToListAsync(cancellationToken);

            // CREATE DICTIONARY FOR FAST LOOKUPS
            Dictionary<string, AgencyAdministrator> latestByCdsCode = latestRecords.ToDictionary(
                x => x.Agency.CDSCode!,
                StringComparer.OrdinalIgnoreCase);

            List<AgencyAdministrator> recordsToInsert = new List<AgencyAdministrator>();

            // COMPARE RECORDS IN MEMORY
            foreach (AgencyAdministrator administrator in agencyAdministrators)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string cdsCode = administrator.Agency.CDSCode!;

                if (latestByCdsCode.TryGetValue(cdsCode, out AgencyAdministrator? latest) &&
                    HasSameDetails(latest, administrator))
                {
                    continue;
                }

                administrator.RecordedAtUtc = DateTime.UtcNow;

                recordsToInsert.Add(administrator);

                // ACCOUNT FOR DUPLICATE CDS CODES IN THE INCOMING LIST
                latestByCdsCode[cdsCode] = administrator;
            }

            if (recordsToInsert.Count == 0)
            {
                return new()
                {
                    IsSuccess = true,
                    RecordHasChanged = false,
                    Message = AdministratorNotChanged
                };
            }

            // ADD ALL CHANGED RECORDS
            _dBcontext.AgencyAdministrator.AddRange(recordsToInsert);

            // SAVE ALL CHANGES
            await _dBcontext.SaveChangesAsync(cancellationToken);

            return new()
            {
                IsSuccess = true,
                RecordHasChanged = true,
                Message = $"{recordsToInsert.Count} records added successfully."
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save administrator observations.");

            return new()
            {
                IsSuccess = false,
                Message = "Unable to save administrator observations."
            };
        }
    }

    // One tracked administrator position per agency. Historic rows are not a roster.
    /// <summary>
    /// GET CURRENT RECORDS OF AGENCY
    /// </summary>
    /// <param name="cdsCode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<AgencyAdministrator?> GetCurrentAgencyAsync(
        string cdsCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cdsCode);
        return await Latest(cdsCode.Trim()).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// GET HISTORICAL RECORDS OF AN AGENCY
    /// </summary>
    /// <param name="cdsCode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<AgencyAdministrator>> GetAgencyHistoryAsync(
        string cdsCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cdsCode);
        return await Latest(cdsCode.Trim()).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// CHECKS FOR ROW CHANGES
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    internal static bool HasSameDetails(AgencyAdministrator left, AgencyAdministrator right) =>
       string.Equals(left.Agency.CDSCode, right.Agency.CDSCode, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Agency.OrganizationType, right.Agency.OrganizationType, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Agency.Name, right.Agency.Name, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Agency.Address, right.Agency.Address, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Agency.ActiveStatus, right.Agency.ActiveStatus, StringComparison.OrdinalIgnoreCase) &&
       left.Agency.IsActive == right.Agency.IsActive &&
       string.Equals(left.Agency.Website, right.Agency.Website, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Agency.Email, right.Agency.Email, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Person.FirstName, right.Person.FirstName, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Person.MiddleName, right.Person.MiddleName, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Person.LastName, right.Person.LastName, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Person.Suffix, right.Person.Suffix, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Person.Title, right.Person.Title, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Person.Email, right.Person.Email, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Person.PhoneNumber, right.Person.PhoneNumber, StringComparison.OrdinalIgnoreCase) &&
       string.Equals(left.Person.PhoneExtension, right.Person.PhoneExtension, StringComparison.OrdinalIgnoreCase);
}
