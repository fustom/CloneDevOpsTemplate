using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface IBoardService
{
    Task<Boards> GetBoardsAsync(Guid projectId, Guid teamId);
    Task<BoardColumns> GetBoardColumnsAsync(Guid projectId, Guid teamId, string boardId);
    Task UpdateBoardColumnsAsync(Guid projectId, Guid teamId, string boardId, BoardColumns boardColumns);
    Task MoveBoardColumnsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards);
}
