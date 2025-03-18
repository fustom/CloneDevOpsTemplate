using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class BoardService(IHttpClientFactory httpClientFactory) : IBoardService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public async Task<Boards> GetBoardsAsync(Guid projectId, Guid teamId)
    {
        return await _client.GetFromJsonAsync<Boards>($"{projectId}/{teamId}/_apis/work/boards?api-version=7.1") ?? new();
    }

    public async Task<BoardColumns> GetBoardColumnsAsync(Guid projectId, Guid teamId, string boardId)
    {
        return await _client.GetFromJsonAsync<BoardColumns>($"{projectId}/{teamId}/_apis/work/boards/{boardId}/columns?api-version=7.1") ?? new();
    }

    public async Task UpdateBoardColumnsAsync(Guid projectId, Guid teamId, string boardId, BoardColumns boardColumns)
    {
        await _client.PutAsJsonAsync($"{projectId}/{teamId}/_apis/work/boards/{boardId}/columns?api-version=7.1", boardColumns.Value, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    public async Task MoveBoardColumnsAsync(Guid projectId, Guid projectTeamId, Guid templateProjectId, Guid templateTeamId, Boards projectBoards)
    {
        Boards templateBoards = await GetBoardsAsync(templateProjectId, templateTeamId) ?? new();
        foreach (Board templateBoard in templateBoards.Value)
        {
            BoardColumns templateBoardColumns = await GetBoardColumnsAsync(templateProjectId, templateTeamId, templateBoard.Id) ?? new();
            var matchingProjectBoard = projectBoards.Value.SingleOrDefault(b => b.Name == templateBoard.Name);
            if (matchingProjectBoard != null)
            {
                BoardColumns currentBoardColumns = await GetBoardColumnsAsync(projectId, projectTeamId, matchingProjectBoard.Id) ?? new();
                var incomingColumnId = currentBoardColumns.Value.FirstOrDefault(c => c.ColumnType == BoardColumnType.incoming.ToString())?.Id;
                var outgoingColumnId = currentBoardColumns.Value.FirstOrDefault(c => c.ColumnType == BoardColumnType.outgoing.ToString())?.Id;
                foreach (var templateBoardColumn in templateBoardColumns.Value)
                {
                    if (templateBoardColumn.ColumnType == BoardColumnType.incoming.ToString() && incomingColumnId is not null)
                    {
                        templateBoardColumn.Id = incomingColumnId;
                    }
                    else if (templateBoardColumn.ColumnType == BoardColumnType.outgoing.ToString() && outgoingColumnId is not null)
                    {
                        templateBoardColumn.Id = outgoingColumnId;
                    }
                    else
                    {
                        templateBoardColumn.Id = string.Empty;
                    }
                }

                await UpdateBoardColumnsAsync(projectId, projectTeamId, matchingProjectBoard.Id, templateBoardColumns);
            }
        }
    }
}
