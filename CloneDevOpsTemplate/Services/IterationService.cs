using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class IterationService(IHttpClientFactory httpClientFactory) : IIterationService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<Iteration?> GetAsync(Guid projectId, TreeStructureGroup structureGroup, string name)
    {
        return _client.GetFromJsonAsync<Iteration>($"{projectId}/_apis/wit/classificationNodes/{structureGroup}/{name}");
    }

    public Task<Iteration?> GetAsync(Guid projectId, TreeStructureGroup structureGroup, int depth)
    {
        return _client.GetFromJsonAsync<Iteration>($"{projectId}/_apis/wit/classificationNodes/{structureGroup}?$depth={depth}");
    }

    public Task<Iteration?> GetAllAsync(Guid projectId, TreeStructureGroup structureGroup)
    {
        return GetAsync(projectId, structureGroup, int.MaxValue);
    }

    public async Task<HttpResponseMessage> CreateAsync(Guid projectId, TreeStructureGroup structureGroup, ClassificationNodeBase classificationNode, string path)
    {
        return await _client.PostAsJsonAsync($"{projectId}/_apis/wit/classificationNodes/{structureGroup}/{path}?api-version=7.1", classificationNode);
    }

    public async Task CreateAsync(Guid projectId, Iteration classificationNodes, TreeStructureGroup structureGroup, string path)
    {
        foreach (var classificationNode in classificationNodes.Children)
        {
            await CreateAsync(projectId, structureGroup, classificationNode, path);
            await CreateAsync(projectId, classificationNode, structureGroup, classificationNode.Name);
        }
    }

    public async Task<HttpResponseMessage> DeleteAsync(Guid projectId, TreeStructureGroup structureGroup, string name)
    {
        return await _client.DeleteAsync($"{projectId}/_apis/wit/classificationNodes/{structureGroup}/{name}?api-version=7.1");
    }

    public async Task DeleteAsync(Guid projectId, TreeStructureGroup structureGroup, Iteration classificationNodes)
    {
        foreach (var classificationNode in classificationNodes.Children)
        {
            await DeleteAsync(projectId, structureGroup, classificationNode.Name);
        }
    }
}
