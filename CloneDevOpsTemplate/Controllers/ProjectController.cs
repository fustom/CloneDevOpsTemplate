using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class ProjectController(IProjectService projectService, ICloneManager cloneManager, IMigrationProgressTracker progressTracker) : Controller
{
    private readonly IProjectService _projectService = projectService;
    private readonly ICloneManager _cloneManager = cloneManager;
    private readonly IMigrationProgressTracker _progressTracker = progressTracker;

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
    public async Task<IActionResult> CreateProject(Guid templateProjectId, string newProjectName, string description, Visibility visibility, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await CreateProject();
        }

        try
        {
            var details = _progressTracker.CreateDetailReporter(operationId);
            CloneProjectResult cloneProjectResult = null!;

            (string Message, Func<Task> Run)[] steps =
            [
                ("Creating project...", async () =>
                    cloneProjectResult = await _cloneManager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility, details)),
                ("Cloning iterations and areas...", () => Task.WhenAll(
                    _cloneManager.CloneClassificationNodes(templateProjectId, cloneProjectResult.Project.Id, TreeStructureGroup.Iterations, details),
                    _cloneManager.CloneClassificationNodes(templateProjectId, cloneProjectResult.Project.Id, TreeStructureGroup.Areas, details))),
                ("Cloning teams, settings, boards and repositories...", () => Task.WhenAll(
                    _cloneManager.CloneTeamsAndSettingsAndBoardsAsync(cloneProjectResult.TemplateProject, cloneProjectResult.Project, details),
                    _cloneManager.CloneRepositoriesAsync(templateProjectId, cloneProjectResult.Project.Id, details))),
                ("Cloning pull requests...", () =>
                    _cloneManager.CloneGitPullRequestsAsync(templateProjectId, cloneProjectResult.Project.Id, details)),
            ];

            _progressTracker.Start(operationId, steps.Length);
            for (int i = 0; i < steps.Length; i++)
            {
                _progressTracker.Report(operationId, i + 1, steps[i].Message);
                await steps[i].Run();

                if (cloneProjectResult.ErrorMessage is not null)
                {
                    ModelState.AddModelError("ErrorMessage", cloneProjectResult.ErrorMessage);
                    return await CreateProject();
                }
            }

            return RedirectToAction("Project", new { projectId = cloneProjectResult.Project.Id });
        }
        finally
        {
            _progressTracker.Remove(operationId);
        }
    }

    [HttpGet]
    public IActionResult MigrationProgress(Guid operationId)
    {
        var progress = _progressTracker.Get(operationId);
        return progress is null ? NoContent() : Json(progress);
    }

    [HttpGet]
    public async Task<IActionResult> CloneProject()
    {
        return await Projects();
    }

    [HttpPost]
    public async Task<IActionResult> CloneProject(Guid templateProjectId, string newProjectName, string description, Visibility visibility, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await Projects();
        }

        _progressTracker.Start(operationId, 1);
        _progressTracker.Report(operationId, 1, "Creating project...");
        CloneProjectResult cloneProjectResult;
        try
        {
            cloneProjectResult = await _cloneManager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility, _progressTracker.CreateDetailReporter(operationId));
        }
        finally
        {
            _progressTracker.Remove(operationId);
        }
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