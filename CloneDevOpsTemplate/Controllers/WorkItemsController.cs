using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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

        WorkItemsListQueryResult workItemsList = await _workItemService.GetWorkItemsListAsync(projectId, projectName) ?? new();
        var workItemIdBatches = workItemsList.WorkItems
            .Select(x => x.Id)
            .Chunk(200);

        foreach (var workItemIdBatch in workItemIdBatches)
        {
            var currentWorkItems = await _workItemService.GetWorkItemsAsync(projectId, workItemIdBatch);
            if (currentWorkItems != null)
            {
                workItems.AddRange(currentWorkItems.Value);
            }
        }
        return View(workItems.ToArray());
    }
}

