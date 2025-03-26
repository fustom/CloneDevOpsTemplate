using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class TeamSettingsController(ITeamSettingsService teamSettingsService) : Controller
{
    private readonly ITeamSettingsService _teamSettingsService = teamSettingsService;

    public async Task<IActionResult> TeamSettings(Guid projectId, Guid teamId)
    {
        TeamSettings teamSettings = await _teamSettingsService.GetTeamSettings(projectId, teamId) ?? new TeamSettings();
        return View(teamSettings);
    }
}
