using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface ITeamsService
{
    Task<HttpResponseMessage> CreateTeamAsync(Guid projectId, string name = "New Team", string description = "New Team Description");
    Task<HttpResponseMessage> DeleteTeamAsync(Guid projectId, string teamId);
    Task<Teams?> GetAllTeamsAsync();
    Task<Team?> GetTeamAsync(Guid projectId, string teamId);
    Task<Teams?> GetTeamsAsync(Guid projectId);
    Task<HttpResponseMessage> UpdateTeamAsync(Guid projectId, string teamId, string name = "New Team", string description = "New Team Description");
}