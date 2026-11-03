using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class TeamSettingsController(ITeamSettingsService teamSettingsService, ICloneManager cloneManager, ITeamsService teamsService, IMigrationProgressTracker progressTracker) : Controller
{
    private readonly ITeamSettingsService _teamSettingsService = teamSettingsService;
    private readonly ICloneManager _cloneManager = cloneManager;
    private readonly ITeamsService _teamsService = teamsService;
    private readonly IMigrationProgressTracker _progressTracker = progressTracker;

    public async Task<IActionResult> TeamSettings(Guid projectId, Guid teamId)
    {
        TeamSettings teamSettings = new();

        if (!ModelState.IsValid)
        {
            return View(teamSettings);
        }

        teamSettings = await _teamSettingsService.GetTeamSettings(projectId, teamId) ?? new();
        return View(teamSettings);
    }

    public async Task<IActionResult> TeamFieldValues(Guid projectId, Guid teamId)
    {
        TeamFieldValues teamFieldValues = new();

        if (!ModelState.IsValid)
        {
            return View(teamFieldValues);
        }

        teamFieldValues = await _teamSettingsService.GetTeamFieldValues(projectId, teamId) ?? new();
        return View(teamFieldValues);
    }

    [HttpGet]
    public async Task<IActionResult> Iterations(Guid projectId, Guid teamId)
    {
        TeamIterations iterations = new();

        if (!ModelState.IsValid)
        {
            return View(iterations.Value);
        }

        iterations = await _teamSettingsService.GetIterations(projectId, teamId) ?? new();
        return View(iterations.Value);
    }

    [HttpGet]
    public async Task<IActionResult> CloneTeamSettings(Guid operationId = default)
    {
        return await CloneFormView(nameof(CloneTeamSettings), _progressTracker.TakeFinished(operationId));
    }

    [HttpPost]
    public async Task<IActionResult> CloneTeamSettings(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await CloneFormView(nameof(CloneTeamSettings));
        }

        operationId = await _progressTracker.RunAsync(operationId, "Cloning team settings...", "Team settings cloned successfully.",
            progress => _cloneManager.CloneTeamSettingsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, progress));
        return RedirectToAction(nameof(CloneTeamSettings), new { operationId });
    }

    [HttpGet]
    public async Task<IActionResult> CloneTeamIterations(Guid operationId = default)
    {
        return await CloneFormView(nameof(CloneTeamIterations), _progressTracker.TakeFinished(operationId));
    }

    [HttpPost]
    public async Task<IActionResult> CloneTeamIterations(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await CloneFormView(nameof(CloneTeamIterations));
        }

        operationId = await _progressTracker.RunAsync(operationId, "Cloning team iterations...", "Team iterations cloned successfully.",
            progress => _cloneManager.CloneTeamIterationsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, progress));
        return RedirectToAction(nameof(CloneTeamIterations), new { operationId });
    }

    private async Task<ViewResult> CloneFormView(string action, MigrationProgress? finishedMigration = null)
    {
        Teams teams = await _teamsService.GetAllTeamsAsync() ?? new();
        return View(action, new CloneFormViewModel<Team>(teams.Value, "TeamSettings", action, finishedMigration));
    }
}
