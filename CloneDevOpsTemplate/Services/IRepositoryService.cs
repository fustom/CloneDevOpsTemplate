using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface IRepositoryService
{
    Task<Repositories?> GetAllRepositoriesAsync();
    Task<Repositories?> GetRepositoriesAsync(Guid projectId);
    Task<Repository?> CreateRepositoryAsync(Guid projectId, string name);
    Task DeleteRepositoryAsync(Guid projectId, Guid repositoryId);
    Task CreateImportRequest(Guid projectId, Guid repositoryId, string sourceRepositoryRemoteUrl, Guid serviceEndpointId);
}
