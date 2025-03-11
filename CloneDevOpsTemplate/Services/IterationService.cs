using CloneDevOpsTemplate.Extensions;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class IterationService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : IIterationService
{
    private readonly HttpClient _client = httpClientFactory.CreateClientWithCredentials(httpContextAccessor?.HttpContext?.Session.GetString(Const.SessionKeyAccessToken) ?? string.Empty);
    private readonly string _organizationName = httpContextAccessor?.HttpContext?.Session.GetString(Const.SessionKeyOrganizationName) ?? string.Empty;

    public async Task<Iteration?> GetIterationAsync(Guid projectId, Guid iterationId)
    {
        return await _client.GetFromJsonAsync<Iteration>($"https://dev.azure.com/{_organizationName}/{projectId}/_apis/work/teamsettings/iterations/{iterationId}");
    }

    public async Task<Iterations?> GetIterationsAsync(Guid projectId)
    {
        return await _client.GetFromJsonAsync<Iterations>($"https://dev.azure.com/{_organizationName}/{projectId}/_apis/work/teamsettings/iterations");
    }
}
