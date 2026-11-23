using MVCS.Models.Organization;

namespace BaseEFAPI.MVCS.Models.Organization;

public sealed class AgencyAdministratorResponse : BaseResponseModel
{ 
    public bool? RecordHasChanged { get; set; }
}
