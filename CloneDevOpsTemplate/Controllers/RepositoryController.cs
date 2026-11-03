using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class RepositoryController(IRepositoryService repositoryService, ICloneManager cloneManager, IProjectService projectService, IMigrationProgressTracker progressTracker) : Controller
{
    private readonly IRepositoryService _repositoryService = repositoryService;
    private readonly ICloneManager _cloneManager = cloneManager;
    private readonly IProjectService _projectService = projectService;
    private readonly IMigrationProgressTracker _progressTracker = progressTracker;

    public async Task<IActionResult> Repositories()
    {
        Repositories repositories = new();

        if (!ModelState.IsValid)
        {
            return View(repositories.Value);
        }

        repositories = await _repositoryService.GetAllRepositoriesAsync() ?? new();
        return View(repositories.Value);
    }

    public async Task<IActionResult> ProjectRepositories(Guid projectId)
    {
        Repositories repositories = new();

        if (!ModelState.IsValid)
        {
            return View("Repositories", repositories.Value);
        }

        repositories = await _repositoryService.GetRepositoriesAsync(projectId) ?? new();
        return View("Repositories", repositories.Value);
    }

    public async Task<IActionResult> PullRequests(Guid projectId)
    {
        GitPullRequests pullRequests = new();

        if (!ModelState.IsValid)
        {
            return View(pullRequests.Value);
        }

        pullRequests = await _repositoryService.GetGitPullRequestAsync(projectId) ?? new();
        return View(pullRequests.Value);
    }

    [HttpGet]
    public async Task<IActionResult> CloneRepository(Guid operationId = default)
    {
        return await CloneFormView(nameof(CloneRepository), _progressTracker.TakeFinished(operationId));
    }

    [HttpPost]
    public async Task<IActionResult> CloneRepository(Guid templateProjectId, Guid projectId, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await CloneFormView(nameof(CloneRepository));
        }

        operationId = await _progressTracker.RunAsync(operationId, "Cloning repositories...", "Repositories cloned successfully.",
            progress => _cloneManager.CloneRepositoriesAsync(templateProjectId, projectId, progress));
        return RedirectToAction(nameof(CloneRepository), new { operationId });
    }

    [HttpGet]
    public async Task<IActionResult> ClonePullRequests(Guid operationId = default)
    {
        return await CloneFormView(nameof(ClonePullRequests), _progressTracker.TakeFinished(operationId));
    }

    [HttpPost]
    public async Task<IActionResult> ClonePullRequests(Guid templateProjectId, Guid projectId, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await CloneFormView(nameof(ClonePullRequests));
        }

        operationId = await _progressTracker.RunAsync(operationId, "Cloning pull requests...", "Pull requests cloned successfully.",
            progress => _cloneManager.CloneGitPullRequestsAsync(templateProjectId, projectId, progress));
        return RedirectToAction(nameof(ClonePullRequests), new { operationId });
    }

    private async Task<ViewResult> CloneFormView(string action, MigrationProgress? finishedMigration = null)
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new();
        return View(action, new CloneFormViewModel<ProjectBase>(projects.Value, "Repository", action, finishedMigration));
    }
}
