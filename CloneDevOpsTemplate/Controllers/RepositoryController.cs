using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class RepositoryController(IRepositoryService repositoryService) : Controller
{
    private readonly IRepositoryService _repositoryService = repositoryService;

    async public Task<IActionResult> Repositories()
    {
        Repositories repositories = await _repositoryService.GetAllRepositoriesAsync() ?? new();
        return View(repositories.Value);
    }

    async public Task<IActionResult> ProjectRepositories(Guid projectId)
    {
        Repositories repositories = await _repositoryService.GetRepositoriesAsync(projectId) ?? new();
        return View("Repositories", repositories.Value);
    }
}