using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CloneDevOpsTemplateTest.Controllers;

public class BoardsControllerTest
{
    private readonly Mock<IBoardService> _mockBoardService;
    private readonly Mock<ICloneManager> _mockCloneManager;
    private readonly Mock<ITeamsService> _mockTeamsService;
    private readonly MigrationProgressTracker _progressTracker = new();
    private readonly BoardsController _controller;

    public BoardsControllerTest()
    {
        _mockBoardService = new Mock<IBoardService>();
        _mockCloneManager = new Mock<ICloneManager>();
        _mockTeamsService = new Mock<ITeamsService>();
        _controller = new BoardsController(_mockBoardService.Object, _mockCloneManager.Object, _mockTeamsService.Object, _progressTracker);
    }

    [Fact]
    public async Task Boards_InvalidModelState_ReturnsEmptyView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.Boards(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ViewBoard[]>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Boards_ValidModelState_ReturnsBoardsView()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var boardRowId = Guid.NewGuid();

        var mockBoards = new Boards
        {
            Value =
            [
                new BoardValue { Id = boardId }
            ]
        };

        var mockBoard = new Board
        {
            Id = boardId,
            Rows = []
        };

        var mockRows = new BoardRows
        {
            Value =
            [
                new BoardRow { Id = boardRowId }
            ]
        };

        _mockBoardService.Setup(s => s.GetBoardsAsync(projectId, teamId))
            .ReturnsAsync(mockBoards);
        _mockBoardService.Setup(s => s.GetBoardAsync(projectId, teamId, boardId))
            .ReturnsAsync(mockBoard);
        _mockBoardService.Setup(s => s.GetBoardRowsAsync(projectId, teamId, boardId))
            .ReturnsAsync(mockRows);

        // Act
        var result = await _controller.Boards(projectId, teamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ViewBoard[]>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal(projectId, model[0].ProjectId);
        Assert.Equal(teamId, model[0].TeamId);
        Assert.Equal(boardId, model[0].Board.Id);
        Assert.Single(model[0].Board.Rows);
    }

    [Fact]
    public async Task CardSettings_InvalidModelState_ReturnsEmptyView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CardSettings(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Cards>(viewResult.Model);
        Assert.Null(model.Bug);
        Assert.Null(model.Epic);
        Assert.Null(model.Feature);
        Assert.Null(model.Issue);
        Assert.Null(model.ProductBacklogItem);
        Assert.Null(model.UserStory);
    }

    [Fact]
    public async Task CardSettings_ValidModelState_ReturnsCardSettingsView()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();

        var mockCardSettings = new CardSettings
        {
            Cards = new Cards
            {
                Bug =
                [
                    new CardItem
                    {
                        FieldIdentifier = "bugCard1",
                        DisplayFormat = "bugCardFormat1"
                    },
                    new CardItem
                    {
                        FieldIdentifier = "bugCard2",
                        DisplayFormat = "bugCardFormat2"
                    }
                ]
            },
        };

        _mockBoardService.Setup(s => s.GetCardSettingsAsync(projectId, teamId, boardId))
            .ReturnsAsync(mockCardSettings);

        // Act
        var result = await _controller.CardSettings(projectId, teamId, boardId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Cards>(viewResult.Model);
        Assert.NotNull(model.Bug);
        Assert.Equal(2, model.Bug.Length);
        Assert.Equal("bugCard1", model.Bug[0].FieldIdentifier);
        Assert.Equal("bugCard2", model.Bug[1].FieldIdentifier);
        Assert.Equal("bugCardFormat1", model.Bug[0].DisplayFormat);
        Assert.Equal("bugCardFormat2", model.Bug[1].DisplayFormat);
    }

    [Fact]
    public async Task CardStyles_InvalidModelState_ReturnsEmptyView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CardStyles(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Rules>(viewResult.Model);
        Assert.Empty(model.Fill);
        Assert.Empty(model.TagStyle);
    }

    [Fact]
    public async Task CardStyles_ValidModelState_ReturnsCardStylesView()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var boardId = Guid.NewGuid();

        var mockCardStyles = new CardStyles
        {
            Rules = new Rules
            {
                Fill =
                [
                    new Fill
                    {
                        Name = "rule1",
                        Clauses =
                        [
                            new Clause
                            {
                                FieldName = "fieldName",
                                Index = 1,
                                LogicalOperator = "&",
                                Operator = "and",
                                Value = "value"
                            }
                        ]
                    },
                    new Fill
                    {
                        Name = "rule2"
                    }
                ],
                TagStyle =
                [
                    new TagStyle
                    {
                        Name = "tagStyle1",
                        Settings = new()
                        {
                            BackgroundColor = "blue",
                            TitleColor = "black"
                        },
                        IsEnabled = "true"
                    }
                ]
            }
        };

        _mockBoardService.Setup(s => s.GetCardStylesAsync(projectId, teamId, boardId))
            .ReturnsAsync(mockCardStyles);

        // Act
        var result = await _controller.CardStyles(projectId, teamId, boardId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Rules>(viewResult.Model);
        Assert.Equal(2, model.Fill.Length);
        Assert.Equal("rule1", model.Fill[0].Name);
        Assert.Equal("rule2", model.Fill[1].Name);
        Assert.Equal("fieldName", model.Fill[0].Clauses[0].FieldName);
        Assert.Equal(1, model.Fill[0].Clauses[0].Index);
        Assert.Equal("&", model.Fill[0].Clauses[0].LogicalOperator);
        Assert.Equal("and", model.Fill[0].Clauses[0].Operator);
        Assert.Equal("value", model.Fill[0].Clauses[0].Value);
        Assert.Equal("blue", model.TagStyle[0].Settings.BackgroundColor);
        Assert.Equal("black", model.TagStyle[0].Settings.TitleColor);
        Assert.Equal("tagStyle1", model.TagStyle[0].Name);
        Assert.Equal("true", model.TagStyle[0].IsEnabled);
    }

    [Fact]
    public async Task CloneBoards_Get_ReturnsCloneFormWithItems()
    {
        // Arrange
        _mockTeamsService.Setup(s => s.GetAllTeamsAsync()).ReturnsAsync(new Teams { Value = [new Team { Name = "Source" }] });

        // Act
        var result = await _controller.CloneBoards();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(BoardsController.CloneBoards), viewResult.ViewName);
        var model = Assert.IsType<CloneFormViewModel<Team>>(viewResult.Model);
        Assert.Equal("Source", Assert.Single(model.Items).Name);
        Assert.Equal("Boards", model.Controller);
        Assert.Equal(nameof(BoardsController.CloneBoards), model.Action);
        Assert.Null(model.FinishedMigration);
    }

