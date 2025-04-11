using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class TestController(ITestService testService) : Controller
{
    private readonly ITestService _testService = testService;

    public async Task<IActionResult> TestPlans(Guid projectId)
    {
        TestPlans testPlans = new();

        if (!ModelState.IsValid)
        {
            return View(testPlans.Value);
        }

        testPlans = await _testService.GetTestPlansAsync(projectId) ?? new();
        return View(testPlans.Value);
    }
}
