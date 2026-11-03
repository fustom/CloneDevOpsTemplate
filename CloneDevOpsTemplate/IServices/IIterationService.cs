using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface IIterationService
{
    Task<Iteration?> GetAsync(Guid projectId, TreeStructureGroup structureGroup, string name);
    Task<Iteration?> GetAsync(Guid projectId, TreeStructureGroup structureGroup, int depth);
    Task<Iteration?> GetAllAsync(Guid projectId, TreeStructureGroup structureGroup);
    Task<HttpResponseMessage> CreateAsync(Guid projectId, TreeStructureGroup structureGroup, ClassificationNodeBase classificationNode, string path);
    Task<CloneResult> CreateAsync(Guid projectId, Iteration classificationNodes, TreeStructureGroup structureGroup, string path);
    Task<HttpResponseMessage> DeleteAsync(Guid projectId, TreeStructureGroup structureGroup, string name);
    Task<CloneResult> DeleteAsync(Guid projectId, TreeStructureGroup structureGroup, Iteration classificationNodes);
}
