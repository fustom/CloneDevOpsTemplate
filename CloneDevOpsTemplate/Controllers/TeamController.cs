using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class TeamController(ITeamsService teamsService) : Controller
{
    private readonly ITeamsService _teamsService = teamsService;

    async public Task<IActionResult> Teams()
    {
        Teams teams = await _teamsService.GetAllTeamsAsync() ?? new Teams();
        return View(teams.Value);
    }

    async public Task<IActionResult> Team(Guid projectId, string teamId)
    {
        Team team = await _teamsService.GetTeamAsync(projectId, teamId) ?? new Team();
        return View(team);
    }

    async public Task<IActionResult> ProjectTeams(Guid projectId)
    {
        Teams teams = await _teamsService.GetTeamsAsync(projectId) ?? new Teams();
        return View("Teams", teams.Value);
    }
}