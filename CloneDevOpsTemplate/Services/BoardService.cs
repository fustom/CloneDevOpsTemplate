using System.Net;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class BoardService(IHttpClientFactory httpClientFactory) : IBoardService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public async Task<Boards> GetBoardsAsync(Guid projectId, string teamId)
    {
        return await _client.GetFromJsonAsync<Boards>($"{projectId}/{teamId}/_apis/work/boards?api-version=7.1") ?? new();
    }

    public async Task<BoardColumns> GetBoardColumnsAsync(Guid projectId, string teamId, string boardId)
    {
        return await _client.GetFromJsonAsync<BoardColumns>($"{projectId}/{teamId}/_apis/work/boards/{boardId}/columns?api-version=7.1") ?? new();
    }
}
