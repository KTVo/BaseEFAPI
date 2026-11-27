using BaseEFAPI.MVCS.Models.Organization;
using MVCS.Models.Organization;

namespace BaseEFAPI.MVCS.Repositories.Intel.Interfaces;

public interface INexusRepository
{
    Task<AgencyAdministratorResponse> AddAgencyAdministratorsAsync(
        IList<AgencyAdministrator> agencyAdministrators,
        CancellationToken cancellationToken = default);
}
