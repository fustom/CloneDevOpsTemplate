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

        var cloneProjectResult = await _cloneManager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility);

        if (cloneProjectResult.ErrorMessage is not null)
        {
            ModelState.AddModelError("ErrorMessage", cloneProjectResult.ErrorMessage);
            return await CreateProject();
        }

        await Task.WhenAll(
            _cloneManager.CloneClassificationNodes(templateProjectId, cloneProjectResult.Project.Id, TreeStructureGroup.Iterations),
            _cloneManager.CloneClassificationNodes(templateProjectId, cloneProjectResult.Project.Id, TreeStructureGroup.Areas)
        );
        await Task.WhenAll(
            _cloneManager.CloneTeamsAndSettingsAndBoardsAsync(cloneProjectResult.TemplateProject, cloneProjectResult.Project),
            _cloneManager.CloneRepositoriesAsync(templateProjectId, cloneProjectResult.Project.Id)
        );

        await _cloneManager.CloneGitPullRequestsAsync(templateProjectId, cloneProjectResult.Project.Id);

        return RedirectToAction("Project", new { projectId = cloneProjectResult.Project.Id });
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

        var cloneProjectResult = await _cloneManager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility);
        if (cloneProjectResult.ErrorMessage is not null)
        {
            ModelState.AddModelError("ErrorMessage", cloneProjectResult.ErrorMessage);
        }
        else
        {
            ViewBag.SuccessMessage = "Success";
        }

        return await Projects();
    }
}