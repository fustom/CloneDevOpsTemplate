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

    public async Task<CloneProjectResult> CloneProjectAsync(Guid templateProjectId, string newProjectName, string description, Visibility visibility, IProgress<string>? progress = null)
    {
        progress?.Report("Reading template project...");
        Project templateProject = await _projectService.GetProjectAsync(templateProjectId) ?? new();
        progress?.Report($"Requesting creation of project '{newProjectName}' based on template '{templateProject.Name}'...");
        CreateProjectResponse createProjectResponse = await _projectService.CreateProjectAsync(
            newProjectName,
            description,
            templateProject.Capabilities.ProcessTemplate.TemplateTypeId,
            templateProject.Capabilities.Versioncontrol.SourceControlType,
            visibility
        ) ?? new();
        progress?.Report($"Waiting for project '{newProjectName}' to be ready...");
        Project project = await WaitForProjectCreationAsync(newProjectName, progress);
        progress?.Report($"Project '{newProjectName}' is ready.");

        return new CloneProjectResult { Project = project, TemplateProject = templateProject, ErrorMessage = createProjectResponse.Message };
    }

    private async Task<Project> WaitForProjectCreationAsync(string newProjectName, IProgress<string>? progress = null)
    {
        var timeout = DateTime.UtcNow.AddMinutes(5);
        ProjectState? lastState = null;

        while (DateTime.UtcNow < timeout)
        {
            await Task.Delay(1000);
            var project = await _projectService.GetProjectAsync(newProjectName);
            if (project?.State == ProjectState.WellFormed)
            {
                return project;
            }

            if (project is not null && project.State != lastState)
            {
                lastState = project.State;
                progress?.Report($"Project '{newProjectName}' state: {project.State}");
            }
        }

        throw new TimeoutException("Project creation timed out.");
    }

    public async Task CloneClassificationNodes(Guid templateProjectId, Guid projectId, TreeStructureGroup structureGroup, IProgress<string>? progress = null)
    {
        var classificationNodes = await _iterationService.GetAllAsync(projectId, structureGroup) ?? new();
        progress?.Report($"{structureGroup}: removing {CountChildNodes(classificationNodes)} default node(s)...");
        await _iterationService.DeleteAsync(projectId, structureGroup, classificationNodes);
        var templateClassificationNodes = await _iterationService.GetAllAsync(templateProjectId, structureGroup) ?? new();
        progress?.Report($"{structureGroup}: creating {CountChildNodes(templateClassificationNodes)} node(s) from template...");
        await _iterationService.CreateAsync(projectId, templateClassificationNodes, structureGroup, "");
        progress?.Report($"{structureGroup}: done.");
    }

    private static int CountChildNodes(Iteration node) => node.Children.Sum(child => 1 + CountChildNodes(child));

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

    public async Task CloneTeamsAndSettingsAndBoardsAsync(Project templateProject, Project project, IProgress<string>? progress = null)
    {
        progress?.Report("Teams: cloning teams from template...");
        Teams templateTeams = await _teamsService.GetTeamsAsync(templateProject.Id) ?? new();
        Dictionary<Guid, Guid> mapTeams = await _teamsService.CreateTeamFromTemplateAsync(project.Id, templateTeams.Value, templateProject.DefaultTeam.Id, project.DefaultTeam.Id);
        progress?.Report($"Teams: {mapTeams.Count} team(s) mapped.");

        int index = 0;
        foreach (var mappedTeam in mapTeams)
        {
            index++;
            var teamName = templateTeams.Value.FirstOrDefault(t => t.Id == mappedTeam.Key)?.Name ?? mappedTeam.Key.ToString();
            var prefix = $"Team '{teamName}' ({index}/{mapTeams.Count})";

            progress?.Report($"{prefix}: cloning settings...");
            await CloneTeamSettingsAsync(templateProject.Id, project.Id, mappedTeam.Key, mappedTeam.Value);
            progress?.Report($"{prefix}: cloning iterations...");
            await CloneTeamIterationsAsync(templateProject.Id, project.Id, mappedTeam.Key, mappedTeam.Value);
            progress?.Report($"{prefix}: cloning area field values...");
            await CloneTeamFieldValuesAsync(templateProject, project, mappedTeam.Key, mappedTeam.Value);
            progress?.Report($"{prefix}: cloning boards...");
            await CloneBoardsAsync(templateProject.Id, project.Id, mappedTeam.Key, mappedTeam.Value);
            progress?.Report($"{prefix}: done.");
        }
    }

    public async Task CloneRepositoriesAsync(Guid templateProjectId, Guid projectId, IProgress<string>? progress = null)
    {
        var repositories = await _repositoryService.GetRepositoriesAsync(projectId) ?? new();
        var templateRepositories = await _repositoryService.GetRepositoriesAsync(templateProjectId) ?? new();
        int total = templateRepositories.Value.Length;
        int completed = 0;
        progress?.Report($"Repositories: {total} repository(ies) to clone.");

        await Parallel.ForEachAsync(templateRepositories.Value, async (templateRepository, ct) =>
        {
            var name = templateRepository.Name;
            var repository = repositories.Value.FirstOrDefault(r => r.Name == name);
            if (repository is null)
            {
                progress?.Report($"Repository '{name}': creating...");
                repository = await _repositoryService.CreateRepositoryAsync(projectId, name) ?? new();
            }

            if (repository.Size > 0)
            {
                progress?.Report($"Repository '{name}': already has content, skipped ({Interlocked.Increment(ref completed)}/{total}).");
                return;
            }

            var serviceModel = (await _serviceService.GetServicesAsync(projectId) ?? new())
                               .Value.FirstOrDefault(s => s.Name == name);
            if (serviceModel is null)
            {
                progress?.Report($"Repository '{name}': creating service connection...");
                serviceModel = await _serviceService.CreateServiceAsync(name, templateRepository.RemoteUrl, name, projectId) ?? new();
            }

            progress?.Report($"Repository '{name}': importing...");
            var importRequest = await _repositoryService.CreateImportRequestAsync(projectId, repository.Id, templateRepository.RemoteUrl, serviceModel.Id);
            await WaitForImportAsync(projectId, repository.Id, importRequest?.ImportRequestId ?? 0, name, progress);
            progress?.Report($"Repository '{name}': imported ({Interlocked.Increment(ref completed)}/{total}).");
        });

        var emptyRepositories = repositories.Value.Where(r => r.Size == 0).ToArray();
        if (emptyRepositories.Length > 0)
        {
            progress?.Report($"Repositories: removing {emptyRepositories.Length} empty default repository(ies)...");
        }

        await Parallel.ForEachAsync(emptyRepositories, async (repository, ct) =>
        {
            await _repositoryService.DeleteRepositoryAsync(projectId, repository.Id);
        });
    }

    private async Task WaitForImportAsync(Guid projectId, Guid repositoryId, int importRequestId, string repositoryName, IProgress<string>? progress)
    {
        var timeout = DateTime.UtcNow.AddMinutes(5);
        GitAsyncOperationStatus? lastStatus = null;

        while (DateTime.UtcNow < timeout)
        {
            await Task.Delay(1000);
            var importRequest = await _repositoryService.GetImportRequestAsync(projectId, repositoryId, importRequestId);
            if (importRequest?.Status == GitAsyncOperationStatus.Completed)
            {
                return;
            }

            if (importRequest is not null && importRequest.Status != lastStatus)
            {
                lastStatus = importRequest.Status;
                progress?.Report($"Repository '{repositoryName}': import status {importRequest.Status}");
            }
        }

        throw new TimeoutException("Project creation timed out.");
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

    public async Task CloneGitPullRequestsAsync(Guid templateProjectId, Guid projectId, IProgress<string>? progress = null)
    {
        var templatePullRequests = await _repositoryService.GetGitPullRequestAsync(templateProjectId) ?? new();
        var mappedRespositories = await MapRepositories(templateProjectId, projectId);
        int total = templatePullRequests.Value.Length;
        progress?.Report($"Pull requests: {total} pull request(s) to clone.");

        int index = 0;
        foreach (var templatePullRequest in templatePullRequests.Value)
        {
            index++;
            progress?.Report($"Pull request '{templatePullRequest.Title}' ({index}/{total}): creating...");
            var repository = mappedRespositories.GetValueOrDefault(templatePullRequest.Repository.Id);
            await _repositoryService.CreateGitPullRequestAsync(projectId, repository, templatePullRequest);
        }
    }

    public async Task<Dictionary<Guid, Guid>> MapRepositories(Guid templateProjectId, Guid projectId)
    {
        var mappedRespositories = new Dictionary<Guid, Guid>();
        var repositories = await _repositoryService.GetRepositoriesAsync(projectId) ?? new();
        var templateRepositories = await _repositoryService.GetRepositoriesAsync(templateProjectId) ?? new();

        foreach (var templateRepository in templateRepositories.Value)
        {
            var mappedRespository = repositories.Value.FirstOrDefault(r => r.Name == templateRepository.Name);
            if (mappedRespository is not null)
            {
                mappedRespositories.Add(templateRepository.Id, mappedRespository.Id);
            }
        }

        return mappedRespositories;
    }
}