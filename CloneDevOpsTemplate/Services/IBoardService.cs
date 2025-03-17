using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface IBoardService
{
    Task<Boards> GetBoardsAsync(Guid projectId, string teamId);
    Task<BoardColumns> GetBoardColumnsAsync(Guid projectId, string teamId, string boardId);
}
