using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class TeamsService(IHttpClientFactory httpClientFactory) : ITeamsService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<Teams?> GetTeamsAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<Teams>($"_apis/projects/{projectId}/teams");
    }
    public Task<Team?> GetTeamAsync(Guid projectId, string teamId)
    {
        return _client.GetFromJsonAsync<Team>($"_apis/projects/{projectId}/teams/{teamId}");
    }
    public Task<HttpResponseMessage> CreateTeamAsync(Guid projectId, string name = "New Team", string description = "New Team Description")
    {
        Team createTeam = new()
        {
            Name = name,
            Description = description
        };

        return _client.PostAsJsonAsync($"_apis/projects/{projectId}/teams", createTeam);
    }
    public Task<HttpResponseMessage> UpdateTeamAsync(Guid projectId, string teamId, string name = "New Team", string description = "New Team Description")
    {
        Team updateTeam = new()
        {
            Name = name,
            Description = description
        };

        return _client.PatchAsJsonAsync($"_apis/projects/{projectId}/teams/{teamId}", updateTeam);
    }
    public Task<HttpResponseMessage> DeleteTeamAsync(Guid projectId, string teamId)
    {
        return _client.DeleteAsync($"_apis/projects/{projectId}/teams/{teamId}");
    }
    public Task<Teams?> GetAllTeamsAsync()
    {
        return _client.GetFromJsonAsync<Teams>($"_apis/teams");
    }
}