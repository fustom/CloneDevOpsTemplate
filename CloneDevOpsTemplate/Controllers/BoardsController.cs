using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers
{
    public class BoardsController(IBoardService boardService) : Controller
    {
        private readonly IBoardService _boardService = boardService;

        async public Task<IActionResult> Boards(Guid projectId, Guid teamId)
        {
            Boards boardValues = await _boardService.GetBoardsAsync(projectId, teamId) ?? new Boards();
            List<Board> boards = new List<Board>();
            foreach (var board in boardValues.Value)
            {
                var currentBoard = await _boardService.GetBoardAsync(projectId, teamId, board.Id);
                if (currentBoard != null)
                {
                    boards.Add(currentBoard);
                }
            }
            return View(boards.ToArray());
        }
    }
}
