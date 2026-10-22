using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class PolicyService(IHttpClientFactory httpClientFactory) : IPolicyService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<PolicyConfigurations?> GetPolicyConfigurationsAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<PolicyConfigurations>($"{projectId}/_apis/policy/configurations?api-version=7.1");
    }

    public async Task<PolicyConfiguration?> CreatePolicyConfigurationAsync(Guid projectId, PolicyConfiguration policyConfiguration)
    {
        var result = await _client.PostAsJsonAsync($"{projectId}/_apis/policy/configurations?api-version=7.1", policyConfiguration);
        return await result.Content.ReadFromJsonAsync<PolicyConfiguration>();
    }
}
