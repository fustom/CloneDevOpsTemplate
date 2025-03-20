using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface IRepositoryService
{
    Task<Repositories?> GetAllRepositoriesAsync();
}
