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

    public async Task<CloneResult> CreateAsync(Guid projectId, Iteration classificationNodes, TreeStructureGroup structureGroup, string path)
    {
        var result = new CloneResult();
        await CreateChildrenAsync(projectId, classificationNodes, structureGroup, path, result);
        return result;
    }

    private async Task CreateChildrenAsync(Guid projectId, Iteration classificationNodes, TreeStructureGroup structureGroup, string path, CloneResult result)
    {
        foreach (var classificationNode in classificationNodes.Children)
        {
            var nodePath = string.IsNullOrEmpty(path) ? classificationNode.Name : $"{path}/{classificationNode.Name}";
            var response = await CreateAsync(projectId, structureGroup, classificationNode, path);
            // Children of a node that could not be created would fail too, so skip them.
            if (await result.CheckResponseAsync(response, $"Creating '{nodePath}'"))
            {
                await CreateChildrenAsync(projectId, classificationNode, structureGroup, nodePath, result);
            }
        }
    }

    public async Task<HttpResponseMessage> DeleteAsync(Guid projectId, TreeStructureGroup structureGroup, string name)
    {
        return await _client.DeleteAsync($"{projectId}/_apis/wit/classificationNodes/{structureGroup}/{name}?api-version=7.1");
    }

    public async Task<CloneResult> DeleteAsync(Guid projectId, TreeStructureGroup structureGroup, Iteration classificationNodes)
    {
        var result = new CloneResult();
        foreach (var classificationNode in classificationNodes.Children)
        {
            var response = await DeleteAsync(projectId, structureGroup, classificationNode.Name);
            await result.CheckResponseAsync(response, $"Removing '{classificationNode.Name}'");
        }
        return result;
    }
}
