using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class TeamSettingsController(ITeamSettingsService teamSettingsService) : Controller
{
    private readonly ITeamSettingsService _teamSettingsService = teamSettingsService;

    public async Task<IActionResult> TeamSettings(Guid projectId, Guid teamId)
    {
        TeamSettings teamSettings = new();

        if (!ModelState.IsValid)
        {
            return View(teamSettings);
        }

        teamSettings = await _teamSettingsService.GetTeamSettings(projectId, teamId) ?? new TeamSettings();
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

}
