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
    /// Validates that the required services are initialized and available for use.
    /// </summary>
    /// <returns></returns>
    public bool ValidateServices()
    {
        if (_dBcontext == null) { throw new ArgumentNullException("NexusDbContext is not initialized."); }
        if (_logger == null) { throw new ArgumentNullException("Logger is not initialized."); }

        return true;
    }

    /// <summary>
    /// ADDS AN ADMINISTRATOR TO RECORD IF FIRST NAME AND LAST NAME DID NOT MATCH LEA'S MOST RECENT RECORD
    /// </summary>
    /// <param name="agencyAdministrator"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<AgencyAdministratorResponse> AddAgencyAdministratorAsync(AgencyAdministrator agencyAdministrator)
    {
        if (agencyAdministrator == null) throw new ArgumentNullException(nameof(agencyAdministrator));
        if (string.IsNullOrEmpty(agencyAdministrator.FirstName) == true) { return new() { IsSuccess = false }; }
        if (string.IsNullOrEmpty(agencyAdministrator.LastName) == true) { return new() { IsSuccess = false }; }
        if (string.IsNullOrEmpty(agencyAdministrator.CDSCode) == true) { return new() { IsSuccess = false }; }

        try
        {
            // GETS THE LATEST RECORD OF THE LEA FOR THE MOST RECENT ADMINISTRATOR
            IQueryable<AgencyAdministrator> query = _dBcontext
            .AgencyAdministrator
            .AsNoTracking()
            .Where(x => x.CDSCode == agencyAdministrator.CDSCode)
            .OrderByDescending(x => x.TimeStampUTC)
            .Take(1);

            AgencyAdministrator? foundUser = query.FirstOrDefault();

            // IF RECORD IS FOUND COMPARE THE ADMINISTRATOR BY FIRST AND LAST NAME
            if (foundUser != null)
            {
                // COMPARE THE ADMINISTRATOR BY FIRST AND LAST NAME
                if (
                    string.Equals(
                        foundUser.FirstName,
                        agencyAdministrator.FirstName,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        foundUser.LastName,
                        agencyAdministrator.LastName,
                        StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        return new AgencyAdministratorResponse()
                        {
                            IsSuccess = true,
                            RecordHasChanged = false,
                            Message = AdministratorNotChanged
                        };
                    }
            }


            // SET THE TIMESTAMP
            agencyAdministrator.TimeStampUTC = DateTime.UtcNow;
            // ADDS RECORD TO DATABASE TABLE
            await _dBcontext.AgencyAdministrator.AddAsync(agencyAdministrator);
            // SAVES CHANGES
            await _dBcontext.SaveChangesAsync();

            return new()
            {
                IsSuccess = true,
                RecordHasChanged = true,
                Message = AddAgencyAdministratorSuccess
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, AdministratorNotChanged);

            return new()
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }

    }
}
