using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface IIterationService
{
    Task<Iteration> GetIterationAsync(Guid projectId, string name);
    Task<Iteration> GetIterationsAsync(Guid projectId);
    Task<Iteration> CreateIterationAsync(Guid projectId, CreateIterationRequest iteration);
    Task<Iteration> CreateIterationAsync(Guid projectId, string projectName, Iteration iterations);
    Task MoveIteration(Guid projectId, Iteration iterations);
}
