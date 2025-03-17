using System.Net;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class IterationService(IHttpClientFactory httpClientFactory) : IIterationService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public async Task<Iteration> GetIterationAsync(Guid projectId, string name)
    {
        return await _client.GetFromJsonAsync<Iteration>($"{projectId}/_apis/wit/classificationNodes/iterations/{name}") ?? new();
    }

    public async Task<Iteration> GetIterationsAsync(Guid projectId)
    {
        return await _client.GetFromJsonAsync<Iteration>($"{projectId}/_apis/wit/classificationNodes/iterations?$depth=10") ?? new();
    }

    public async Task<Iteration> CreateIterationAsync(Guid projectId, CreateIterationRequest iteration)
    {
        var result = await _client.PostAsJsonAsync($"{projectId}/_apis/wit/classificationNodes/iterations?api-version=7.1", iteration);
        if (result.IsSuccessStatusCode || result.StatusCode == HttpStatusCode.BadRequest)
        {
            return await result.Content.ReadFromJsonAsync<Iteration>() ?? new();
        }
        if (result.StatusCode == HttpStatusCode.Conflict)
        {
            return await GetIterationAsync(projectId, iteration.Name);
        }
        return new();
    }

    public async Task<Iteration> CreateIterationAsync(Guid projectId, Iteration iterations)
    {
        Iteration response = new();

        foreach (Iteration iteration in iterations.Children)
        {
            Iteration resp = await CreateIterationAsync(projectId, new CreateIterationRequest
            {
                Name = iteration.Name
            });

            Iteration child = await CreateIterationAsync(projectId, iteration);
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

    public Task MoveIteration(Guid projectId, string path, int Id)
    {
        return _client.PostAsJsonAsync($"{projectId}/_apis/wit/classificationNodes/iterations/{path}?api-version=7.1", new { id = Id });
    }

    public async Task MoveIteration(Guid projectId, List<Iteration> iterations, string name)
    {
        foreach (Iteration iteration in iterations)
        {
            await MoveIteration(projectId, name, iteration.Id);
            if (iteration.Children.Count > 0) 
            {
                await MoveIteration(projectId, iteration.Children, $"{name}/{iteration.Name}");
            }
        }
    }
}
