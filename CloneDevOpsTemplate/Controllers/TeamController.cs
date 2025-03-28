using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class TeamController(ITeamsService teamsService) : Controller
{
    private readonly ITeamsService _teamsService = teamsService;

    public async Task<IActionResult> Teams()
    {
        Teams teams = await _teamsService.GetAllTeamsAsync() ?? new Teams();
        return View(teams.Value);
    }

    public async Task<IActionResult> Team(Guid projectId, Guid teamId)
    {
        Team team = await _teamsService.GetTeamAsync(projectId, teamId) ?? new Team();
        return View(team);
    }

    public async Task<IActionResult> ProjectTeams(Guid projectId)
    {
        Teams teams = await _teamsService.GetTeamsAsync(projectId) ?? new Teams();
        return View("Teams", teams.Value);
    }
}