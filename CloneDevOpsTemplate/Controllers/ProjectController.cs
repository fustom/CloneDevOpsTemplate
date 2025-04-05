using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class ProjectController(IProjectService projectService, ICloneManager cloneManager) : Controller
{
    private readonly IProjectService _projectService = projectService;
    private readonly ICloneManager _cloneManager = cloneManager;

    [HttpGet]
    public async Task<IActionResult> Projects()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new();
        return View(projects.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Project(Guid projectId)
    {
        Project project = new();

        if (!ModelState.IsValid)
        {
            return View(project);
        }

        project = await _projectService.GetProjectAsync(projectId) ?? new();
        return View(project);
    }

    [HttpGet]
    public async Task<IActionResult> ProjectProperties(Guid projectId)
    {
        ProjectProperties projectProperties = new();

        if (!ModelState.IsValid)
        {
            return View(projectProperties.Value);
        }

        projectProperties = await _projectService.GetProjectPropertiesAsync(projectId) ?? new();
        return View(projectProperties.Value);
    }

    [HttpGet]
    public async Task<IActionResult> CreateProject()
    {
        return await Projects();
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject(Guid templateProjectId, string newProjectName, string description, Visibility visibility)
    {
        if (!ModelState.IsValid)
        {
            return await CreateProject();
        }

        (Project project, Project templateProject, string? errorMessage) = await _cloneManager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility);

        if (errorMessage is not null)
        {
            ModelState.AddModelError("ErrorMessage", errorMessage);
            return await CreateProject();
        }

        await Task.WhenAll(
            _cloneManager.CloneClassificationNodes(templateProjectId, project.Id, TreeStructureGroup.Iterations),
            _cloneManager.CloneClassificationNodes(templateProjectId, project.Id, TreeStructureGroup.Areas)
        );
        await Task.WhenAll(
            _cloneManager.CloneTeamsAndSettingsAndBoardsAsync(templateProject, project),
            _cloneManager.CloneRepositoriesAsync(templateProjectId, project.Id)
        );

        return RedirectToAction("Project", new { projectId = project.Id });
    }

    [HttpGet]
    public async Task<IActionResult> CloneProject()
    {
        return await Projects();
    }

    [HttpPost]
    public async Task<IActionResult> CloneProject(Guid templateProjectId, string newProjectName, string description, Visibility visibility)
    {
        if (!ModelState.IsValid)
        {
            return await Projects();
        }

        (_, _, string? message) = await _cloneManager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility);
        if (message is not null)
        {
            ModelState.AddModelError("ErrorMessage", message);
        }
        else
        {
            ViewBag.SuccessMessage = "Success";
        }

        return await Projects();
    }
}