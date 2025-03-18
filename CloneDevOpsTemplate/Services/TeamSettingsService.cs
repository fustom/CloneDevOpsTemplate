using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class TeamSettingsService(IHttpClientFactory httpClientFactory) : ITeamSettingsService
{

    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<TeamSettings?> GetTeamSettings(Guid projectId, Guid teamId)
    {
        return _client.GetFromJsonAsync<TeamSettings>($"{projectId}/{teamId}/_apis/work/teamsettings");
    }

    public Task UpdateTeamSettings(Guid projectId, Guid teamId, TeamSettings teamSettings)
    {
        return _client.PutAsJsonAsync($"{projectId}/{teamId}/_apis/work/teamsettings", teamSettings);
    }
}