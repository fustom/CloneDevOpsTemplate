using System.Net;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class IterationService(IHttpClientFactory httpClientFactory) : IIterationService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<Iteration?> GetIterationAsync(Guid projectId, string name)
    {
        return _client.GetFromJsonAsync<Iteration>($"{projectId}/_apis/wit/classificationNodes/iterations/{name}");
    }

    public Task<Iteration?> GetIterationsAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<Iteration>($"{projectId}/_apis/wit/classificationNodes/iterations?$depth=10");
    }

    public Task<Iteration?> GetAreaAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<Iteration>($"{projectId}/_apis/wit/classificationNodes/areas?$depth=10");
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
            return await GetIterationAsync(projectId, iteration.Name) ?? new();
        }
        return new();
    }

    public async Task<Iteration> CreateIterationAsync(Guid projectId, Iteration iterations)
    {
        return await CreateAsync(projectId, iterations, CreateIterationAsync);
    }

    public async Task<Iteration> CreateAreaAsync(Guid projectId, CreateIterationRequest iteration)
    {
        var result = await _client.PostAsJsonAsync($"{projectId}/_apis/wit/classificationNodes/areas?api-version=7.1", iteration);
        if (result.IsSuccessStatusCode || result.StatusCode == HttpStatusCode.BadRequest)
        {
            return await result.Content.ReadFromJsonAsync<Iteration>() ?? new();
        }
        if (result.StatusCode == HttpStatusCode.Conflict)
        {
            return await GetIterationAsync(projectId, iteration.Name) ?? new();
        }
        return new();
    }

    public async Task<Iteration> CreateAreaAsync(Guid projectId, Iteration iterations)
    {
        return await CreateAsync(projectId, iterations, CreateAreaAsync);
    }

    private static async Task<Iteration> CreateAsync(Guid projectId, Iteration iterations, Func<Guid, CreateIterationRequest, Task<Iteration>> createAsyncFunc)
    {
        Iteration response = new();

        foreach (Iteration iteration in iterations.Children)
        {
            Iteration resp = await createAsyncFunc(projectId, new CreateIterationRequest
            {
                Name = iteration.Name
            });

            Iteration child = await CreateAsync(projectId, iteration, createAsyncFunc);
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

    public Task<HttpResponseMessage> MoveIterationAsync(Guid projectId, string path, int Id)
    {
        return _client.PostAsJsonAsync($"{projectId}/_apis/wit/classificationNodes/iterations/{path}?api-version=7.1", new { id = Id });
    }

    public Task<HttpResponseMessage> MoveAreaAsync(Guid projectId, string path, int Id)
    {
        return _client.PostAsJsonAsync($"{projectId}/_apis/wit/classificationNodes/areas/{path}?api-version=7.1", new { id = Id });
    }

    private static async Task MoveAsync(Guid projectId, List<Iteration> iterations, string name, Func<Guid, string, int,Task<HttpResponseMessage>> moveAsyncFunc)
    {
        foreach (Iteration iteration in iterations)
        {
            await moveAsyncFunc(projectId, name, iteration.Id);
            if (iteration.Children.Count > 0) 
            {
                await MoveAsync(projectId, iteration.Children, $"{name}/{iteration.Name}", moveAsyncFunc);
            }
        }
    }

    public async Task MoveIterationAsync(Guid projectId, List<Iteration> iterations, string name)
    {
        await MoveAsync(projectId, iterations, name, MoveIterationAsync);
    }

    public async Task MoveAreaAsync(Guid projectId, List<Iteration> iterations, string name)
    {
        await MoveAsync(projectId, iterations, name, MoveAreaAsync);
    }
}
