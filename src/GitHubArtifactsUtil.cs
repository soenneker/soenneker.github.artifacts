using Soenneker.GitHub.Artifacts.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Octokit;
using Soenneker.Enums.UnitOfTime;
using Soenneker.Extensions.DateTime;
using Soenneker.Extensions.Double;
using Soenneker.GitHub.Client.Abstract;
using Soenneker.Extensions.ValueTask;
using Soenneker.Extensions.Task;

namespace Soenneker.GitHub.Artifacts;

/// <inheritdoc cref="IGitHubArtifactsUtil"/>
public class GitHubArtifactsUtil : IGitHubArtifactsUtil
{
    private readonly IGitHubClientUtil _gitHubClientUtil;
    private readonly ILogger<GitHubArtifactsUtil> _logger;

    // GitHub restricted
    private const int _maximumPerPage = 100;

    public GitHubArtifactsUtil(ILogger<GitHubArtifactsUtil> logger, IGitHubClientUtil gitHubClientUtil)
    {
        _gitHubClientUtil = gitHubClientUtil;
        _logger = logger;
    }

    public async ValueTask<List<Artifact>> GetAllArtifactsOlderThan(string owner, string repositoryName, int olderThanDays = 3, CancellationToken cancellationToken = default)
    {
        var result = new List<Artifact>();
        var page = 1;

        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        while (true)
        {
            ListArtifactsResponse? artifactsResponse = await client.Actions.Artifacts.ListArtifacts(owner, repositoryName, new ListArtifactsRequest {Page = page, PerPage = _maximumPerPage}).NoSync();

            if (artifactsResponse.TotalCount == 0)
                break;

            foreach (Artifact? artifact in artifactsResponse.Artifacts)
            {
                int ageDays = artifact.CreatedAt.ToAge(UnitOfTime.Day).ToInt();

                if (ageDays > olderThanDays)
                {
                    result.Add(artifact);
                }
            }

            if (artifactsResponse.Artifacts.Count < _maximumPerPage)
                break;

            page++;
        }

        return result;
    }

    public async ValueTask DeleteOldArtifacts(string owner, string repo, int keepWithinDays = 3, CancellationToken cancellationToken = default)
    {
        List<Artifact> artifacts = await GetAllArtifactsOlderThan(owner, repo, keepWithinDays, cancellationToken).NoSync();

        await DeleteArtifacts(owner, repo, artifacts, cancellationToken).NoSync();
    }

    public async ValueTask DeleteArtifacts(string owner, string repositoryName, List<Artifact> artifacts, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Deleting {count} artifacts...", artifacts.Count);

        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        foreach (Artifact artifact in artifacts)
        {
            _logger.LogInformation("Deleting artifact {artifactName} ({artifactId}) that's {age} days old...", artifact.Name, artifact.Id, artifact.CreatedAt.ToAge(UnitOfTime.Day).ToInt());

            await client.Actions.Artifacts.DeleteArtifact(owner, repositoryName, artifact.Id).NoSync();

            await Task.Delay(500, cancellationToken).NoSync();
        }
    }
}