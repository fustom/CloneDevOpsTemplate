using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class WorkItemService(IHttpClientFactory httpClientFactory) : IWorkItemService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public async Task<WorkItemsListQueryResult?> GetWorkItemsListAsync(Guid projectId, string projectName)
    {
        WorkItemsListQueryRequest wiqlRequest = new()
        {
            Query = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{projectName}'",
        };

        var result = await _client.PostAsJsonAsync($"{projectId}/_apis/wit/wiql?api-version=7.1", wiqlRequest);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<WorkItemsListQueryResult>();
        }
        return new();
    }

    public async Task<WorkItems?> GetWorkItemsAsync(Guid projectId, int[] workItemIds)
    {
        WorkItemsQueryRequest wiQueryRequest = new()
        {
            Ids = workItemIds,
        };
        
        var result = await _client.PostAsJsonAsync($"{projectId}/_apis/wit/workitemsbatch?api-version=7.1", wiQueryRequest);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<WorkItems>();
        }
        return new();
}

    public Task<WorkItem?> GetWorkItemAsync(Guid projectId, int workItemId)
    {
        return _client.GetFromJsonAsync<WorkItem>($"{projectId}/_apis/wit/workItems/{workItemId}?api-version=7.1");
    }
}

