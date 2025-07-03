
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities;

public class OfficerMarketAssignment : BaseEntity
{
    public string AssistCenterOfficerId { get; set; }
    public string MarketId { get; set; }

    // Navigation properties
    public virtual AssistCenterOfficer AssistCenterOfficer { get; set; }
    public virtual Market Market { get; set; }
}
