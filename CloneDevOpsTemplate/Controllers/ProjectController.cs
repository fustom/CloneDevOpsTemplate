using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class ProjectController(IProjectService projectService, IIterationService iterationService, ITeamsService teamsService, ITeamSettingsService teamSettingsService, IBoardService boardService, IRepositoryService repositoryService, IServiceService serviceService) : Controller
{
    private readonly IProjectService _projectService = projectService;
    private readonly IIterationService _iterationService = iterationService;
    private readonly ITeamsService _teamsService = teamsService;
    private readonly IBoardService _boardService = boardService;
    private readonly ITeamSettingsService _teamSettingsService = teamSettingsService;
    private readonly IRepositoryService _repositoryService = repositoryService;
    private readonly IServiceService _serviceService = serviceService;

    [HttpGet]
    async public Task<IActionResult> Projects()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new Projects();
        return View(projects.Value);
    }

    [HttpGet]
    async public Task<IActionResult> Project(Guid projectId)
    {
        Project project = await _projectService.GetProjectAsync(projectId) ?? new Project();
        return View(project);
    }

    [HttpGet]
    async public Task<IActionResult> ProjectProperties(Guid projectId)
    {
        ProjectProperties projectProperties = await _projectService.GetProjectPropertiesAsync(projectId) ?? new ProjectProperties();
        return View(projectProperties.Value);
    }

    [HttpGet]
    async public Task<IActionResult> CreateProject()
    {
        return await Projects();
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
        // TODO: UpdateBoardRows

        // Loop through the teams in the template project
        foreach (Guid templateTeamId in templateTeams.Value.Select(templateTeam => templateTeam.Id))
        {
            var projectTeamId = mapTeams.GetValueOrDefault(templateTeamId);
            var templateTeamSettings = await _teamSettingsService.GetTeamSettings(templateProjectId, templateTeamId) ?? new();
            await _teamSettingsService.UpdateTeamSettings(project.Id, projectTeamId, templateTeamSettings);

            Boards projectBoards = await _boardService.GetBoardsAsync(project.Id, projectTeamId) ?? new();
            // Clone the board columns from the template project
            await _boardService.MoveBoardColumnsAsync(project.Id, projectTeamId, templateProjectId, templateTeamId, projectBoards);

            // Clone the board rows from the template project
            await _boardService.MoveBoardRowsAsync(project.Id, projectTeamId, templateProjectId, templateTeamId, projectBoards);
        }

        // Get default repositories
        Repositories repositories = await _repositoryService.GetRepositoriesAsync(project.Id) ?? new();
        // Get template repositories
        Repositories templateRepositories = await _repositoryService.GetRepositoriesAsync(templateProjectId) ?? new();
        // Loop through the repositories in the template project
        foreach (Repository templateRepository in templateRepositories.Value)
        {
            // Clone repository from the template project
            Repository repository = await _repositoryService.CreateRepositoryAsync(project.Id, templateRepository.Name) ?? new();
            ServiceModel serviceModel = await _serviceService.CreateServiceAsync(templateRepository.Name, templateRepository.RemoteUrl, templateRepository.Name, project.Id) ?? new();
            await _repositoryService.CreateImportRequest(project.Id, repository.Id, templateRepository.RemoteUrl, serviceModel.Id);
        }
        // Loop through the default repositories
        foreach (Repository repository in repositories.Value)
        {
            // Delete default repositories
            await _repositoryService.DeleteRepositoryAsync(project.Id, repository.Id);
        }

        return RedirectToAction("Project", new { projectId = project.Id });
    }
}