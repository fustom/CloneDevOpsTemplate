using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Managers;

public interface ICloneManager
{
    Task<CloneResult> CloneClassificationNodes(Guid templateProjectId, Guid projectId, TreeStructureGroup structureGroup, IProgress<string>? progress = null);
    Task<CloneResult> CloneBoardsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, IProgress<string>? progress = null);
    Task<CloneProjectResult> CloneProjectAsync(Guid templateProjectId, string newProjectName, string description, Visibility visibility, IProgress<string>? progress = null);
    Task<CloneResult> CloneRepositoriesAsync(Guid templateProjectId, Guid projectId, IProgress<string>? progress = null);
    Task<CloneResult> CloneTeamFieldValuesAsync(Project templateProject, Project project, Guid templateTeamId, Guid projectTeamId, IProgress<string>? progress = null);
    Task<CloneResult> CloneTeamFieldValuesAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, IProgress<string>? progress = null);
    Task<CloneTeamsResult> CloneTeamsAsync(Project templateProject, Project project, IProgress<string>? progress = null);
    Task<CloneTeamsResult> CloneTeamsAsync(Guid templateProjectId, Guid projectId, IProgress<string>? progress = null);
    Task<CloneResult> CloneTeamsAndSettingsAndBoardsAsync(Project templateProject, Project project, IProgress<string>? progress = null);
    Task<CloneResult> CloneTeamSettingsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, IProgress<string>? progress = null);
    Task<CloneResult> CloneTeamIterationsAsync(Guid templateProjectId, Guid projectId, Guid templateTeamId, Guid projectTeamId, IProgress<string>? progress = null);
    Task<CloneResult> CloneGitPullRequestsAsync(Guid templateProjectId, Guid projectId, IProgress<string>? progress = null);
}
