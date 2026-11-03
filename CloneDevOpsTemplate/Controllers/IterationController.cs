using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class IterationController(IIterationService iterationService, ICloneManager cloneManager, IProjectService projectService, IMigrationProgressTracker progressTracker) : Controller
{
    private readonly IIterationService _iterationService = iterationService;
    private readonly ICloneManager _cloneManager = cloneManager;
    private readonly IProjectService _projectService = projectService;
    private readonly IMigrationProgressTracker _progressTracker = progressTracker;

    public async Task<IActionResult> Iterations(Guid projectId)
    {
        Iteration iterations = new();

        if (!ModelState.IsValid)
        {
            return View(iterations);
        }

        iterations = await _iterationService.GetAllAsync(projectId, TreeStructureGroup.Iterations) ?? new();
        return View(iterations);
    }

    public async Task<IActionResult> Areas(Guid projectId)
    {
        Iteration areas = new();

        if (!ModelState.IsValid)
        {
            return View(areas);
        }

        areas = await _iterationService.GetAllAsync(projectId, TreeStructureGroup.Areas) ?? new();
        return View(areas);
    }

    [HttpGet]
    public async Task<IActionResult> CloneIterations(Guid operationId = default)
    {
        return await CloneFormView(nameof(CloneIterations), _progressTracker.TakeFinished(operationId));
    }

    [HttpPost]
    public async Task<IActionResult> CloneIterations(Guid templateProjectId, Guid projectId, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await CloneFormView(nameof(CloneIterations));
        }

        operationId = await _progressTracker.RunAsync(operationId, "Cloning iterations...", "Iterations cloned successfully.",
            progress => _cloneManager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations, progress));
        return RedirectToAction(nameof(CloneIterations), new { operationId });
    }

    [HttpGet]
    public async Task<IActionResult> CloneAreas(Guid operationId = default)
    {
        return await CloneFormView(nameof(CloneAreas), _progressTracker.TakeFinished(operationId));
    }

    [HttpPost]
    public async Task<IActionResult> CloneAreas(Guid templateProjectId, Guid projectId, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await CloneFormView(nameof(CloneAreas));
        }

        operationId = await _progressTracker.RunAsync(operationId, "Cloning areas...", "Areas cloned successfully.",
            progress => _cloneManager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Areas, progress));
        return RedirectToAction(nameof(CloneAreas), new { operationId });
    }

    private async Task<ViewResult> CloneFormView(string action, MigrationProgress? finishedMigration = null)
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new();
        return View(action, new CloneFormViewModel<ProjectBase>(projects.Value, "Iteration", action, finishedMigration));
    }
}
