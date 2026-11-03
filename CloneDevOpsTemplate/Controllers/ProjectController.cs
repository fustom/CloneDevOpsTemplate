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

        operationId = EnsureOperationId(operationId);
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

        try
        {
            _progressTracker.Start(operationId, steps.Length);
            for (int i = 0; i < steps.Length; i++)
            {
                _progressTracker.Report(operationId, i + 1, steps[i].Message);
                await steps[i].Run();

                if (cloneProjectResult.ErrorMessage is not null)
                {
                    _progressTracker.Fail(operationId, cloneProjectResult.ErrorMessage);
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

        operationId = EnsureOperationId(operationId);
        _progressTracker.Start(operationId, 1);
        _progressTracker.Report(operationId, 1, "Creating project...");
        CloneProjectResult cloneProjectResult;
        try
        {
            cloneProjectResult = await _cloneManager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility, _progressTracker.CreateDetailReporter(operationId));
        }
        catch
        {
            _progressTracker.Remove(operationId);
            throw;
        }

        if (cloneProjectResult.ErrorMessage is not null)
        {
            _progressTracker.Fail(operationId, cloneProjectResult.ErrorMessage);
        }
        else
        {
            _progressTracker.Complete(operationId, $"Project '{newProjectName}' cloned successfully.");
        }

        return RedirectToAction(nameof(CloneProject), new { operationId });
    }

    // The result page reads the outcome from the tracker, so every POST needs an operation id.
    private static Guid EnsureOperationId(Guid operationId) => operationId == Guid.Empty ? Guid.NewGuid() : operationId;

    private async Task<ViewResult> ProjectFormView(string action, MigrationProgress? finishedMigration = null)
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new();
        return View(action, new CreateProjectFormViewModel(projects.Value, "Project", action, finishedMigration));
    }
}
