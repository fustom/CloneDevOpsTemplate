using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class RepositoryService(IHttpClientFactory httpClientFactory) : IRepositoryService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");
    public Task<Repositories?> GetAllRepositoriesAsync()
    {
        return _client.GetFromJsonAsync<Repositories>($"_apis/git/repositories");
    }
    public Task<Repositories?> GetRepositoriesAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<Repositories>($"{projectId}/_apis/git/repositories");
    }

    public async Task<Repository?> CreateRepositoryAsync(Guid projectId, string name)
    {
        var result = await _client.PostAsJsonAsync($"{projectId}/_apis/git/repositories?api-version=7.1", new { name });
        return await result.Content.ReadFromJsonAsync<Repository>();
    }

    public Task<HttpResponseMessage> DeleteRepositoryAsync(Guid projectId, Guid repositoryId)
    {
        return _client.DeleteAsync($"{projectId}/_apis/git/repositories/{repositoryId}?api-version=7.1");
    }

    public Task<HttpResponseMessage> CreateImportRequest(Guid projectId, Guid repositoryId, string sourceRepositoryRemoteUrl, Guid serviceEndpointId)
    {
        ImportRequest importRequest = new()
        {
            Parameters = new()
            {
                GitSource = new()
                {
                    Url = sourceRepositoryRemoteUrl
                },
                ServiceEndpointId = serviceEndpointId,
                DeleteServiceEndpointAfterImportIsDone = true
            }
        };
        return _client.PostAsJsonAsync($"{projectId}/_apis/git/repositories/{repositoryId}/importRequests?api-version=7.1", importRequest);
    }
}