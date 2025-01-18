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
using Soenneker.GitHub.Repositories.Abstract;
using Soenneker.GitHub.Repositories;

namespace Soenneker.GitHub.Artifacts;

/// <inheritdoc cref="IGitHubArtifactsUtil"/>
public class GitHubArtifactsUtil : IGitHubArtifactsUtil
{
    private readonly IGitHubClientUtil _gitHubClientUtil;
    private readonly IGitHubRepositoriesUtil _gitHubRepositoriesUtil;
    private readonly ILogger<GitHubArtifactsUtil> _logger;

    // GitHub restricted
    private const int _maximumPerPage = 100;

    public GitHubArtifactsUtil(ILogger<GitHubArtifactsUtil> logger, IGitHubClientUtil gitHubClientUtil, IGitHubRepositoriesUtil gitHubRepositoriesUtil)
    {
        _gitHubClientUtil = gitHubClientUtil;
        _gitHubRepositoriesUtil = gitHubRepositoriesUtil;
        _logger = logger;
    }

    public async ValueTask<List<Artifact>> GetAllForOwner(string owner, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all artifacts for owner ({owner})...", owner);

        IReadOnlyList<Repository> allRepos = await _gitHubRepositoriesUtil.GetAllForOwner(owner, cancellationToken).NoSync();

        var result = new List<Artifact>();

        foreach (Repository repo in allRepos)
        {
            List<Artifact> artifacts = await GetAllForRepo(owner, repo.Name, cancellationToken).NoSync();
            result.AddRange(artifacts);
        }

        return result;
    }

    public async ValueTask<List<Artifact>> GetAllForRepo(string owner, string repo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all artifacts for repo ({owner}/{repo})...", owner, repo);

        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        var result = new List<Artifact>();
        var page = 1;

        while (true)
        {
            ListArtifactsResponse? artifactsResponse = await client.Actions.Artifacts.ListArtifacts(owner, repo, new ListArtifactsRequest {Page = page, PerPage = _maximumPerPage}).NoSync();

            if (artifactsResponse.TotalCount == 0)
                break;

            result.AddRange(artifactsResponse.Artifacts);

            if (artifactsResponse.Artifacts.Count < _maximumPerPage)
                break;

            page++;
        }

        return result;
    }

    public async ValueTask<List<Artifact>> GetAllOlderThan(string owner, string repo, int olderThanDays = 3, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all artifacts older than {days} days...", olderThanDays);

        List<Artifact> allArtifacts = await GetAllForRepo(owner, repo, cancellationToken);

        List<Artifact> results = [];

        foreach (Artifact? artifact in allArtifacts)
        {
            int ageDays = artifact.CreatedAt.ToAge(UnitOfTime.Day).ToInt();

            if (ageDays > olderThanDays)
            {
                results.Add(artifact);
            }
        }

        return results;
    }

    public async ValueTask DeleteOldArtifacts(string owner, string repo, int keepWithinDays = 3, CancellationToken cancellationToken = default)
    {
        List<Artifact> artifacts = await GetAllOlderThan(owner, repo, keepWithinDays, cancellationToken).NoSync();

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