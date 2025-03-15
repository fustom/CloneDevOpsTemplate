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

    public async Task<Iteration> CreateIterationAsync(Guid projectId, string projectName, Iteration iterations)
    {
        var response = new Iteration();

        foreach (Iteration iteration in iterations.Children)
        {
            Iteration resp = await CreateIterationAsync(projectId, new CreateIterationRequest
            {
                Name = iteration.Name
            });
            resp.Path = iteration.Path.Replace(iterations.Name, projectName);

            Iteration child = await CreateIterationAsync(projectId, projectName, iteration);
            if (child.Id > 0)
            {
                resp.Children.Add(child);
            }
            if (resp.Id > 0)
            {
                response.Children.Add(resp);
            }
        }

        return response;
    }

    public async Task MoveIteration(Guid projectId, Iteration iterations)
    {
        foreach (Iteration iteration in iterations.Children)
        {
            await _client.PostAsJsonAsync($"{projectId}/_apis/wit/classificationNodes/{iteration.Path}iterations?api-version=7.1", new { Id = iteration.Id });
        }
    }
}
