using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class ProjectController(IProjectService projectService, IIterationService iterationService, ITeamsService teamsService, ITeamSettingsService teamSettingsService, IBoardService boardService) : Controller
{
    private readonly IProjectService _projectService = projectService;
    private readonly IIterationService _iterationService = iterationService;
    private readonly ITeamsService _teamsService = teamsService;
    private readonly IBoardService _boardService = boardService;
    private readonly ITeamSettingsService _teamSettingsService = teamSettingsService;

    async public Task<IActionResult> Projects()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new Projects();
        return View(projects.Value);
    }

    async public Task<IActionResult> Project(Guid projectId)
    {
        Project project = await _projectService.GetProjectAsync(projectId) ?? new Project();
        return View(project);
    }

    async public Task<IActionResult> ProjectProperties(Guid projectId)
    {
        ProjectProperties projectProperties = await _projectService.GetProjectPropertiesAsync(projectId) ?? new ProjectProperties();
        return View(projectProperties.Value);
    }

    [HttpGet]
    async public Task<IActionResult> CreateProject()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new Projects();
        return View(projects.Value);
    }

    [HttpPost]
    async public Task<IActionResult> CreateProject(Guid templateProjectId, string newProjectName, string description, string visibility)
    {
        // Query all the projects to fill out the combobox for the 'Create project' form
        Project templateProject = await _projectService.GetProjectAsync(templateProjectId) ?? new();
        CreateProjectResponse createProjectResponse = await _projectService.CreateProjectAsync(newProjectName, description, templateProject.Capabilities.ProcessTemplate.TemplateTypeId, templateProject.Capabilities.Versioncontrol.SourceControlType, visibility) ?? new();

        if (createProjectResponse.Message is not null)
        {
            ModelState.AddModelError("ErrorMessage", createProjectResponse.Message);
            Projects projects = await _projectService.GetAllProjectsAsync() ?? new();
            return View(projects.Value);
        }

        // Ping every 1 sec until the new project creation is 'done' (its sate is 'wellFormed')
        Project project = new();
        while (project.State != "wellFormed")
        {
            await Task.Delay(1000);
            project = await _projectService.GetProjectAsync(newProjectName) ?? new();
        }

        // Clone the iterations from the template project
        Iteration templateIterations = await _iterationService.GetIterationsAsync(templateProjectId) ?? new();
        Iteration iterations = await _iterationService.CreateIterationAsync(project.Id, templateIterations);
        await _iterationService.MoveIteration(project.Id, iterations.Children);
        // TODO: Areas

        Teams templateTeams = await _teamsService.GetTeamsAsync(templateProjectId) ?? new();
        // TODO: TeamIterationMap
        Dictionary<Guid, Guid> mapTeams = await _teamsService.CreateTeamFromTemplateAsync(project.Id, templateTeams.Value, templateProject.DefaultTeam.Id, project.DefaultTeam.Id);

        // Loop through the teams in the template project
        foreach (Team templateTeam in templateTeams.Value)
        {
            var projectTeamId = mapTeams.GetValueOrDefault(templateTeam.Id);
            var templateTeamSettings = await _teamSettingsService.GetTeamSettings(templateProjectId, templateTeam.Id) ?? new();
            await _teamSettingsService.UpdateTeamSettings(project.Id, projectTeamId, templateTeamSettings);

            Boards projectBoards = await _boardService.GetBoardsAsync(project.Id, projectTeamId) ?? new();
            // Clone the board columns from the template project
            await _boardService.MoveBoardColumnsAsync(project.Id, projectTeamId, templateProjectId, templateTeam.Id, projectBoards);

            // Clone the board rows from the template project
            await _boardService.MoveBoardRowsAsync(project.Id, projectTeamId, templateProjectId, templateTeam.Id, projectBoards);

            // Clone the card settings from the template project
            await _boardService.MoveCardSettingsAsync(project.Id, projectTeamId, templateProjectId, templateTeam.Id, projectBoards);

            //TODO: Clone the card styles from the template project
        }

        return RedirectToAction("Project", new { projectId = project.Id });
    }
}