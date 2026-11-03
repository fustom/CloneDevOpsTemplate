using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class TeamController(ITeamsService teamsService, ICloneManager cloneManager, IProjectService projectService, IMigrationProgressTracker progressTracker) : Controller
{
    private readonly ITeamsService _teamsService = teamsService;
    private readonly ICloneManager _cloneManager = cloneManager;
    private readonly IProjectService _projectService = projectService;
    private readonly IMigrationProgressTracker _progressTracker = progressTracker;

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
    public async Task<IActionResult> CloneTeams(Guid operationId = default)
    {
        return await ProjectCloneFormView(nameof(CloneTeams), _progressTracker.TakeFinished(operationId));
    }

    [HttpPost]
    public async Task<IActionResult> CloneTeams(Guid templateProjectId, Guid projectId, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await ProjectCloneFormView(nameof(CloneTeams));
        }

        operationId = await _progressTracker.RunAsync(operationId, "Cloning teams...", "Teams cloned successfully.",
            async progress => await _cloneManager.CloneTeamsAsync(templateProjectId, projectId, progress));
        return RedirectToAction(nameof(CloneTeams), new { operationId });
    }

    [HttpGet]
    public async Task<IActionResult> CloneTeamFieldValues(Guid operationId = default)
    {
        return await TeamCloneFormView(nameof(CloneTeamFieldValues), _progressTracker.TakeFinished(operationId));
    }

    [HttpPost]
    public async Task<IActionResult> CloneTeamFieldValues(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, Guid operationId = default)
    {
        if (!ModelState.IsValid)
        {
            return await TeamCloneFormView(nameof(CloneTeamFieldValues));
        }

        operationId = await _progressTracker.RunAsync(operationId, "Cloning team field values...", "Team field values cloned successfully.",
            progress => _cloneManager.CloneTeamFieldValuesAsync(templateProjectId, projectId, templateTeamId, projectTeamId, progress));
        return RedirectToAction(nameof(CloneTeamFieldValues), new { operationId });
    }

    private async Task<ViewResult> ProjectCloneFormView(string action, MigrationProgress? finishedMigration = null)
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new();
        return View(action, new CloneFormViewModel<ProjectBase>(projects.Value, "Team", action, finishedMigration));
    }

    private async Task<ViewResult> TeamCloneFormView(string action, MigrationProgress? finishedMigration = null)
    {
        Teams teams = await _teamsService.GetAllTeamsAsync() ?? new();
        return View(action, new CloneFormViewModel<Team>(teams.Value, "Team", action, finishedMigration));
    }
}
