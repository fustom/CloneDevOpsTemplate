using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class IterationController(IIterationService iterationService) : Controller
{
    private readonly IIterationService _iterationService = iterationService;

    public async Task<IActionResult> Iterations(Guid projectId)
    {
        Iteration iterations = new();

        if (!ModelState.IsValid)
        {
            return View(iterations);
        }

        iterations = await _iterationService.GetIterationsAsync(projectId) ?? new();
        return View(iterations);
    }

    public async Task<IActionResult> Areas(Guid projectId)
    {
        Iteration areas = new();

        if (!ModelState.IsValid)
        {
            return View(areas);
        }

        areas = await _iterationService.GetAreaAsync(projectId) ?? new();
        return View(areas);
    }
}