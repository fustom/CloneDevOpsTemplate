using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class RepositoryController(IRepositoryService repositoryService, ICloneManager cloneManager, IProjectService projectService) : Controller
{
    private readonly IRepositoryService _repositoryService = repositoryService;
    private readonly ICloneManager _cloneManager = cloneManager;
    private readonly IProjectService _projectService = projectService;

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

        pullRequests = await _repositoryService.GetGitPullRequest(projectId) ?? new();
        return View(pullRequests.Value);
    }

    [HttpGet]
    public async Task<IActionResult> CloneRepository()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new();
        return View(projects.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CloneRepository(Guid templateProjectId, Guid projectId)
    {
        if (!ModelState.IsValid)
        {
            return await CloneRepository();
        }

        await _cloneManager.CloneRepositoriesAsync(templateProjectId, projectId);
        ViewBag.SuccessMessage = "Success";

        return await CloneRepository();
    }
}