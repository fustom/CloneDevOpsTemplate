using System.Net.Http.Json;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Moq;

namespace CloneDevOpsTemplateTest.Managers;

public class CloneManagerTest
{
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<IIterationService> _mockIterationService;
    private readonly Mock<ITeamsService> _mockTeamsService;
    private readonly Mock<ITeamSettingsService> _mockTeamSettingsService;
    private readonly Mock<IBoardService> _mockBoardService;
    private readonly Mock<IRepositoryService> _mockRepositoryService;
    private readonly Mock<IServiceService> _mockServiceService;
    private readonly CloneManager _cloneManager;

    public CloneManagerTest()
    {
        _mockProjectService = new Mock<IProjectService>();
        _mockIterationService = new Mock<IIterationService>();
        _mockTeamsService = new Mock<ITeamsService>();
        _mockTeamSettingsService = new Mock<ITeamSettingsService>();
        _mockBoardService = new Mock<IBoardService>();
        _mockRepositoryService = new Mock<IRepositoryService>();
        _mockServiceService = new Mock<IServiceService>();

        // Successful defaults for the bulk service operations; individual tests override them.
        _mockIterationService.Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Iteration>(), It.IsAny<TreeStructureGroup>(), It.IsAny<string>()))
            .ReturnsAsync(new CloneResult());
        _mockIterationService.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<TreeStructureGroup>(), It.IsAny<Iteration>()))
            .ReturnsAsync(new CloneResult());
        _mockBoardService.Setup(s => s.MoveBoardColumnsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .ReturnsAsync(new CloneResult());
        _mockBoardService.Setup(s => s.MoveBoardRowsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .ReturnsAsync(new CloneResult());
        _mockBoardService.Setup(s => s.MoveCardSettingsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .ReturnsAsync(new CloneResult());
        _mockBoardService.Setup(s => s.MoveCardStylesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .ReturnsAsync(new CloneResult());

        _cloneManager = new CloneManager(
            _mockProjectService.Object,
            _mockIterationService.Object,
            _mockTeamsService.Object,
            _mockTeamSettingsService.Object,
            _mockBoardService.Object,
            _mockRepositoryService.Object,
            _mockServiceService.Object
        );
    }

    [Fact]
    public async Task CloneProjectAsync_ShouldReturnClonedProject()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var newProjectName = "New Project";
        var description = "New Project Description";
        var visibility = Visibility.Private;
        var templateTypeId = Guid.NewGuid();

        var templateProject = new Project
        {
            Capabilities = new Capabilities
            {
                ProcessTemplate = new ProcessTemplate { TemplateTypeId = templateTypeId },
                Versioncontrol = new VersionControl { SourceControlType = "Git" }
            }
        };

        var createProjectResponse = new CreateProjectResponse { Message = "Project created successfully" };
        var newProject = new Project { State = ProjectState.WellFormed };

        _mockProjectService.Setup(s => s.GetProjectAsync(templateProjectId)).ReturnsAsync(templateProject);
        _mockProjectService.Setup(s => s.CreateProjectAsync(newProjectName, description, templateTypeId, "Git", visibility))
            .ReturnsAsync(createProjectResponse);
        _mockProjectService.Setup(s => s.GetProjectAsync(newProjectName)).ReturnsAsync(newProject);

        // Act
        var result = await _cloneManager.CloneProjectAsync(templateProjectId, newProjectName, description, visibility);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newProject, result.Project);
        Assert.Equal(templateProject, result.TemplateProject);
        Assert.Equal("Project created successfully", result.ErrorMessage);
    }

    [Fact]
    public async Task CloneRepositoriesAsync_ShouldCloneRepositories()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var templateRepositories = new Repositories
        {
            Value =
            [
                new Repository { Name = "Repo1", RemoteUrl = "http://example.com/repo1" }
            ]
        };

        var repositories = new Repositories
        {
            Value =
            [
                new Repository { Id = Guid.NewGuid() }
            ]
        };

        _mockRepositoryService.Setup(s => s.GetRepositoriesAsync(templateProjectId)).ReturnsAsync(templateRepositories);
        _mockRepositoryService.Setup(s => s.GetRepositoriesAsync(projectId)).ReturnsAsync(repositories);
        _mockRepositoryService.Setup(s => s.GetImportRequestAsync(projectId, It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(new GitImportRequest { Status = GitAsyncOperationStatus.InProgress });
        _mockRepositoryService.Setup(s => s.GetImportRequestAsync(projectId, It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(new GitImportRequest { Status = GitAsyncOperationStatus.Completed });

        // Act
        await _cloneManager.CloneRepositoriesAsync(templateProjectId, projectId);

        // Assert
        _mockRepositoryService.Verify(s => s.CreateRepositoryAsync(projectId, "Repo1"), Times.Once);
        _mockRepositoryService.Verify(s => s.DeleteRepositoryAsync(projectId, repositories.Value[0].Id), Times.Once);
    }

    [Fact]
    public async Task CloneTeamsAndSettingsAndBoardsAsync_ShouldCloneTeamsSettingsAndBoards()
    {
        // Arrange
        var templateProject = new Project
        {
            Id = Guid.NewGuid(),
            DefaultTeam = new Team { Id = Guid.NewGuid() },
            Name = "templateProject"
        };
        var project = new Project
        {
            Id = Guid.NewGuid(),
            DefaultTeam = new Team { Id = Guid.NewGuid() }
        };

        var mapTeams = new Dictionary<Guid, Guid>
        {
            { Guid.NewGuid(), Guid.NewGuid() },
            { Guid.NewGuid(), Guid.NewGuid() }
        };

        var teamSettings = new TeamSettings();

        _mockTeamsService.Setup(s => s.GetTeamsAsync(templateProject.Id)).ReturnsAsync(new Teams());
        _mockBoardService.Setup(s => s.GetBoardsAsync(project.Id, It.IsAny<Guid>())).ReturnsAsync(new Boards());
        _mockTeamsService.Setup(s => s.CreateTeamFromTemplateAsync(project.Id, It.IsAny<Team[]>(), templateProject.DefaultTeam.Id, project.DefaultTeam.Id))
            .ReturnsAsync(mapTeams);

        _mockTeamSettingsService.Setup(s => s.GetTeamSettings(project.Id, project.DefaultTeam.Id))
            .ReturnsAsync(teamSettings);

        _mockTeamSettingsService.Setup(s => s.UpdateTeamSettings(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PatchTeamSettings>()))
            .Returns(Task.FromResult(new HttpResponseMessage()));

        _mockTeamSettingsService.Setup(s => s.UpdateTeamFieldValues(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TeamFieldValues>()))
            .Returns(Task.FromResult(new HttpResponseMessage()));

        _mockBoardService.Setup(s => s.MoveBoardColumnsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .ReturnsAsync(new CloneResult());

        _mockBoardService.Setup(s => s.MoveBoardRowsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .ReturnsAsync(new CloneResult());

        _mockBoardService.Setup(s => s.MoveCardSettingsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .ReturnsAsync(new CloneResult());

        _mockBoardService.Setup(s => s.MoveCardStylesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .ReturnsAsync(new CloneResult());

        // Act
        var result = await _cloneManager.CloneTeamsAndSettingsAndBoardsAsync(templateProject, project);

        // Assert
        Assert.True(result.Succeeded, result.ErrorMessage);
        _mockTeamsService.Verify(s => s.CreateTeamFromTemplateAsync(project.Id, It.IsAny<Team[]>(), templateProject.DefaultTeam.Id, project.DefaultTeam.Id), Times.Once);

        foreach (var mappedTeam in mapTeams)
        {
            _mockTeamSettingsService.Verify(s => s.UpdateTeamSettings(project.Id, mappedTeam.Value, It.IsAny<PatchTeamSettings>()), Times.Once);
            _mockTeamSettingsService.Verify(s => s.UpdateTeamFieldValues(project.Id, mappedTeam.Value, It.IsAny<TeamFieldValues>()), Times.Once);

            _mockBoardService.Verify(s => s.MoveBoardColumnsAsync(project.Id, mappedTeam.Value, templateProject.Id, mappedTeam.Key, It.IsAny<Boards>()), Times.Once);
            _mockBoardService.Verify(s => s.MoveBoardRowsAsync(project.Id, mappedTeam.Value, templateProject.Id, mappedTeam.Key, It.IsAny<Boards>()), Times.Once);
            _mockBoardService.Verify(s => s.MoveCardSettingsAsync(project.Id, mappedTeam.Value, templateProject.Id, mappedTeam.Key, It.IsAny<Boards>()), Times.Once);
            _mockBoardService.Verify(s => s.MoveCardStylesAsync(project.Id, mappedTeam.Value, templateProject.Id, mappedTeam.Key, It.IsAny<Boards>()), Times.Once);
        }
    }

    [Fact]
    public async Task CloneTeamSettingsAsync_ShouldCloneTeamSettings()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();
        var templateIterationId = Guid.NewGuid();
        var newIterationId = Guid.NewGuid();

        var templateTeamSettings = new TeamSettings
        {
            BacklogVisibilities = new BacklogVisibilities { EpicCategory = true, FeatureCategory = false, RequirementCategory = true },
            BacklogIteration = new TeamIterationSettings { Id = templateIterationId },
            BugsBehavior = BugsBehavior.AsTasks,
            DefaultIteration = new TeamIterationSettings { Id = templateIterationId },
            DefaultIterationMacro = "macro",
            WorkingDays = [CloneDevOpsTemplate.Models.DayOfWeek.Monday, CloneDevOpsTemplate.Models.DayOfWeek.Tuesday]
        };

        _mockTeamSettingsService.Setup(s => s.GetTeamSettings(templateProjectId, templateTeamId))
            .ReturnsAsync(templateTeamSettings);

        _mockIterationService.Setup(s => s.GetAllAsync(templateProjectId, TreeStructureGroup.Iterations))
            .ReturnsAsync(new Iteration { Identifier = templateIterationId });

        _mockIterationService.Setup(s => s.GetAllAsync(projectId, TreeStructureGroup.Iterations))
            .ReturnsAsync(new Iteration { Identifier = newIterationId });

        _mockTeamSettingsService.Setup(s => s.UpdateTeamSettings(projectId, projectTeamId, It.IsAny<PatchTeamSettings>()))
            .Returns(Task.FromResult(new HttpResponseMessage()));

        // Act
        await _cloneManager.CloneTeamSettingsAsync(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        _mockTeamSettingsService.Verify(s => s.GetTeamSettings(templateProjectId, templateTeamId), Times.Once);
        _mockTeamSettingsService.Verify(s => s.UpdateTeamSettings(projectId, projectTeamId, It.Is<PatchTeamSettings>(settings =>
            settings.BacklogIteration == newIterationId &&
            settings.BacklogVisibilities == templateTeamSettings.BacklogVisibilities &&
            settings.BugsBehavior == templateTeamSettings.BugsBehavior &&
            settings.DefaultIteration == (templateTeamSettings.DefaultIterationMacro == null ? newIterationId : null) &&
            settings.DefaultIterationMacro == templateTeamSettings.DefaultIterationMacro &&
            settings.WorkingDays.SequenceEqual(templateTeamSettings.WorkingDays)
        )), Times.Once);
    }

    [Fact]
    public async Task CloneTeamFieldValuesAsync_ShouldCloneTeamFieldValues()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();

        var templateProject = new Project { Name = "TemplateProject", Id = templateProjectId };
        var project = new Project { Name = "NewProject", Id = projectId };

        var teamFieldValues = new TeamFieldValues
        {
            DefaultValue = "TemplateProject\\Area",
            Values =
            [
                new Values { Value = "TemplateProject\\Area1" },
                new Values { Value = "TemplateProject\\Area2" }
            ]
        };

        _mockProjectService.Setup(s => s.GetProjectAsync(templateProjectId)).ReturnsAsync(templateProject);
        _mockProjectService.Setup(s => s.GetProjectAsync(projectId)).ReturnsAsync(project);
        _mockTeamSettingsService.Setup(s => s.GetTeamFieldValues(templateProjectId, templateTeamId)).ReturnsAsync(teamFieldValues);

        _mockTeamSettingsService.Setup(s => s.UpdateTeamFieldValues(projectId, projectTeamId, It.IsAny<TeamFieldValues>()))
            .Returns(Task.FromResult(new HttpResponseMessage()));

        // Act
        await _cloneManager.CloneTeamFieldValuesAsync(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        _mockTeamSettingsService.Verify(s => s.GetTeamFieldValues(templateProjectId, templateTeamId), Times.Once);
        _mockTeamSettingsService.Verify(s => s.UpdateTeamFieldValues(projectId, projectTeamId, It.Is<TeamFieldValues>(values =>
            values.DefaultValue == "NewProject\\Area" &&
            values.Values.Any(v => v.Value == "NewProject\\Area1") &&
            values.Values.Any(v => v.Value == "NewProject\\Area2")
        )), Times.Once);
    }

    [Fact]
    public void MapClassificationNodes_ShouldMapNodesCorrectly()
    {
        // Arrange
        var templateIterations = new Iteration
        {
            Children =
            [
                new Iteration
                {
                    Name = "Iteration1",
                    Identifier = Guid.NewGuid(),
                    Children =
                    [
                        new Iteration
                        {
                            Name = "SubIteration1",
                            Identifier = Guid.NewGuid()
                        }
                    ]
                },
                new Iteration
                {
                    Name = "Iteration2",
                    Identifier = Guid.NewGuid()
                }
            ]
        };

        var iterations = new Iteration
        {
            Children =
            [
                new Iteration
                {
                    Name = "Iteration1",
                    Identifier = Guid.NewGuid(),
                    Children =
                    [
                        new Iteration
                        {
                            Name = "SubIteration1",
                            Identifier = Guid.NewGuid()
                        }
                    ]
                },
                new Iteration
                {
                    Name = "Iteration2",
                    Identifier = Guid.NewGuid()
                }
            ]
        };

        var iterationMap = new Dictionary<Guid, Guid>();

        // Act
        CloneManager.MapClassificationNodes(templateIterations, iterations, iterationMap);

        // Assert
        Assert.Equal(3, iterationMap.Count);
        Assert.Equal(iterations.Children[0].Identifier, iterationMap[templateIterations.Children[0].Identifier]);
        Assert.Equal(iterations.Children[0].Children[0].Identifier, iterationMap[templateIterations.Children[0].Children[0].Identifier]);
        Assert.Equal(iterations.Children[1].Identifier, iterationMap[templateIterations.Children[1].Identifier]);
    }

    [Fact]
    public void MapClassificationNodes_ShouldHandleEmptyChildren()
    {
        // Arrange
        var templateIterations = new Iteration
        {
            Children = []
        };

        var iterations = new Iteration
        {
            Children = []
        };

        var iterationMap = new Dictionary<Guid, Guid>();

        // Act
        CloneManager.MapClassificationNodes(templateIterations, iterations, iterationMap);

        // Assert
        Assert.Empty(iterationMap);
    }

    [Fact]
    public void MapClassificationNodes_ShouldNotMapIfNamesDoNotMatch()
    {
        // Arrange
        var templateIterations = new Iteration
        {
            Children =
            [
                new Iteration
                {
                    Name = "Iteration1",
                    Identifier = Guid.NewGuid()
                }
            ]
        };

        var iterations = new Iteration
        {
            Children =
            [
                new Iteration
                {
                    Name = "DifferentIteration",
                    Identifier = Guid.NewGuid()
                }
            ]
        };

        var iterationMap = new Dictionary<Guid, Guid>();

        // Act
        CloneManager.MapClassificationNodes(templateIterations, iterations, iterationMap);

        // Assert
        Assert.Empty(iterationMap);
    }

    [Fact]
    public async Task CloneTeamIterationsAsync_ShouldCloneTeamIterations()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();

        var oldIterations = new TeamIterations
        {
            Value =
            [
                new TeamIterationSettings { Id = Guid.NewGuid() }
            ]
        };

        var templateIterations = new TeamIterations
        {
            Value =
            [
                new TeamIterationSettings { Id = Guid.NewGuid() }
            ]
        };

        var iterationMap = new Dictionary<Guid, Guid>
        {
            { templateIterations.Value[0].Id, Guid.NewGuid() }
        };

        _mockTeamSettingsService.Setup(s => s.GetIterations(projectId, projectTeamId)).ReturnsAsync(oldIterations);
        _mockTeamSettingsService.Setup(s => s.GetIterations(templateProjectId, templateTeamId)).ReturnsAsync(templateIterations);
        _mockIterationService.Setup(s => s.GetAllAsync(templateProjectId, TreeStructureGroup.Iterations))
            .ReturnsAsync(new Iteration { Identifier = templateIterations.Value[0].Id });
        _mockIterationService.Setup(s => s.GetAllAsync(projectId, TreeStructureGroup.Iterations))
            .ReturnsAsync(new Iteration { Identifier = iterationMap[templateIterations.Value[0].Id] });

        _mockTeamSettingsService.Setup(s => s.DeleteIteration(projectId, projectTeamId, oldIterations.Value[0].Id))
            .Returns(Task.FromResult(new HttpResponseMessage()));
        _mockTeamSettingsService.Setup(s => s.CreateIteration(projectId, projectTeamId, iterationMap[templateIterations.Value[0].Id]))
            .Returns(Task.FromResult(new HttpResponseMessage()));

        // Act
        await _cloneManager.CloneTeamIterationsAsync(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        _mockTeamSettingsService.Verify(s => s.GetIterations(projectId, projectTeamId), Times.Once);
        _mockTeamSettingsService.Verify(s => s.DeleteIteration(projectId, projectTeamId, oldIterations.Value[0].Id), Times.Once);
        _mockTeamSettingsService.Verify(s => s.GetIterations(templateProjectId, templateTeamId), Times.Once);
        _mockTeamSettingsService.Verify(s => s.CreateIteration(projectId, projectTeamId, iterationMap[templateIterations.Value[0].Id]), Times.Once);
    }

    [Fact]
    public async Task CloneClassificationNodes_ShouldCloneNodesCorrectly()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var structureGroup = TreeStructureGroup.Iterations;

        var existingClassificationNodes = new Iteration
        {
            Identifier = Guid.NewGuid(),
            Children = []
        };

        var templateClassificationNodes = new Iteration
        {
            Identifier = Guid.NewGuid(),
            Children =
            [
                new Iteration
                {
                    Name = "Iteration1",
                    Identifier = Guid.NewGuid()
                }
            ]
        };

        _mockIterationService.Setup(s => s.GetAllAsync(projectId, structureGroup)).ReturnsAsync(existingClassificationNodes);
        _mockIterationService.Setup(s => s.GetAllAsync(templateProjectId, structureGroup)).ReturnsAsync(templateClassificationNodes);

        // Act
        await _cloneManager.CloneClassificationNodes(templateProjectId, projectId, structureGroup);

        // Assert
        _mockIterationService.Verify(s => s.GetAllAsync(projectId, structureGroup), Times.Once);
        _mockIterationService.Verify(s => s.DeleteAsync(projectId, structureGroup, existingClassificationNodes), Times.Once);
        _mockIterationService.Verify(s => s.GetAllAsync(templateProjectId, structureGroup), Times.Once);
        _mockIterationService.Verify(s => s.CreateAsync(projectId, templateClassificationNodes, structureGroup, ""), Times.Once);
    }

    [Fact]
    public async Task CloneGitPullRequestsAsync_ShouldClonePullRequests()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var templateRepositoryId = Guid.NewGuid();
        var mappedRepositoryId = Guid.NewGuid();

        var templatePullRequests = new GitPullRequests
        {
            Value =
            [
                new GitPullRequest
                {
                    Repository = new Repository { Id = templateRepositoryId },
                    Title = "Sample Pull Request"
                }
            ]
        };

        _mockRepositoryService.Setup(s => s.GetGitPullRequestAsync(templateProjectId))
            .ReturnsAsync(templatePullRequests);

        _mockRepositoryService.Setup(s => s.GetRepositoriesAsync(templateProjectId))
            .ReturnsAsync(new Repositories { Value = [new Repository { Name = "Repo1", Id = templateRepositoryId }] });

        _mockRepositoryService.Setup(s => s.GetRepositoriesAsync(projectId))
            .ReturnsAsync(new Repositories { Value = [new Repository { Name = "Repo1", Id = mappedRepositoryId }] });

        // Act
        await _cloneManager.CloneGitPullRequestsAsync(templateProjectId, projectId);

        // Assert
        _mockRepositoryService.Verify(s => s.GetGitPullRequestAsync(templateProjectId), Times.Once);
        _mockRepositoryService.Verify(s => s.CreateGitPullRequestAsync(projectId, mappedRepositoryId, It.IsAny<GitPullRequest>()), Times.Once);
    }
    [Fact]
    public async Task CloneGitPullRequestsAsync_ReportsProgressDetails()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var progress = new RecordingProgress();

        _mockRepositoryService.Setup(s => s.GetGitPullRequestAsync(templateProjectId))
            .ReturnsAsync(new GitPullRequests { Value = [new GitPullRequest { Repository = new Repository(), Title = "PR1" }] });

        // Act
        await _cloneManager.CloneGitPullRequestsAsync(templateProjectId, projectId, progress);

        // Assert
        Assert.Contains("Pull requests: 1 pull request(s) to clone.", progress.Messages);
        Assert.Contains("Pull request 'PR1' (1/1): creating...", progress.Messages);
    }

    [Fact]
    public async Task CloneClassificationNodes_ReportsNodeCounts()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var progress = new RecordingProgress();

        _mockIterationService.Setup(s => s.GetAllAsync(projectId, TreeStructureGroup.Areas))
            .ReturnsAsync(new Iteration { Children = [new Iteration()] });
        _mockIterationService.Setup(s => s.GetAllAsync(templateProjectId, TreeStructureGroup.Areas))
            .ReturnsAsync(new Iteration { Children = [new Iteration { Children = [new Iteration(), new Iteration()] }] });

        // Act
        await _cloneManager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Areas, progress);

        // Assert
        Assert.Equal(
            ["Areas: removing 1 default node(s)...", "Areas: creating 3 node(s) from template...", "Areas: done."],
            progress.Messages);
    }

    [Fact]
    public async Task CloneTeamsAndSettingsAndBoardsAsync_ReportsTeamNames()
    {
        // Arrange
        var templateTeamId = Guid.NewGuid();
        var templateProject = new Project { Id = Guid.NewGuid(), Name = "Template", DefaultTeam = new Team { Id = Guid.NewGuid() } };
        var project = new Project { Id = Guid.NewGuid(), DefaultTeam = new Team { Id = Guid.NewGuid() } };
        var templateTeams = new Teams { Value = [new Team { Id = templateTeamId, Name = "Alpha" }] };
        var progress = new RecordingProgress();

        _mockTeamsService.Setup(s => s.GetTeamsAsync(templateProject.Id)).ReturnsAsync(templateTeams);
        _mockTeamsService.Setup(s => s.CreateTeamFromTemplateAsync(project.Id, templateTeams.Value, templateProject.DefaultTeam.Id, project.DefaultTeam.Id))
            .ReturnsAsync(new Dictionary<Guid, Guid> { { templateTeamId, Guid.NewGuid() } });

        // Act
        await _cloneManager.CloneTeamsAndSettingsAndBoardsAsync(templateProject, project, progress);

        // Assert
        Assert.Contains("Teams: 1 team(s) mapped.", progress.Messages);
        Assert.Contains("Team 'Alpha' (1/1): cloning boards...", progress.Messages);
        Assert.Contains("Team 'Alpha' (1/1): done.", progress.Messages);
    }

    [Fact]
    public async Task CloneClassificationNodes_TemplateNodesMissing_ReturnsErrorWithoutDeleting()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        _mockIterationService.Setup(s => s.GetAllAsync(projectId, TreeStructureGroup.Iterations)).ReturnsAsync(new Iteration());

        // Act
        var result = await _cloneManager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(["Iterations: could not read the template project's nodes."], result.Errors);
        _mockIterationService.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<TreeStructureGroup>(), It.IsAny<Iteration>()), Times.Never);
    }

    [Fact]
    public async Task CloneTeamsAndSettingsAndBoardsAsync_UpdateFails_ReturnsErrorWithDevOpsMessage()
    {
        // Arrange
        var templateTeamId = Guid.NewGuid();
        var templateProject = new Project { Id = Guid.NewGuid(), Name = "Template", DefaultTeam = new Team { Id = Guid.NewGuid() } };
        var project = new Project { Id = Guid.NewGuid(), Name = "New", DefaultTeam = new Team { Id = Guid.NewGuid() } };
        var templateTeams = new Teams { Value = [new Team { Id = templateTeamId, Name = "Alpha" }] };
        var progress = new RecordingProgress();

        _mockTeamsService.Setup(s => s.GetTeamsAsync(templateProject.Id)).ReturnsAsync(templateTeams);
        _mockTeamsService.Setup(s => s.CreateTeamFromTemplateAsync(project.Id, templateTeams.Value, templateProject.DefaultTeam.Id, project.DefaultTeam.Id))
            .ReturnsAsync(new Dictionary<Guid, Guid> { { templateTeamId, Guid.NewGuid() } });
        _mockTeamSettingsService.Setup(s => s.UpdateTeamSettings(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PatchTeamSettings>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "Bad Request",
                Content = JsonContent.Create(new ErrorResponse { Message = "TF400898: Invalid iteration." })
            });
        _mockTeamSettingsService.Setup(s => s.UpdateTeamFieldValues(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TeamFieldValues>()))
            .ReturnsAsync(new HttpResponseMessage());
        _mockBoardService.Setup(s => s.GetBoardsAsync(project.Id, It.IsAny<Guid>())).ReturnsAsync(new Boards());

        // Act
        var result = await _cloneManager.CloneTeamsAndSettingsAndBoardsAsync(templateProject, project, progress);

        // Assert
        var error = "Team 'Alpha': updating settings failed (400 Bad Request: TF400898: Invalid iteration.).";
        Assert.Equal([error], result.Errors);
        Assert.Contains($"Error: {error}", progress.Messages);
    }

    [Fact]
    public async Task CloneRepositoriesAsync_ImportFails_ReturnsErrorWithoutWaitingForTimeout()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var repositoryId = Guid.NewGuid();

        _mockRepositoryService.Setup(s => s.GetRepositoriesAsync(templateProjectId))
            .ReturnsAsync(new Repositories { Value = [new Repository { Name = "Api", RemoteUrl = "http://example.com/api" }] });
        _mockRepositoryService.Setup(s => s.GetRepositoriesAsync(projectId))
            .ReturnsAsync(new Repositories { Value = [new Repository { Id = repositoryId, Name = "Api" }] });
        _mockServiceService.Setup(s => s.GetServicesAsync(projectId))
            .ReturnsAsync(new ServicesModel { Value = [new ServiceModel { Name = "Api" }] });
        _mockRepositoryService.Setup(s => s.CreateImportRequestAsync(projectId, repositoryId, It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(new GitImportRequest { ImportRequestId = 7 });
        _mockRepositoryService.Setup(s => s.GetImportRequestAsync(projectId, repositoryId, 7))
            .ReturnsAsync(new GitImportRequest { Status = GitAsyncOperationStatus.Failed });
        _mockRepositoryService.Setup(s => s.DeleteRepositoryAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new HttpResponseMessage());

        // Act
        var result = await _cloneManager.CloneRepositoriesAsync(templateProjectId, projectId);

        // Assert
        Assert.Equal(["Repository 'Api': import Failed."], result.Errors);
    }

    [Fact]
    public async Task CloneRepositoriesAsync_CreateRepositoryFails_ReturnsErrorAndSkipsImport()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _mockRepositoryService.Setup(s => s.GetRepositoriesAsync(templateProjectId))
            .ReturnsAsync(new Repositories { Value = [new Repository { Name = "Api" }] });
        _mockRepositoryService.Setup(s => s.GetRepositoriesAsync(projectId))
            .ReturnsAsync(new Repositories());

        // Act
        var result = await _cloneManager.CloneRepositoriesAsync(templateProjectId, projectId);

        // Assert
        Assert.Equal(["Repository 'Api': could not be created."], result.Errors);
        _mockRepositoryService.Verify(s => s.CreateImportRequestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CloneGitPullRequestsAsync_ReportsUnmappedRepositoryAndFailedCreation()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var templateRepositoryId = Guid.NewGuid();

        _mockRepositoryService.Setup(s => s.GetGitPullRequestAsync(templateProjectId))
            .ReturnsAsync(new GitPullRequests
            {
                Value =
                [
                    new GitPullRequest { Title = "Mapped", Repository = new Repository { Id = templateRepositoryId, Name = "Api" } },
                    new GitPullRequest { Title = "Orphan", Repository = new Repository { Id = Guid.NewGuid(), Name = "Gone" } }
                ]
            });
        _mockRepositoryService.Setup(s => s.GetRepositoriesAsync(templateProjectId))
            .ReturnsAsync(new Repositories { Value = [new Repository { Id = templateRepositoryId, Name = "Api" }] });
        _mockRepositoryService.Setup(s => s.GetRepositoriesAsync(projectId))
            .ReturnsAsync(new Repositories { Value = [new Repository { Id = Guid.NewGuid(), Name = "Api" }] });
        _mockRepositoryService.Setup(s => s.CreateGitPullRequestAsync(projectId, It.IsAny<Guid>(), It.IsAny<GitPullRequestBase>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Conflict) { ReasonPhrase = "Conflict" });

        // Act
        var result = await _cloneManager.CloneGitPullRequestsAsync(templateProjectId, projectId);

        // Assert
        Assert.Equal(
            ["Pull request 'Mapped': creating failed (409 Conflict).", "Pull request 'Orphan': repository 'Gone' not found in the target project."],
            result.Errors);
        _mockRepositoryService.Verify(s => s.CreateGitPullRequestAsync(projectId, It.IsAny<Guid>(), It.IsAny<GitPullRequestBase>()), Times.Once);
    }

    [Fact]
    public async Task CloneTeamsAsync_TeamNotCreated_ReturnsErrorAndDropsMapping()
    {
        // Arrange
        var failedTeamId = Guid.NewGuid();
        var createdTeamId = Guid.NewGuid();
        var templateProject = new Project { Id = Guid.NewGuid(), DefaultTeam = new Team { Id = Guid.NewGuid() } };
        var project = new Project { Id = Guid.NewGuid(), DefaultTeam = new Team { Id = Guid.NewGuid() } };
        var templateTeams = new Teams { Value = [new Team { Id = failedTeamId, Name = "Broken" }, new Team { Id = createdTeamId, Name = "Fine" }] };

        _mockTeamsService.Setup(s => s.GetTeamsAsync(templateProject.Id)).ReturnsAsync(templateTeams);
        _mockTeamsService.Setup(s => s.CreateTeamFromTemplateAsync(project.Id, templateTeams.Value, templateProject.DefaultTeam.Id, project.DefaultTeam.Id))
            .ReturnsAsync(new Dictionary<Guid, Guid> { { failedTeamId, Guid.Empty }, { createdTeamId, Guid.NewGuid() } });

        // Act
        var result = await _cloneManager.CloneTeamsAsync(templateProject, project);

        // Assert
        Assert.Equal(["Team 'Broken': could not be created."], result.Errors);
        Assert.Equal([createdTeamId], result.TeamMap.Keys);
    }

    [Fact]
    public async Task CloneTeamsAsync_ProjectNotFound_ReturnsError()
    {
        // Act
        var result = await _cloneManager.CloneTeamsAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(["Could not read the template project."], result.Errors);
        _mockTeamsService.Verify(s => s.CreateTeamFromTemplateAsync(It.IsAny<Guid>(), It.IsAny<Team[]>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CloneTeamIterationsAsync_CreateIterationFails_ReturnsError()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var templateTeamId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();
        var templateIterationId = Guid.NewGuid();

        _mockTeamSettingsService.Setup(s => s.GetIterations(templateProjectId, templateTeamId))
            .ReturnsAsync(new TeamIterations { Value = [new TeamIterationSettings { Id = templateIterationId, Name = "Sprint 1" }] });
        _mockIterationService.Setup(s => s.GetAllAsync(templateProjectId, TreeStructureGroup.Iterations))
            .ReturnsAsync(new Iteration { Identifier = Guid.NewGuid(), Children = [new Iteration { Name = "Sprint 1", Identifier = templateIterationId }] });
        _mockIterationService.Setup(s => s.GetAllAsync(projectId, TreeStructureGroup.Iterations))
            .ReturnsAsync(new Iteration { Identifier = Guid.NewGuid(), Children = [new Iteration { Name = "Sprint 1", Identifier = Guid.NewGuid() }] });
        _mockTeamSettingsService.Setup(s => s.CreateIteration(projectId, projectTeamId, It.IsAny<Guid>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden) { ReasonPhrase = "Forbidden" });

        // Act
        var result = await _cloneManager.CloneTeamIterationsAsync(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        Assert.Equal(["Team: adding iteration 'Sprint 1' failed (403 Forbidden)."], result.Errors);
    }

    [Fact]
    public async Task CloneBoardsAsync_BoardsMissing_ReturnsErrorWithoutMoving()
    {
        // Act
        var result = await _cloneManager.CloneBoardsAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(["Team: could not read the target team's boards."], result.Errors);
        _mockBoardService.Verify(s => s.MoveBoardColumnsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()), Times.Never);
    }

    [Fact]
    public async Task CloneClassificationNodes_ServiceErrors_ArePrefixedAndReported()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var progress = new RecordingProgress();
        _mockIterationService.Setup(s => s.GetAllAsync(It.IsAny<Guid>(), TreeStructureGroup.Areas)).ReturnsAsync(new Iteration());
        _mockIterationService.Setup(s => s.CreateAsync(projectId, It.IsAny<Iteration>(), TreeStructureGroup.Areas, ""))
            .ReturnsAsync(CloneResult.FromError("Creating 'Web' failed (400 Bad Request)."));

        // Act
        var result = await _cloneManager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Areas, progress);

        // Assert
        Assert.Equal(["Areas: Creating 'Web' failed (400 Bad Request)."], result.Errors);
        Assert.Contains("Error: Areas: Creating 'Web' failed (400 Bad Request).", progress.Messages);
    }

    [Fact]
    public async Task CloneBoardsAsync_MoveErrors_AreCollectedOnce()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectTeamId = Guid.NewGuid();
        var readError = CloneResult.FromError("Could not read the template team's boards.");
        _mockBoardService.Setup(s => s.GetBoardsAsync(projectId, projectTeamId)).ReturnsAsync(new Boards());
        _mockBoardService.Setup(s => s.MoveBoardColumnsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>())).ReturnsAsync(readError);
        _mockBoardService.Setup(s => s.MoveBoardRowsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>())).ReturnsAsync(readError);
        _mockBoardService.Setup(s => s.MoveCardStylesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .ReturnsAsync(CloneResult.FromError("Board 'Stories': updating card styles failed (403 Forbidden)."));

        // Act
        var result = await _cloneManager.CloneBoardsAsync(Guid.NewGuid(), projectId, Guid.NewGuid(), projectTeamId);

        // Assert
        Assert.Equal(
            ["Team: Could not read the template team's boards.", "Team: Board 'Stories': updating card styles failed (403 Forbidden)."],
            result.Errors);
    }

    private sealed class RecordingProgress : IProgress<string>
    {
        public List<string> Messages { get; } = [];
        public void Report(string value)
        {
            lock (Messages)
            {
                Messages.Add(value);
            }
        }
    }
}
