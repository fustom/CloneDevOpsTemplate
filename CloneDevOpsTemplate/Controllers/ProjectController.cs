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
    public async Task<IActionResult> CreateProject(Guid operationId = default)
    {
        return await ProjectFormView(nameof(CreateProject), _progressTracker.TakeFinished(operationId));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject(Guid templateProjectId, string newProjectName, string description, Visibility visibility, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await ProjectFormView(nameof(CreateProject));
        }

        operationId = MigrationProgressTrackerExtensions.EnsureOperationId(operationId);
        var details = _progressTracker.CreateDetailReporter(operationId);
        CloneProjectResult cloneProjectResult = null!;

        (string Message, Func<Task<CloneResult>> Run)[] steps =
        [
            ("Creating project...", async () =>
            {
                cloneProjectResult = await _cloneManager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility, details);
                return CloneResult.FromError(cloneProjectResult.ErrorMessage);
            }),
            ("Cloning iterations and areas...", async () => CloneResult.Combine(await Task.WhenAll(
                _cloneManager.CloneClassificationNodes(templateProjectId, cloneProjectResult.Project.Id, TreeStructureGroup.Iterations, details),
                _cloneManager.CloneClassificationNodes(templateProjectId, cloneProjectResult.Project.Id, TreeStructureGroup.Areas, details)))),
            ("Cloning teams, settings, boards and repositories...", async () => CloneResult.Combine(await Task.WhenAll(
                _cloneManager.CloneTeamsAndSettingsAndBoardsAsync(cloneProjectResult.TemplateProject, cloneProjectResult.Project, details),
                _cloneManager.CloneRepositoriesAsync(templateProjectId, cloneProjectResult.Project.Id, details)))),
            ("Cloning pull requests...", () =>
                _cloneManager.CloneGitPullRequestsAsync(templateProjectId, cloneProjectResult.Project.Id, details)),
        ];

        try
        {
            _progressTracker.Start(operationId, steps.Length);
            for (int i = 0; i < steps.Length; i++)
            {
                _progressTracker.Report(operationId, i + 1, steps[i].Message);
                var stepResult = await steps[i].Run();

                if (!stepResult.Succeeded)
                {
                    _progressTracker.Fail(operationId, $"{steps[i].Message.TrimEnd('.')} failed: {stepResult.ErrorMessage}");
                    return RedirectToAction(nameof(CreateProject), new { operationId });
                }
            }
        }
        catch
        {
            _progressTracker.Remove(operationId);
            throw;
        }

        _progressTracker.Remove(operationId);
        return RedirectToAction("Project", new { projectId = cloneProjectResult.Project.Id });
    }

    [HttpGet]
    public IActionResult MigrationProgress(Guid operationId)
    {
        var progress = _progressTracker.Get(operationId);
        return progress is null ? NoContent() : Json(progress);
    }

    [HttpGet]
    public async Task<IActionResult> CloneProject(Guid operationId = default)
    {
        return await ProjectFormView(nameof(CloneProject), _progressTracker.TakeFinished(operationId));
    }

    [HttpPost]
    public async Task<IActionResult> CloneProject(Guid templateProjectId, string newProjectName, string description, Visibility visibility, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await ProjectFormView(nameof(CloneProject));
        }

        operationId = await _progressTracker.RunAsync(operationId, "Creating project...", $"Project '{newProjectName}' cloned successfully.",
            async progress =>
            {
                var cloneProjectResult = await _cloneManager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility, progress);
                return CloneResult.FromError(cloneProjectResult.ErrorMessage);
            });
        return RedirectToAction(nameof(CloneProject), new { operationId });
    }

    private async Task<ViewResult> ProjectFormView(string action, MigrationProgress? finishedMigration = null)
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new();
        return View(action, new CloneFormViewModel<ProjectBase>(projects.Value, "Project", action, finishedMigration));
    }
}
