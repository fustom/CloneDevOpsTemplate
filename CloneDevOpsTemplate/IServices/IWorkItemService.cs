using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface IWorkItemService
{
    Task<WorkItemsListQueryResult?> GetWorkItemsListAsync(Guid projectId, string projectName);
    Task<WorkItems?> GetWorkItemsAsync(Guid projectId, int[] workItemIds);
    Task<WorkItem?> GetWorkItemAsync(Guid projectId, int workItemId);
}