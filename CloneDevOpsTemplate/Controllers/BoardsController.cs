using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class BoardsController(IBoardService boardService) : Controller
{
    private readonly IBoardService _boardService = boardService;

    public async Task<IActionResult> Boards(Guid projectId, Guid teamId)
    {
        List<ViewBoard> boards = [];

        if (!ModelState.IsValid)
        {
            return View(boards.ToArray());
        }

        Boards boardValues = await _boardService.GetBoardsAsync(projectId, teamId) ?? new();
        foreach (var board in boardValues.Value)
        {
            var currentBoard = await _boardService.GetBoardAsync(projectId, teamId, board.Id) ?? new();

            // Query rows separately, since row colors are ALWAYS null from the GetBoard call
            var rows = await _boardService.GetBoardRowsAsync(projectId, teamId, currentBoard.Id) ?? new();
            currentBoard.Rows = rows.Value;
            boards.Add(new() { Board = currentBoard, ProjectId = projectId, TeamId = teamId });
        }
        return View(boards.ToArray());
    }

    public async Task<IActionResult> CardSettings(Guid projectId, Guid teamId, Guid boardId)
    {
        CardSettings cardSettings = new();

        if (!ModelState.IsValid)
        {
            return View(cardSettings.Cards);
        }

        cardSettings = await _boardService.GetCardSettingsAsync(projectId, teamId, boardId) ?? new();
        return View(cardSettings.Cards);
    }

    public async Task<IActionResult> CardStyles(Guid projectId, Guid teamId, Guid boardId)
    {
        CardStyles cardStyles = new();

        if (!ModelState.IsValid)
        {
            return View(cardStyles.Rules);
        }

        cardStyles = await _boardService.GetCardStylesAsync(projectId, teamId, boardId) ?? new();
        return View(cardStyles.Rules);
    }
}

