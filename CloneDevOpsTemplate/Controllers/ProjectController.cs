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
    public async Task<IActionResult> Projects()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new Projects();
        return View(projects.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Project(Guid projectId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Project project = await _projectService.GetProjectAsync(projectId) ?? new Project();
        return View(project);
    }

    [HttpGet]
    public async Task<IActionResult> ProjectProperties(Guid projectId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ProjectProperties projectProperties = await _projectService.GetProjectPropertiesAsync(projectId) ?? new ProjectProperties();
        return View(projectProperties.Value);
    }

    [HttpGet]
    public async Task<IActionResult> CreateProject()
    {
        return await Projects();
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject(Guid templateProjectId, string newProjectName, string description, string visibility)
    {
        if (!ModelState.IsValid)
        {
            return await CreateProject();
        }

        Project templateProject = await _projectService.GetProjectAsync(templateProjectId) ?? new();
        CreateProjectResponse createProjectResponse = await _projectService.CreateProjectAsync(
            newProjectName,
            description,
            templateProject.Capabilities.ProcessTemplate.TemplateTypeId,
            templateProject.Capabilities.Versioncontrol.SourceControlType,
            visibility
        ) ?? new();

        if (createProjectResponse.Message is not null)
        {
            ModelState.AddModelError("ErrorMessage", createProjectResponse.Message);
            return await CreateProject();
        }

        Project project = await WaitForProjectCreation(newProjectName);

        await Task.WhenAll(
            CloneIterations(templateProjectId, project.Id),
            // TODO: CloneAreas
            CloneTeamsAndBoards(templateProject, project),
            // TODO: MapTeamIterations
            CloneRepositories(templateProjectId, project.Id)
        );

        return RedirectToAction("Project", new { projectId = project.Id });
    }

    private async Task<Project> WaitForProjectCreation(string newProjectName)
    {
        var timeout = DateTime.UtcNow.AddMinutes(5);

        while (DateTime.UtcNow < timeout)
        {
            var project = await _projectService.GetProjectAsync(newProjectName);
            if (project?.State == "wellFormed")
            {
                return project;
            }
            await Task.Delay(1000);
        }

        throw new TimeoutException("Project creation timed out.");
    }

    private async Task CloneIterations(Guid templateProjectId, Guid newProjectId)
    {
        Iteration templateIterations = await _iterationService.GetIterationsAsync(templateProjectId) ?? new();
        Iteration iterations = await _iterationService.CreateIterationAsync(newProjectId, templateIterations);
        await _iterationService.MoveIteration(newProjectId, iterations.Children);
    }

    private async Task CloneTeamsAndBoards(Project templateProject, Project project)
    {
        Teams templateTeams = await _teamsService.GetTeamsAsync(templateProject.Id) ?? new();
        Dictionary<Guid, Guid> mapTeams = await _teamsService.CreateTeamFromTemplateAsync(
            project.Id,
            templateTeams.Value,
            templateProject.DefaultTeam.Id,
            project.DefaultTeam.Id
        );

        await Parallel.ForEachAsync(templateTeams.Value.Select(templateTeam => templateTeam.Id), async (templateTeamId, ct) =>
        {
            var projectTeamId = mapTeams.GetValueOrDefault(templateTeamId);
            var templateTeamSettings = await _teamSettingsService.GetTeamSettings(templateProject.Id, templateTeamId) ?? new();
            await _teamSettingsService.UpdateTeamSettings(project.Id, projectTeamId, templateTeamSettings);

            Boards projectBoards = await _boardService.GetBoardsAsync(project.Id, projectTeamId) ?? new();
            await Task.WhenAll(
                _boardService.MoveBoardColumnsAsync(project.Id, projectTeamId, templateProject.Id, templateTeamId, projectBoards),
                _boardService.MoveBoardRowsAsync(project.Id, projectTeamId, templateProject.Id, templateTeamId, projectBoards),
                _boardService.MoveCardSettingsAsync(project.Id, projectTeamId, templateProject.Id, templateTeamId, projectBoards),
                _boardService.MoveCardStylesAsync(project.Id, projectTeamId, templateProject.Id, templateTeamId, projectBoards)
            );
        });
    }

    private async Task CloneRepositories(Guid templateProjectId, Guid newProjectId)
    {
        Repositories repositories = await _repositoryService.GetRepositoriesAsync(newProjectId) ?? new();
        Repositories templateRepositories = await _repositoryService.GetRepositoriesAsync(templateProjectId) ?? new();

        foreach (Repository templateRepository in templateRepositories.Value)
        {
            Repository repository = await _repositoryService.CreateRepositoryAsync(newProjectId, templateRepository.Name) ?? new();
            ServiceModel serviceModel = await _serviceService.CreateServiceAsync(
                templateRepository.Name,
                templateRepository.RemoteUrl,
                templateRepository.Name,
                newProjectId
            ) ?? new();
            await _repositoryService.CreateImportRequest(newProjectId, repository.Id, templateRepository.RemoteUrl, serviceModel.Id);
        }

        foreach (Repository repository in repositories.Value)
        {
            await _repositoryService.DeleteRepositoryAsync(newProjectId, repository.Id);
        }
    }
}