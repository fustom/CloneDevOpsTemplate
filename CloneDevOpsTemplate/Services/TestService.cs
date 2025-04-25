using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class TestService(IHttpClientFactory httpClientFactory) : ITestService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<TestPlans?> GetTestPlansAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<TestPlans>($"{projectId}/_apis/testplan/plans");
    }

    public Task<TestSuites?> GetTestSuitesAsync(Guid projectId, int testPlanId)
    {
        return _client.GetFromJsonAsync<TestSuites>($"{projectId}/_apis/testplan/plans/{testPlanId}/suites");
    }
}