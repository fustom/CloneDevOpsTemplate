using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface ITeamSettingsService
{
    Task<TeamSettings?> GetTeamSettings(Guid projectId, Guid teamId);
    Task<HttpResponseMessage> UpdateTeamSettings(Guid projectId, Guid teamId, PatchTeamSettings teamSettings);
}
