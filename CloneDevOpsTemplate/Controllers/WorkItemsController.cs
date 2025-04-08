using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class WorkItemsController(IWorkItemService workItemService) : Controller
{
    private readonly IWorkItemService _workItemService = workItemService;

    public async Task<IActionResult> WorkItems(Guid projectId, string projectName)
    {
        List<WorkItem> workItems = [];

        if (!ModelState.IsValid)
        {
            return View(new Tuple<Guid, WorkItem[]>(projectId, [.. workItems]));
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
        return View(new Tuple<Guid, WorkItem[]>(projectId, [.. workItems]));
    }

    public async Task<IActionResult> WorkItem(Guid projectId, int workitemId)
    {
        WorkItem workItem = new();

        if (!ModelState.IsValid)
        {
            return View(workItem);
        }

        workItem = await _workItemService.GetWorkItemAsync(projectId, workitemId) ?? new();
        return View(workItem);
    }
}

