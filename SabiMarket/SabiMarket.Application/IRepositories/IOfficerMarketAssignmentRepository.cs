
public interface IOfficerMarketAssignmentRepository
{
    void AddAssignment(OfficerMarketAssignment assignment);
    void RemoveAssignment(OfficerMarketAssignment assignment);
    Task<List<OfficerMarketAssignment>> GetAssignmentsByOfficerId(string officerId);
    Task<bool> HasAssignment(string officerId, string marketId);
}