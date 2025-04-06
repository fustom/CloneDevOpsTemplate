using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface IIterationService
{
    Task<Iteration?> GetAsync(Guid projectId, TreeStructureGroup structureGroup, string name);
    Task<Iteration?> GetAsync(Guid projectId, TreeStructureGroup structureGroup, int depth);
    Task<Iteration?> GetAllAsync(Guid projectId, TreeStructureGroup structureGroup);
    Task<Iteration> CreateAsync(Guid projectId, Iteration iterations, TreeStructureGroup structureGroup);
    Task MoveAsync(Guid projectId, List<Iteration> iterations, TreeStructureGroup structureGroup, string name);
}
