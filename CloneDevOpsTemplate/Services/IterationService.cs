using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class IterationService(IHttpClientFactory httpClientFactory) : IIterationService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public async Task<Iteration?> GetIterationAsync(Guid projectId, Guid iterationId)
    {
        return await _client.GetFromJsonAsync<Iteration>($"{projectId}/_apis/work/teamsettings/iterations/{iterationId}");
    }

    public async Task<Iterations?> GetIterationsAsync(Guid projectId)
    {
        return await _client.GetFromJsonAsync<Iterations>($"{projectId}/_apis/work/teamsettings/iterations");
    }
}
