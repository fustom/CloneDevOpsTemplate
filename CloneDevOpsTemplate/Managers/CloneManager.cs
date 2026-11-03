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

    public async Task<CloneResult> CloneClassificationNodes(Guid templateProjectId, Guid projectId, TreeStructureGroup structureGroup, IProgress<string>? progress = null)
    {
        var result = new CloneResult();
        var templateClassificationNodes = await _iterationService.GetAllAsync(templateProjectId, structureGroup);
        if (templateClassificationNodes is null)
        {
            AddError(result, progress, $"{structureGroup}: could not read the template project's nodes.");
            return result;
        }

        var classificationNodes = await _iterationService.GetAllAsync(projectId, structureGroup);
        if (classificationNodes is null)
        {
            AddError(result, progress, $"{structureGroup}: could not read the target project's nodes.");
            return result;
        }

        progress?.Report($"{structureGroup}: removing {CountChildNodes(classificationNodes)} default node(s)...");
        AddErrors(result, progress, $"{structureGroup}", await _iterationService.DeleteAsync(projectId, structureGroup, classificationNodes));
        progress?.Report($"{structureGroup}: creating {CountChildNodes(templateClassificationNodes)} node(s) from template...");
        AddErrors(result, progress, $"{structureGroup}", await _iterationService.CreateAsync(projectId, templateClassificationNodes, structureGroup, ""));
        progress?.Report($"{structureGroup}: done.");
        return result;
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

    public async Task<CloneResult> CloneTeamsAndSettingsAndBoardsAsync(Project templateProject, Project project, IProgress<string>? progress = null)
    {
        progress?.Report("Teams: cloning teams from template...");
        var teamsResult = await CloneTeamsAsync(templateProject, project, progress);
        var result = CloneResult.Combine(teamsResult);
        var mapTeams = teamsResult.TeamMap;
        progress?.Report($"Teams: {mapTeams.Count} team(s) mapped.");

        int index = 0;
        foreach (var mappedTeam in mapTeams)
        {
            index++;
            var teamName = teamsResult.TemplateTeams.FirstOrDefault(t => t.Id == mappedTeam.Key)?.Name ?? mappedTeam.Key.ToString();
            var prefix = $"Team '{teamName}' ({index}/{mapTeams.Count})";
            var errorPrefix = $"Team '{teamName}'";

            progress?.Report($"{prefix}: cloning settings...");
            await CloneTeamSettingsCoreAsync(templateProject.Id, project.Id, mappedTeam.Key, mappedTeam.Value, result, errorPrefix, progress);
            progress?.Report($"{prefix}: cloning iterations...");
            await CloneTeamIterationsCoreAsync(templateProject.Id, project.Id, mappedTeam.Key, mappedTeam.Value, result, errorPrefix, progress);
            progress?.Report($"{prefix}: cloning area field values...");
            await CloneTeamFieldValuesCoreAsync(templateProject, project, mappedTeam.Key, mappedTeam.Value, result, errorPrefix, progress);
            progress?.Report($"{prefix}: cloning boards...");
            await CloneBoardsCoreAsync(templateProject.Id, project.Id, mappedTeam.Key, mappedTeam.Value, result, errorPrefix, progress);
            progress?.Report($"{prefix}: done.");
        }

        return result;
    }

    public async Task<CloneResult> CloneRepositoriesAsync(Guid templateProjectId, Guid projectId, IProgress<string>? progress = null)
    {
        var result = new CloneResult();
        var templateRepositories = await _repositoryService.GetRepositoriesAsync(templateProjectId);
        if (templateRepositories is null)
        {
            AddError(result, progress, "Repositories: could not read the template project's repositories.");
            return result;
        }

        var repositories = await _repositoryService.GetRepositoriesAsync(projectId);
        if (repositories is null)
        {
            AddError(result, progress, "Repositories: could not read the target project's repositories.");
            return result;
        }

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
                repository = await _repositoryService.CreateRepositoryAsync(projectId, name);
                if (repository is null)
                {
                    AddError(result, progress, $"Repository '{name}': could not be created.");
                    return;
                }
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
                serviceModel = await _serviceService.CreateServiceAsync(name, templateRepository.RemoteUrl, name, projectId);
                if (serviceModel is null)
                {
                    AddError(result, progress, $"Repository '{name}': could not create the service connection.");
                    return;
                }
            }

            progress?.Report($"Repository '{name}': importing...");
            var importRequest = await _repositoryService.CreateImportRequestAsync(projectId, repository.Id, templateRepository.RemoteUrl, serviceModel.Id);
            if (importRequest is null)
            {
                AddError(result, progress, $"Repository '{name}': could not start the import.");
                return;
            }

            var importStatus = await WaitForImportAsync(projectId, repository.Id, importRequest.ImportRequestId, name, progress);
            if (importStatus != GitAsyncOperationStatus.Completed)
            {
                AddError(result, progress, $"Repository '{name}': import {importStatus}.");
                return;
            }

            progress?.Report($"Repository '{name}': imported ({Interlocked.Increment(ref completed)}/{total}).");
        });

        var emptyRepositories = repositories.Value.Where(r => r.Size == 0).ToArray();
        if (emptyRepositories.Length > 0)
        {
            progress?.Report($"Repositories: removing {emptyRepositories.Length} empty default repository(ies)...");
        }

        await Parallel.ForEachAsync(emptyRepositories, async (repository, ct) =>
        {
            var response = await _repositoryService.DeleteRepositoryAsync(projectId, repository.Id);
            await CheckResponseAsync(response, result, progress, $"Repository '{repository.Name}': removing");
        });

        return result;
    }

    private async Task<GitAsyncOperationStatus> WaitForImportAsync(Guid projectId, Guid repositoryId, int importRequestId, string repositoryName, IProgress<string>? progress)
    {
        var timeout = DateTime.UtcNow.AddMinutes(5);
        GitAsyncOperationStatus? lastStatus = null;

        while (DateTime.UtcNow < timeout)
        {
            await Task.Delay(1000);
            var importRequest = await _repositoryService.GetImportRequestAsync(projectId, repositoryId, importRequestId);
            if (importRequest?.Status is GitAsyncOperationStatus.Completed or GitAsyncOperationStatus.Failed or GitAsyncOperationStatus.Abandoned)
            {
                return importRequest.Status;
            }

            if (importRequest is not null && importRequest.Status != lastStatus)
            {
                lastStatus = importRequest.Status;
                progress?.Report($"Repository '{repositoryName}': import status {importRequest.Status}");
            }
        }

        throw new TimeoutException($"Import of repository '{repositoryName}' timed out.");
    }

    public async Task<CloneTeamsResult> CloneTeamsAsync(Project templateProject, Project project, IProgress<string>? progress = null)
    {
        Teams? templateTeams = await _teamsService.GetTeamsAsync(templateProject.Id);
        if (templateTeams is null)
        {
            var failed = new CloneTeamsResult();
            AddError(failed, progress, "Teams: could not read the template project's teams.");
            return failed;
        }

        var teamMap = await _teamsService.CreateTeamFromTemplateAsync(project.Id, templateTeams.Value, templateProject.DefaultTeam.Id, project.DefaultTeam.Id);
        var result = new CloneTeamsResult { TemplateTeams = templateTeams.Value, TeamMap = teamMap };

        // The teams service returns an empty id when a team could not be created.
        foreach (var (templateTeamId, _) in teamMap.Where(t => t.Value == Guid.Empty).ToArray())
        {
            var teamName = templateTeams.Value.FirstOrDefault(t => t.Id == templateTeamId)?.Name ?? templateTeamId.ToString();
            AddError(result, progress, $"Team '{teamName}': could not be created.");
            teamMap.Remove(templateTeamId);
        }

        return result;
    }

    public async Task<CloneTeamsResult> CloneTeamsAsync(Guid templateProjectId, Guid projectId, IProgress<string>? progress = null)
    {
        var (templateProject, project, error) = await GetProjectsAsync(templateProjectId, projectId);
        if (error is not null)
        {
            var failed = new CloneTeamsResult();
            AddError(failed, progress, error);
            return failed;
        }

        var result = await CloneTeamsAsync(templateProject, project, progress);
        progress?.Report($"Teams: {result.TeamMap.Count} team(s) mapped.");
        return result;
    }

    private async Task<(Project TemplateProject, Project Project, string? Error)> GetProjectsAsync(Guid templateProjectId, Guid projectId)
    {
        var templateProject = await _projectService.GetProjectAsync(templateProjectId);
        if (templateProject is null)
        {
            return (new(), new(), "Could not read the template project.");
        }

        var project = await _projectService.GetProjectAsync(projectId);
        if (project is null)
        {
            return (new(), new(), "Could not read the target project.");
        }

        return (templateProject, project, null);
    }

    public async Task<CloneResult> CloneTeamSettingsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, IProgress<string>? progress = null)
    {
        var result = new CloneResult();
        await CloneTeamSettingsCoreAsync(templateProjectId, projectId, templateTeamId, projectTeamId, result, "Team", progress);
        return result;
    }

    private async Task CloneTeamSettingsCoreAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, CloneResult result, string prefix, IProgress<string>? progress)
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
        var response = await _teamSettingsService.UpdateTeamSettings(projectId, projectTeamId, newTeamSettings);
        await CheckResponseAsync(response, result, progress, $"{prefix}: updating settings");
    }

    public async Task<CloneResult> CloneTeamFieldValuesAsync(Project templateProject, Project project, Guid templateTeamId, Guid projectTeamId, IProgress<string>? progress = null)
    {
        var result = new CloneResult();
        await CloneTeamFieldValuesCoreAsync(templateProject, project, templateTeamId, projectTeamId, result, "Team", progress);
        return result;
    }

    private async Task CloneTeamFieldValuesCoreAsync(Project templateProject, Project project, Guid templateTeamId, Guid projectTeamId, CloneResult result, string prefix, IProgress<string>? progress)
    {
        var teamFieldValues = await _teamSettingsService.GetTeamFieldValues(templateProject.Id, templateTeamId) ?? new();
        teamFieldValues.DefaultValue = teamFieldValues.DefaultValue.Replace(templateProject.Name, project.Name);
        foreach (var value in teamFieldValues.Values)
        {
            value.Value = value.Value.Replace(templateProject.Name, project.Name);
        }
        var response = await _teamSettingsService.UpdateTeamFieldValues(project.Id, projectTeamId, teamFieldValues);
        await CheckResponseAsync(response, result, progress, $"{prefix}: updating area field values");
    }

    public async Task<CloneResult> CloneTeamFieldValuesAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, IProgress<string>? progress = null)
    {
        var (templateProject, project, error) = await GetProjectsAsync(templateProjectId, projectId);
        if (error is not null)
        {
            var failed = new CloneResult();
            AddError(failed, progress, error);
            return failed;
        }

        return await CloneTeamFieldValuesAsync(templateProject, project, templateTeamId, projectTeamId, progress);
    }

    public async Task<CloneResult> CloneBoardsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, IProgress<string>? progress = null)
    {
        var result = new CloneResult();
        await CloneBoardsCoreAsync(templateProjectId, projectId, templateTeamId, projectTeamId, result, "Team", progress);
        return result;
    }

    private async Task CloneBoardsCoreAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, CloneResult result, string prefix, IProgress<string>? progress)
    {
        Boards? projectBoards = await _boardService.GetBoardsAsync(projectId, projectTeamId);
        if (projectBoards is null)
        {
            AddError(result, progress, $"{prefix}: could not read the target team's boards.");
            return;
        }

        var moveResults = await Task.WhenAll(
            _boardService.MoveBoardColumnsAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards),
            _boardService.MoveBoardRowsAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards),
            _boardService.MoveCardSettingsAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards),
            _boardService.MoveCardStylesAsync(projectId, projectTeamId, templateProjectId, templateTeamId, projectBoards)
        );
        // Each move reads the template boards itself, so a read failure would otherwise be reported four times.
        foreach (var error in moveResults.SelectMany(r => r.Errors).Distinct())
        {
            AddError(result, progress, $"{prefix}: {error}");
        }
    }

    public async Task<CloneResult> CloneTeamIterationsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, IProgress<string>? progress = null)
    {
        var result = new CloneResult();
        await CloneTeamIterationsCoreAsync(templateProjectId, projectId, templateTeamId, projectTeamId, result, "Team", progress);
        return result;
    }

    private async Task CloneTeamIterationsCoreAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, CloneResult result, string prefix, IProgress<string>? progress)
    {
        var oldIterations = await _teamSettingsService.GetIterations(projectId, projectTeamId) ?? new();
        foreach (var iteration in oldIterations.Value)
        {
            var response = await _teamSettingsService.DeleteIteration(projectId, projectTeamId, iteration.Id);
            await CheckResponseAsync(response, result, progress, $"{prefix}: removing iteration '{iteration.Name}'");
        }

        var iterations = await _teamSettingsService.GetIterations(templateProjectId, templateTeamId) ?? new();
        var iterationMap = await MapClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations);
        foreach (var iteration in iterations.Value)
        {
            if (iterationMap.TryGetValue(iteration.Id, out var mappedIterationId))
            {
                var response = await _teamSettingsService.CreateIteration(projectId, projectTeamId, mappedIterationId);
                await CheckResponseAsync(response, result, progress, $"{prefix}: adding iteration '{iteration.Name}'");
            }
        }
    }

    public async Task<CloneResult> CloneGitPullRequestsAsync(Guid templateProjectId, Guid projectId, IProgress<string>? progress = null)
    {
        var result = new CloneResult();
        var templatePullRequests = await _repositoryService.GetGitPullRequestAsync(templateProjectId);
        if (templatePullRequests is null)
        {
            AddError(result, progress, "Pull requests: could not read the template project's pull requests.");
            return result;
        }

        var mappedRespositories = await MapRepositories(templateProjectId, projectId);
        int total = templatePullRequests.Value.Length;
        progress?.Report($"Pull requests: {total} pull request(s) to clone.");

        int index = 0;
        foreach (var templatePullRequest in templatePullRequests.Value)
        {
            index++;
            var label = $"Pull request '{templatePullRequest.Title}'";
            progress?.Report($"{label} ({index}/{total}): creating...");
            if (!mappedRespositories.TryGetValue(templatePullRequest.Repository.Id, out var repository))
            {
                AddError(result, progress, $"{label}: repository '{templatePullRequest.Repository.Name}' not found in the target project.");
                continue;
            }

            var response = await _repositoryService.CreateGitPullRequestAsync(projectId, repository, templatePullRequest);
            await CheckResponseAsync(response, result, progress, $"{label}: creating");
        }

        return result;
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

    private static void AddError(CloneResult result, IProgress<string>? progress, string error)
    {
        result.AddError(error);
        progress?.Report($"Error: {error}");
    }

    private static void AddErrors(CloneResult result, IProgress<string>? progress, string prefix, CloneResult source)
    {
        foreach (var error in source.Errors)
        {
            AddError(result, progress, $"{prefix}: {error}");
        }
    }

    private static async Task CheckResponseAsync(HttpResponseMessage? response, CloneResult result, IProgress<string>? progress, string action)
    {
        var responseResult = new CloneResult();
        if (!await responseResult.CheckResponseAsync(response, action))
        {
            AddError(result, progress, responseResult.Errors[0]);
        }
    }
}
