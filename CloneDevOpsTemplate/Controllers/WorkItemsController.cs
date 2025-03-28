using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class WorkItemsController(IWorkItemService workItemService) : Controller
{
    private readonly IWorkItemService _workItemService = workItemService;

    public async Task<IActionResult> WorkItems(Guid projectId, string projectName)
    {
        List<WorkItem> workItems = [];

        if (!ModelState.IsValid)
        {
            return View(workItems.ToArray());
        }

        WorkItemQueryList workItemQueryList = await _workItemService.GetWorkItemsAsync(projectId, projectName) ?? new();
        foreach (var workItem in workItemQueryList.WorkItems)
        {
            var currentWorkItem = await _workItemService.GetWorkItemAsync(projectId, workItem.Id) ?? new();
            workItems.Add(currentWorkItem);
        }
        return View(workItems.ToArray());
    }
}

