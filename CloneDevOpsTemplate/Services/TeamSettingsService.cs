using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class TeamSettingsService(IHttpClientFactory httpClientFactory) : ITeamSettingsService
{

    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<TeamSettings?> GetTeamSettings(Guid projectId, Guid teamId)
    {
        return _client.GetFromJsonAsync<TeamSettings>($"{projectId}/{teamId}/_apis/work/teamsettings");
    }

    public Task<HttpResponseMessage> UpdateTeamSettings(Guid projectId, Guid teamId, PatchTeamSettings teamSettings)
    {
        return _client.PatchAsJsonAsync($"{projectId}/{teamId}/_apis/work/teamsettings?api-version=7.1", teamSettings);
    }
}