using System.Net;
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

    public Task<Iteration?> GetAllAsync(Guid projectId, TreeStructureGroup structureGroup)
    {
        return _client.GetFromJsonAsync<Iteration>($"{projectId}/_apis/wit/classificationNodes/{structureGroup}?$depth=10");
    }

    public async Task<Iteration> CreateAsync(Guid projectId, TreeStructureGroup structureGroup, CreateIterationRequest iteration)
    {
        var result = await _client.PostAsJsonAsync($"{projectId}/_apis/wit/classificationNodes/{structureGroup}?api-version=7.1", iteration);
        if (result.IsSuccessStatusCode || result.StatusCode == HttpStatusCode.BadRequest)
        {
            return await result.Content.ReadFromJsonAsync<Iteration>() ?? new();
        }
        if (result.StatusCode == HttpStatusCode.Conflict)
        {
            return await GetAsync(projectId, structureGroup, iteration.Name) ?? new();
        }
        return new();
    }

    public async Task<Iteration> CreateAsync(Guid projectId, Iteration iterations, TreeStructureGroup structureGroup)
    {
        Iteration response = new();

        foreach (Iteration iteration in iterations.Children)
        {
            Iteration resp = await CreateAsync(projectId, structureGroup, new CreateIterationRequest
            {
                Name = iteration.Name
            });

            Iteration child = await CreateAsync(projectId, iteration, structureGroup);
            if (child.Children.Count > 0)
            {
                resp.Children.AddRange(child.Children);
            }
            if (resp.Id > 0)
            {
                response.Children.Add(resp);
            }
        }

        return response;
    }

    public Task<HttpResponseMessage> MoveAsync(Guid projectId, TreeStructureGroup structureGroup, string path, int Id)
    {
        return _client.PostAsJsonAsync($"{projectId}/_apis/wit/classificationNodes/{structureGroup}/{path}?api-version=7.1", new { id = Id });
    }

    public async Task MoveAsync(Guid projectId, List<Iteration> iterations, TreeStructureGroup structureGroup, string name)
    {
        foreach (Iteration iteration in iterations)
        {
            await MoveAsync(projectId, structureGroup, name, iteration.Id);
            if (iteration.Children.Count > 0)
            {
                await MoveAsync(projectId, iteration.Children, structureGroup, $"{name}/{iteration.Name}");
            }
        }
    }
}