    [Fact]
    public async Task CloneBoards_Get_NullItems_ReturnsEmptyCloneForm()
    {
        // Arrange
        _mockTeamsService.Setup(s => s.GetAllTeamsAsync()).ReturnsAsync((Teams?)null);

        // Act
        var result = await _controller.CloneBoards();

        // Assert
        var model = Assert.IsType<CloneFormViewModel<Team>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Items);
    }

    [Fact]
    public async Task CloneBoards_Get_WithFinishedOperation_ShowsItOnce()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        _progressTracker.Fail(operationId, "Boom");

        // Act
        var result = await _controller.CloneBoards(operationId);

        // Assert
        var model = Assert.IsType<CloneFormViewModel<Team>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(MigrationStatus.Failed, model.FinishedMigration?.Status);
        Assert.Null(_progressTracker.Get(operationId));
    }

    [Fact]
    public async Task CloneBoards_Post_InvalidModelState_ReturnsCloneFormWithoutCloning()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = await _controller.CloneBoards(templateProjectId, projectId, templateTeamId, projectTeamId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(BoardsController.CloneBoards), viewResult.ViewName);
        Assert.IsType<CloneFormViewModel<Team>>(viewResult.Model);
        Assert.Empty(_mockCloneManager.Invocations);
    }

    [Fact]
    public async Task CloneBoards_Post_Success_CompletesProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        _mockCloneManager.Setup(m => m.CloneBoardsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>())).ReturnsAsync(new CloneResult());

        // Act
        var result = await _controller.CloneBoards(templateProjectId, projectId, templateTeamId, projectTeamId, operationId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BoardsController.CloneBoards), redirectResult.ActionName);
        Assert.Equal(operationId, redirectResult.RouteValues?["operationId"]);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Succeeded, progress?.Status);
        Assert.Equal("Boards cloned successfully.", progress?.Message);
        _mockCloneManager.Verify(m => m.CloneBoardsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>()), Times.Once);
    }

    [Fact]
    public async Task CloneBoards_Post_Failure_FailsProgressAndRedirects()
    {
        // Arrange
        Guid templateProjectId = Guid.NewGuid(), projectId = Guid.NewGuid(), templateTeamId = Guid.NewGuid(), projectTeamId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var failure = new CloneResult();
        failure.AddError("Boom");
        _mockCloneManager.Setup(m => m.CloneBoardsAsync(templateProjectId, projectId, templateTeamId, projectTeamId, It.IsAny<IProgress<string>?>())).ReturnsAsync(failure);

        // Act
        var result = await _controller.CloneBoards(templateProjectId, projectId, templateTeamId, projectTeamId, operationId);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        var progress = _progressTracker.Get(operationId);
        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Equal("Cloning boards failed: Boom", progress?.Message);
        Assert.Equal(0, _controller.ModelState.ErrorCount);
    }
}
