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
            if (project?.State == ProjectState.WellFormed)
            {
                return project;
            }
        }

        throw new TimeoutException("Project creation timed out.");
    }

    public async Task CloneClassificationNodes(Guid templateProjectId, Guid projectId, TreeStructureGroup structureGroup)
    {
        Iteration templateIterations = await _iterationService.GetAllAsync(templateProjectId, structureGroup) ?? new();
        Iteration iterations = await _iterationService.CreateAsync(projectId, templateIterations, structureGroup);
        await _iterationService.MoveAsync(projectId, iterations.Children, structureGroup, "");
    }

    private async Task<Dictionary<Guid, Guid>> MapClassificationNodes(Guid templateProjectId, Guid projectId, TreeStructureGroup structureGroup)
    {
        var templateClassificationNodes = await _iterationService.GetAllAsync(templateProjectId, structureGroup) ?? new();
        var classificationNodes = await _iterationService.GetAllAsync(projectId, structureGroup) ?? new();
        var ClassificationNodeMap = new Dictionary<Guid, Guid>
        {
            { templateClassificationNodes.Identifier, classificationNodes.Identifier }
        };

        MapClassificationNodes(templateClassificationNodes, classificationNodes, ClassificationNodeMap);

        return ClassificationNodeMap;
    }

    public static void MapClassificationNodes(Iteration templateIterations, Iteration iterations, Dictionary<Guid, Guid> iterationMap)
    {
        foreach (var templateIteration in templateIterations.Children)
        {
            var iteration = iterations.Children.FirstOrDefault(i => i.Name == templateIteration.Name);
            if (iteration != null)
            {
                iterationMap.Add(templateIteration.Identifier, iteration.Identifier);
                MapClassificationNodes(templateIteration, iteration, iterationMap);
            }
        }
    }

    public async Task CloneTeamsAndSettingsAndBoardsAsync(Project templateProject, Project project)
    {
        Dictionary<Guid, Guid> mapTeams = await CloneTeamsAsync(templateProject, project);

        foreach (var mappedTeam in mapTeams)
        {
            await CloneTeamSettingsAsync(templateProject.Id, project.Id, mappedTeam.Key, mappedTeam.Value);
            await CloneTeamIterationsAsync(templateProject.Id, project.Id, mappedTeam.Key, mappedTeam.Value);
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
        return await _teamsService.CreateTeamFromTemplateAsync(project.Id, templateTeams.Value, templateProject.DefaultTeam.Id, project.DefaultTeam.Id);
    }

    public async Task<Dictionary<Guid, Guid>> CloneTeamsAsync(Guid templateProjectId, Guid projectId)
    {
        Project templateProject = await _projectService.GetProjectAsync(templateProjectId) ?? new();
        Project project = await _projectService.GetProjectAsync(projectId) ?? new();

        return await CloneTeamsAsync(templateProject, project);
    }

    public async Task CloneTeamSettingsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId)
    {
        var templateTeamSettings = await _teamSettingsService.GetTeamSettings(templateProjectId, templateTeamId) ?? new();
        var iterationMap = await MapClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations);
        var mappedBacklogIterationId = iterationMap.GetValueOrDefault(templateTeamSettings.BacklogIteration?.Id ?? Guid.Empty);
        var mappedDefaultIterationId = iterationMap.GetValueOrDefault(templateTeamSettings.DefaultIteration?.Id ?? Guid.Empty);

        PatchTeamSettings newTeamSettings = new()
        {
            BacklogIteration = mappedBacklogIterationId,
            BacklogVisibilities = templateTeamSettings.BacklogVisibilities,
            BugsBehavior = templateTeamSettings.BugsBehavior,
            DefaultIteration = templateTeamSettings.DefaultIterationMacro is null ? mappedDefaultIterationId : null,
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

    public async Task CloneTeamFieldValuesAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId)
    {
        var templateProject = await _projectService.GetProjectAsync(templateProjectId) ?? new();
        var project = await _projectService.GetProjectAsync(projectId) ?? new();
        await CloneTeamFieldValuesAsync(templateProject, project, templateTeamId, projectTeamId);
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

    public async Task CloneTeamIterationsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId)
    {
        var oldIterations = await _teamSettingsService.GetIterations(projectId, projectTeamId) ?? new();
        foreach (var iteration in oldIterations.Value)
        {
            await _teamSettingsService.DeleteIteration(projectId, projectTeamId, iteration.Id);
        }

        var iterations = await _teamSettingsService.GetIterations(templateProjectId, templateTeamId) ?? new();
        var iterationMap = await MapClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations);
        foreach (var iteration in iterations.Value)
        {
            if (iterationMap.TryGetValue(iteration.Id, out var mappedIterationId))
            {
                await _teamSettingsService.CreateIteration(projectId, projectTeamId, mappedIterationId);
            }
        }
    }
}