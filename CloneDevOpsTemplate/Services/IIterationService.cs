using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface IIterationService
{
    Task<Iteration?> GetIterationAsync(Guid projectId, Guid iterationId);
    Task<Iterations?> GetIterationsAsync(Guid projectId);
}
