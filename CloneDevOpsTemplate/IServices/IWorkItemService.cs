using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface IWorkItemService
{
    Task<WorkItemQueryList> GetWorkItemsAsync(Guid projectId, string projectName);
    Task<WorkItem?> GetWorkItemAsync(Guid projectId, int workItemId);
}