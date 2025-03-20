using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class RepositoryService(IHttpClientFactory httpClientFactory) : IRepositoryService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");
    public Task<Repositories?> GetAllRepositoriesAsync()
    {
        return _client.GetFromJsonAsync<Repositories>($"_apis/git/repositories");
    }
}