using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Managers;

public class CloneManager(IProjectService projectService, IIterationService iterationService, ITeamsService teamsService, ITeamSettingsService teamSettingsService, IBoardService boardService, IRepositoryService repositoryService, IServiceService serviceService) : ICloneManager
{
    private readonly IProjectService _projectService = projectService;
    private readonly IIterationService _iterationService = iterationService;
    private readonly ITeamsService _teamsService = teamsService;
    private readonly ITeamSettingsService _teamSettingsService = teamSettingsService;
    private readonly IBoardService _boardService = boardService;
    private readonly IRepositoryService _repositoryService = repositoryService;
    private readonly IServiceService _serviceService = serviceService;

    public async Task<Tuple<Project, Project, string?>> CloneProjectAsync(Guid templateProjectId, string newProjectName, string description, Visibility visibility)
    {
        Project templateProject = await _projectService.GetProjectAsync(templateProjectId) ?? new();
        CreateProjectResponse createProjectResponse = await _projectService.CreateProjectAsync(
            newProjectName,
            description,
            templateProject.Capabilities.ProcessTemplate.TemplateTypeId,
            templateProject.Capabilities.Versioncontrol.SourceControlType,
            visibility
        ) ?? new();
        Project project = await WaitForProjectCreationAsync(newProjectName);

        return Tuple.Create(project, templateProject, createProjectResponse.Message);
    }

    private async Task<Project> WaitForProjectCreationAsync(string newProjectName)
    {
        var timeout = DateTime.UtcNow.AddMinutes(5);

        while (DateTime.UtcNow < timeout)
        {
            await Task.Delay(1000);
            var project = await _projectService.GetProjectAsync(newProjectName);
            if (project?.State == "wellFormed")
            {
                return project;
            }
        }

        throw new TimeoutException("Project creation timed out.");
    }

    public async Task CloneIterationsAsync(Guid templateProjectId, Guid projectId)
    {
        Iteration templateIterations = await _iterationService.GetIterationsAsync(templateProjectId) ?? new();
        Iteration iterations = await _iterationService.CreateIterationAsync(projectId, templateIterations);
        await _iterationService.MoveIterationAsync(projectId, iterations.Children, "");
    }

    public async Task CloneAreasAsync(Guid templateProjectId, Guid projectId)
    {
        Iteration templateAreas = await _iterationService.GetAreaAsync(templateProjectId) ?? new();
        Iteration areas = await _iterationService.CreateAreaAsync(projectId, templateAreas);
        await _iterationService.MoveAreaAsync(projectId, areas.Children, "");
    }

    public async Task CloneTeamsAndSettingsAndBoardsAsync(Project templateProject, Project project)
    {
        Dictionary<Guid, Guid> mapTeams = await CloneTeamsAsync(templateProject, project);
        var teamSettings = await _teamSettingsService.GetTeamSettings(project.Id, project.DefaultTeam.Id) ?? new();

        foreach (var mappedTeam in mapTeams)
        {
            await CloneTeamSettingsAsync(templateProject.Id, project.Id, mappedTeam.Key, mappedTeam.Value, teamSettings.BacklogIteration?.Id);
            await CloneTeamFieldValuesAsync(templateProject, project, mappedTeam.Key, mappedTeam.Value);
            await CloneBoardsAsync(templateProject.Id, project.Id, mappedTeam.Key, mappedTeam.Value);
        }
    }

    public async Task CloneRepositoriesAsync(Guid templateProjectId, Guid projectId)
    {
        Repositories repositories = await _repositoryService.GetRepositoriesAsync(projectId) ?? new();
        Repositories templateRepositories = await _repositoryService.GetRepositoriesAsync(templateProjectId) ?? new();

        await Parallel.ForEachAsync(templateRepositories.Value, async (templateRepository, ct) =>
        {
            Repository repository = await _repositoryService.CreateRepositoryAsync(projectId, templateRepository.Name) ?? new();
            ServiceModel serviceModel = await _serviceService.CreateServiceAsync(templateRepository.Name, templateRepository.RemoteUrl, templateRepository.Name, projectId) ?? new();
            await _repositoryService.CreateImportRequest(projectId, repository.Id, templateRepository.RemoteUrl, serviceModel.Id);
        });

        await Parallel.ForEachAsync(repositories.Value, async (repository, ct) =>
        {
            await _repositoryService.DeleteRepositoryAsync(projectId, repository.Id);
        });
    }

    public async Task<Dictionary<Guid, Guid>> CloneTeamsAsync(Project templateProject, Project project)
    {
        Teams templateTeams = await _teamsService.GetTeamsAsync(templateProject.Id) ?? new();
        // TODO: TeamIterationMap
        return await _teamsService.CreateTeamFromTemplateAsync(project.Id, templateTeams.Value, templateProject.DefaultTeam.Id, project.DefaultTeam.Id);
    }

    public async Task<Dictionary<Guid, Guid>> CloneTeamsAsync(Guid templateProjectId, Guid projectId)
    {
        Project templateProject = await _projectService.GetProjectAsync(templateProjectId) ?? new();
        Project project = await _projectService.GetProjectAsync(projectId) ?? new();

        return await CloneTeamsAsync(templateProject, project);
    }

    public async Task CloneTeamSettingsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, Guid? backlogIterationId)
    {
        var templateTeamSettings = await _teamSettingsService.GetTeamSettings(templateProjectId, templateTeamId) ?? new();
        PatchTeamSettings newTeamSettings = new()
        {
            BacklogIteration = backlogIterationId,
            BacklogVisibilities = templateTeamSettings.BacklogVisibilities,
            BugsBehavior = templateTeamSettings.BugsBehavior,
            DefaultIteration = templateTeamSettings.DefaultIteration?.Id,
            DefaultIterationMacro = templateTeamSettings.DefaultIterationMacro,
            WorkingDays = templateTeamSettings.WorkingDays
        };
        await _teamSettingsService.UpdateTeamSettings(projectId, projectTeamId, newTeamSettings);
    }

    public async Task CloneTeamFieldValuesAsync(Project templateProject, Project project, Guid templateTeamId, Guid projectTeamId)
    {
        var teamFieldValues = await _teamSettingsService.GetTeamFieldValues(templateProject.Id, templateTeamId) ?? new();
        teamFieldValues.DefaultValue = teamFieldValues.DefaultValue.Replace(templateProject.Name, project.Name);
        foreach (var value in teamFieldValues.Values)
        {
            value.Value = value.Value.Replace(templateProject.Name, project.Name);
        }
        await _teamSettingsService.UpdateTeamFieldValues(project.Id, projectTeamId, teamFieldValues);
    }

    public async Task CloneBoardsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId)
    {
        Boards projectBoards = await _boardService.GetBoardsAsync(projectId, projectTeamId) ?? new();
        await Task.WhenAll(
            _boardService.MoveBoardColumnsAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards),
            _boardService.MoveBoardRowsAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards),
            _boardService.MoveCardSettingsAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards),
            _boardService.MoveCardStylesAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards)
        );
    }
}