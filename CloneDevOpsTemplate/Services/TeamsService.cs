using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class TeamsService(IHttpClientFactory httpClientFactory) : ITeamsService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<Teams?> GetTeamsAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<Teams>($"_apis/projects/{projectId}/teams");
    }
    public Task<Team?> GetTeamAsync(Guid projectId, Guid teamId)
    {
        return _client.GetFromJsonAsync<Team>($"_apis/projects/{projectId}/teams/{teamId}");
    }
    public async Task<Team> CreateTeamAsync(Guid projectId, string name = "New Team", string description = "New Team Description")
    {
        Team createTeam = new()
        {
            Name = name,
            Description = description
        };

        var result = await _client.PostAsJsonAsync($"_apis/projects/{projectId}/teams?api-version=7.1", createTeam);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<Team>() ?? new();
        }
        return new();
    }
    public async Task<Dictionary<Guid, Guid>> CreateTeamFromTemplateAsync(Guid projectId, Team[] templateTeams, Guid defaultTeamId, Guid newDefaultTeamId)
    {
        Dictionary<Guid, Guid> mapTeams = [];
        foreach(Team templateTeam in templateTeams)
        {
            if (templateTeam.Id != defaultTeamId)
            {
                var newTeam = await CreateTeamAsync(projectId, templateTeam.Name, templateTeam.Description);
                mapTeams.Add(templateTeam.Id, newTeam.Id);
            }
            else
            {
                mapTeams.Add(templateTeam.Id, newDefaultTeamId);
            }
        }

        return mapTeams;
    }
    public Task<Teams?> GetAllTeamsAsync()
    {
        return _client.GetFromJsonAsync<Teams>($"_apis/teams");
    }
}