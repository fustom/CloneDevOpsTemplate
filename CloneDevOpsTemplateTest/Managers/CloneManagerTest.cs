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
        Assert.Equal(newProject, result.Item1);
        Assert.Equal(templateProject, result.Item2);
        Assert.Equal("Project created successfully", result.Item3);
    }

    [Fact]
    public async Task CloneIterationsAsync_ShouldCloneIterations()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var templateIterations = new Iteration();
        var createdIterations = new Iteration { Children = new List<Iteration>() };

        _mockIterationService.Setup(s => s.GetAllAsync(templateProjectId, TreeStructureGroup.Iterations)).ReturnsAsync(templateIterations);
        _mockIterationService.Setup(s => s.CreateAsync(projectId, templateIterations, TreeStructureGroup.Iterations)).ReturnsAsync(createdIterations);

        // Act
        await _cloneManager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Iterations);

        // Assert
        _mockIterationService.Verify(s => s.MoveAsync(projectId, createdIterations.Children, TreeStructureGroup.Iterations, ""), Times.Once);
    }

    [Fact]
    public async Task CloneAreasAsync_ShouldCloneAreas()
    {
        // Arrange
        var templateProjectId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var templateAreas = new Iteration();
        var createdAreas = new Iteration { Children = new List<Iteration>() };

        _mockIterationService.Setup(s => s.GetAllAsync(templateProjectId, TreeStructureGroup.Areas)).ReturnsAsync(templateAreas);
        _mockIterationService.Setup(s => s.CreateAsync(projectId, templateAreas, TreeStructureGroup.Areas)).ReturnsAsync(createdAreas);

        // Act
        await _cloneManager.CloneClassificationNodes(templateProjectId, projectId, TreeStructureGroup.Areas);

        // Assert
        _mockIterationService.Verify(s => s.MoveAsync(projectId, createdAreas.Children, TreeStructureGroup.Areas, ""), Times.Once);
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

        _mockTeamsService.Setup(s => s.CreateTeamFromTemplateAsync(project.Id, It.IsAny<Team[]>(), templateProject.DefaultTeam.Id, project.DefaultTeam.Id))
            .ReturnsAsync(mapTeams);

        _mockTeamSettingsService.Setup(s => s.GetTeamSettings(project.Id, project.DefaultTeam.Id))
            .ReturnsAsync(teamSettings);

        _mockTeamSettingsService.Setup(s => s.UpdateTeamSettings(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PatchTeamSettings>()))
            .Returns(Task.FromResult(new HttpResponseMessage()));

        _mockTeamSettingsService.Setup(s => s.UpdateTeamFieldValues(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TeamFieldValues>()))
            .Returns(Task.FromResult(new HttpResponseMessage()));

        _mockBoardService.Setup(s => s.MoveBoardColumnsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .Returns(Task.CompletedTask);

        _mockBoardService.Setup(s => s.MoveBoardRowsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .Returns(Task.CompletedTask);

        _mockBoardService.Setup(s => s.MoveCardSettingsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .Returns(Task.CompletedTask);

        _mockBoardService.Setup(s => s.MoveCardStylesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Boards>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cloneManager.CloneTeamsAndSettingsAndBoardsAsync(templateProject, project);

        // Assert
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
}