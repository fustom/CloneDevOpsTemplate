using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class RepositoryController(IRepositoryService repositoryService) : Controller
{
    private readonly IRepositoryService _repositoryService = repositoryService;

    public async Task<IActionResult> Repositories()
    {
        Repositories repositories = await _repositoryService.GetAllRepositoriesAsync() ?? new();
        return View(repositories.Value);
    }

    public async Task<IActionResult> ProjectRepositories(Guid projectId)
    {
        Repositories repositories = await _repositoryService.GetRepositoriesAsync(projectId) ?? new();
        return View("Repositories", repositories.Value);
    }
}