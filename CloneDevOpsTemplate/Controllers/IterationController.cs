using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class IterationController(IIterationService iterationService, ICloneManager cloneManager, IProjectService projectService) : Controller
{
    private readonly IIterationService _iterationService = iterationService;
    private readonly ICloneManager _cloneManager = cloneManager;
    private readonly IProjectService _projectService = projectService;

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
    public async Task<IActionResult> CloneIterations()
    {
        var projects = await _projectService.GetAllProjectsAsync() ?? new();
        return View(projects.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CloneIterations(Guid templateProjectId, Guid projectId)
    {
        if (!ModelState.IsValid)
        {
            return await CloneIterations();
        }

        await _cloneManager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations);
        ViewBag.SuccessMessage = "Success";

        return await CloneIterations();
    }

    [HttpGet]
    public async Task<IActionResult> CloneAreas()
    {
        return await CloneIterations();
    }

    [HttpPost]
    public async Task<IActionResult> CloneAreas(Guid templateProjectId, Guid projectId)
    {
        if (!ModelState.IsValid)
        {
            return await CloneIterations();
        }

        await _cloneManager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Areas);
        ViewBag.SuccessMessage = "Success";

        return await CloneIterations();
    }
}