using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class ProjectController(IProjectService projectService, ICloneManager cloneManager) : Controller
{
    private readonly IProjectService _projectService = projectService;
    private readonly ICloneManager _cloneManager = cloneManager;

    [HttpGet]
    public async Task<IActionResult> Projects()
    {
        Projects projects = new();

        if (!ModelState.IsValid)
        {
            return View(projects.Value);
        }
        
        projects = await _projectService.GetAllProjectsAsync() ?? new Projects();
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

        project = await _projectService.GetProjectAsync(projectId) ?? new Project();
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

        projectProperties = await _projectService.GetProjectPropertiesAsync(projectId) ?? new ProjectProperties();
        return View(projectProperties.Value);
    }

    [HttpGet]
    public async Task<IActionResult> CreateProject()
    {
        return await Projects();
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject(Guid templateProjectId, string newProjectName, string description, string visibility)
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
            _cloneManager.CloneIterationsAsync(templateProjectId, project.Id),
            _cloneManager.CloneAreasAsync(templateProjectId, project.Id)
        );
        await Task.WhenAll(
            _cloneManager.CloneTeamsAndSettingsAndBoardsAsync(templateProject, project),
            _cloneManager.CloneRepositoriesAsync(templateProjectId, project.Id)
        );

        return RedirectToAction("Project", new { projectId = project.Id });
    }
}