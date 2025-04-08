using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface IRepositoryService
{
    Task<Repositories?> GetAllRepositoriesAsync();
    Task<Repositories?> GetRepositoriesAsync(Guid projectId);
    Task<Repository?> CreateRepositoryAsync(Guid projectId, string name);
    Task<HttpResponseMessage> DeleteRepositoryAsync(Guid projectId, Guid repositoryId);
    Task<GitImportRequest?> CreateImportRequestAsync(Guid projectId, Guid repositoryId, string sourceRepositoryRemoteUrl, Guid serviceEndpointId);
    Task<GitImportRequest?> GetImportRequestAsync(Guid projectId, Guid repositoryId, int importRequestId);
}
