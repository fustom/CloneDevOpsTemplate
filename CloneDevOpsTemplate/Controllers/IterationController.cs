using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class IterationController(IIterationService iterationService) : Controller
{
    private readonly IIterationService _iterationService = iterationService;

    public async Task<IActionResult> Iterations(Guid projectId)
    {
        if (!ModelState.IsValid)
        {
            return View(new Iteration());
        }
        Iteration iterations = await _iterationService.GetIterationsAsync(projectId) ?? new();
        return View(iterations);
    }

    [HttpPost]
    public async Task<IActionResult> CreateIteration(Guid projectId, CreateIterationRequest iteration)
    {
        await _iterationService.CreateIterationAsync(projectId, iteration);
        return RedirectToAction("Iterations", new { projectId });
    }

    public async Task<IActionResult> Areas(Guid projectId)
    {
        if (!ModelState.IsValid)
        {
            return View(new Iteration());
        }
        Iteration areas = await _iterationService.GetAreaAsync(projectId) ?? new();
        return View(areas);
    }
}