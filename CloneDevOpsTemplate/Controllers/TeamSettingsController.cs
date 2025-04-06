using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class TeamSettingsController(ITeamSettingsService teamSettingsService, ICloneManager cloneManager, ITeamsService teamsService) : Controller
{
    private readonly ITeamSettingsService _teamSettingsService = teamSettingsService;
    private readonly ICloneManager _cloneManager = cloneManager;
    private readonly ITeamsService _teamsService = teamsService;

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
    public async Task<IActionResult> CloneTeamSettings()
    {
        var teams = await _teamsService.GetAllTeamsAsync() ?? new();
        return View(teams.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CloneTeamSettings(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId)
    {
        if (!ModelState.IsValid)
        {
            return await CloneTeamSettings();
        }

        await _cloneManager.CloneTeamSettingsAsync(templateProjectId, projectId, templateTeamId, projectTeamId);
        ViewBag.SuccessMessage = "Success";

        return await CloneTeamSettings();
    }

    [HttpGet]
    public async Task<IActionResult> CloneTeamIterations()
    {
        return await CloneTeamSettings();
    }

    [HttpPost]
    public async Task<IActionResult> CloneTeamIterations(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId)
    {
        if (!ModelState.IsValid)
        {
            return await CloneTeamIterations();
        }

        await _cloneManager.CloneTeamIterationsAsync(templateProjectId, projectId, templateTeamId, projectTeamId);
        ViewBag.SuccessMessage = "Success";

        return await CloneTeamIterations();
    }
}
