using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class TeamSettingsController(ITeamSettingsService teamSettingsService) : Controller
{
    private readonly ITeamSettingsService _teamSettingsService = teamSettingsService;

    async public Task<IActionResult> TeamSettings(Guid projectId, Guid teamId)
    {
        TeamSettings teamSettings = await _teamSettingsService.GetTeamSettings(projectId, teamId) ?? new TeamSettings();
        return View(teamSettings);
    }
}
