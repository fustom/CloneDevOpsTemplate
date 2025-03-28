using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class WorkItemService(IHttpClientFactory httpClientFactory) : IWorkItemService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public async Task<WorkItemQueryList> GetWorkItemsAsync(Guid projectId, string projectName)
    {
        WorkItemQueryRequest wiqlRequest = new()
        {
            Query = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{projectName}'",
        };

        var result = await _client.PostAsJsonAsync($"{projectId}/_apis/wit/wiql?api-version=7.1", wiqlRequest);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<WorkItemQueryList>() ?? new();
        }
        return new();
    }

    public Task<WorkItem?> GetWorkItemAsync(Guid projectId, int workItemId)
    {
        return _client.GetFromJsonAsync<WorkItem>($"{projectId}/_apis/wit/workItems/{workItemId}?api-version=7.1");
    }
}

