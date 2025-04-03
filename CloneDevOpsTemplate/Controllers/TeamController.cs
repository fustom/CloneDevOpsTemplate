using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class TeamController(ITeamsService teamsService, ICloneManager cloneManager, IProjectService projectService) : Controller
{
    private readonly ITeamsService _teamsService = teamsService;
    private readonly ICloneManager _cloneManager = cloneManager;
    private readonly IProjectService _projectService = projectService;

    public async Task<IActionResult> Teams()
    {
        Teams teams = await _teamsService.GetAllTeamsAsync() ?? new();
        return View(teams.Value);
    }

    public async Task<IActionResult> Team(Guid projectId, Guid teamId)
    {
        Team team = new();

        if (!ModelState.IsValid)
        {
            return View(team);
        }

        team = await _teamsService.GetTeamAsync(projectId, teamId) ?? new();
        return View(team);
    }

    public async Task<IActionResult> ProjectTeams(Guid projectId)
    {
        Teams teams = new();

        if (!ModelState.IsValid)
        {
            return View("Teams", teams.Value);
        }

        teams = await _teamsService.GetTeamsAsync(projectId) ?? new();
        return View("Teams", teams.Value);
    }

    [HttpGet]
    public async Task<IActionResult> CloneTeams()
    {
        var projects = await _projectService.GetAllProjectsAsync() ?? new();
        return View(projects.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CloneTeams(Guid templateProjectId, Guid projectId)
    {
        if (!ModelState.IsValid)
        {
            return await CloneTeams();
        }

        await _cloneManager.CloneTeamsAsync(templateProjectId, projectId);
        ViewBag.SuccessMessage = "Success";

        return await CloneTeams();
    }

    [HttpGet]
    public async Task<IActionResult> CloneTeamFieldValues()
    {
        return await Teams();
    }

    [HttpPost]
    public async Task<IActionResult> CloneTeamFieldValues(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId)
    {
        if (!ModelState.IsValid)
        {
            return await CloneTeamFieldValues();
        }

        await _cloneManager.CloneTeamFieldValuesAsync(templateProjectId, projectId, templateTeamId, projectTeamId);
        ViewBag.SuccessMessage = "Success";

        return await CloneTeamFieldValues();
    }
}