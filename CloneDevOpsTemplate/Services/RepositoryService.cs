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

    public async Task<GitImportRequest?> CreateImportRequestAsync(Guid projectId, Guid repositoryId, string sourceRepositoryRemoteUrl, Guid serviceEndpointId)
    {
        GitImportRequestBase importRequest = new()
        {
            Parameters =
            {
                GitSource =
                {
                    Url = sourceRepositoryRemoteUrl
                },
                ServiceEndpointId = serviceEndpointId,
                DeleteServiceEndpointAfterImportIsDone = true
            }
        };
        var result = await _client.PostAsJsonAsync($"{projectId}/_apis/git/repositories/{repositoryId}/importRequests?api-version=7.1", importRequest);
        return await result.Content.ReadFromJsonAsync<GitImportRequest>();
    }

    public Task<GitImportRequest?> GetImportRequestAsync(Guid projectId, Guid repositoryId, int importRequestId)
    {
        return _client.GetFromJsonAsync<GitImportRequest>($"{projectId}/_apis/git/repositories/{repositoryId}/importRequests/{importRequestId}");
    }

    public Task<GitPullRequests?> GetGitPullRequestAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<GitPullRequests>($"{projectId}/_apis/git/pullrequests");
    }

    public Task<HttpResponseMessage> CreateGitPullRequestAsync(Guid projectId, Guid repositoryId, GitPullRequestBase pullRequest)
    {
        return _client.PostAsJsonAsync($"{projectId}/_apis/git/repositories/{repositoryId}/pullrequests?api-version=7.1", pullRequest);
    }
}