using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class IterationController(IIterationService iterationService) : Controller
{
    private readonly IIterationService _iterationService = iterationService;

    async public Task<IActionResult> Iterations(Guid projectId)
    {
        Iteration iterations = await _iterationService.GetIterationsAsync(projectId) ?? new();
        return View(iterations);
    }

    [HttpPost]
    async public Task<IActionResult> CreateIteration(Guid projectId, CreateIterationRequest iteration)
    {
        await _iterationService.CreateIterationAsync(projectId, iteration);
        return RedirectToAction("Iterations", new { projectId });
    }
}