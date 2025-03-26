using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface ITeamsService
{
    Task<Team> CreateTeamAsync(Guid projectId, string name = "New Team", string description = "New Team Description");
    Task<Dictionary<Guid, Guid>> CreateTeamFromTemplateAsync(Guid projectId, Team[] templateTeams, Guid defaultTeamId, Guid newDefaultTeamId);
    Task<Teams?> GetAllTeamsAsync();
    Task<Team?> GetTeamAsync(Guid projectId, Guid teamId);
    Task<Teams?> GetTeamsAsync(Guid projectId);
}