using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
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

    public async Task<IActionResult> TestSuites(Guid projectId, int testPlanId)
    {
        TestSuites testSuites = new();

        if (!ModelState.IsValid)
        {
            return View(testSuites.Value);
        }

        testSuites = await _testService.GetTestSuitesAsync(projectId, testPlanId) ?? new();
        return View(testSuites.Value);
    }

    public async Task<IActionResult> TestCases(Guid projectId, int testPlanId, int testSuiteId)
    {
        TestCases testCases = new();

        if (!ModelState.IsValid)
        {
            return View(testCases.Value);
        }

        testCases = await _testService.GetTestCasesAsync(projectId, testPlanId, testSuiteId) ?? new();
        return View(testCases.Value);
    }
}
