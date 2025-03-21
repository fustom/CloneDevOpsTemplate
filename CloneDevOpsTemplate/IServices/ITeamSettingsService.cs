using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface ITeamSettingsService
{
    Task<TeamSettings?> GetTeamSettings(Guid projectId, Guid teamId);
    Task UpdateTeamSettings(Guid projectId, Guid teamId, TeamSettings teamSettings);
}
