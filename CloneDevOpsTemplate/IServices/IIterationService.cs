using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface IIterationService
{
    Task<Iteration?> GetIterationAsync(Guid projectId, string name);
    Task<Iteration?> GetIterationsAsync(Guid projectId);
    Task<Iteration> CreateIterationAsync(Guid projectId, CreateIterationRequest iteration);
    Task<Iteration> CreateIterationAsync(Guid projectId, Iteration iterations);
    Task MoveIteration(Guid projectId, List<Iteration> iterations, string name = "");
    Task<HttpResponseMessage> MoveIteration(Guid projectId, string path, int Id);
}
