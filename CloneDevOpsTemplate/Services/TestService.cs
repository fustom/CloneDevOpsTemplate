using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface ITestService
{
    Task<TestPlans?> GetTestPlansAsync(Guid projectId);
}

public class TestService(IHttpClientFactory httpClientFactory) : ITestService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<TestPlans?> GetTestPlansAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<TestPlans>($"{projectId}/_apis/testplan/plans");
    }
}