using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.GitHub.Artifacts.Abstract;
using Soenneker.GitHub.ClientUtil.Abstract;
using Soenneker.GitHub.OpenApiClient;
using Soenneker.GitHub.OpenApiClient.Models;
using Soenneker.GitHub.OpenApiClient.Repos.Item.Item.Actions.Artifacts;
using Soenneker.GitHub.Repositories.Abstract;
using Soenneker.Utils.Delay;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.GitHub.Artifacts;

///<inheritdoc cref="IGitHubArtifactsUtil"/>
public sealed class GitHubArtifactsUtil : IGitHubArtifactsUtil
{
    private readonly ILogger<GitHubArtifactsUtil> _logger;
    private readonly IGitHubOpenApiClientUtil _gitHubClientUtil;
    private const int _maximumPerPage = 100;
    private readonly IGitHubRepositoriesUtil _repositoriesUtil;

    public GitHubArtifactsUtil(ILogger<GitHubArtifactsUtil> logger, IGitHubOpenApiClientUtil gitHubClientUtil, IGitHubRepositoriesUtil repositoriesUtil)
    {
        _logger = logger;
        _gitHubClientUtil = gitHubClientUtil;
        _repositoriesUtil = repositoriesUtil;
    }

    public async ValueTask<List<Artifact>> GetAllForOwner(string owner, DateTime? startAt = null, DateTime? endAt = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all artifacts for owner ({owner})...", owner);

        IReadOnlyList<MinimalRepository> allRepos = await _repositoriesUtil.GetAllForOwner(owner, startAt, endAt, cancellationToken).NoSync();

        var result = new List<Artifact>();

        for (var i = 0; i < allRepos.Count; i++)
        {
            MinimalRepository repo = allRepos[i];
            List<Artifact> artifacts = await GetAllForRepo(owner, repo.Name, cancellationToken).NoSync();
            result.AddRange(artifacts);
        }

        return result;
    }

    public async ValueTask<List<Artifact>> GetAllForRepo(string owner, string repo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all artifacts for repo ({owner}/{repo})...", owner, repo);

        GitHubOpenApiClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        var result = new List<Artifact>();
        var page = 1;

        while (true)
        {
            ArtifactsGetResponse? artifactsResponse = await client.Repos[owner][repo]
                                                                  .Actions.Artifacts.GetAsync(requestConfiguration =>
                                                                  {
                                                                      requestConfiguration.QueryParameters.Page = page;
                                                                      requestConfiguration.QueryParameters.PerPage = _maximumPerPage;
                                                                  }, cancellationToken).NoSync();

            if (artifactsResponse?.TotalCount == 0)
                break;

            _logger.LogDebug("{count} artifacts found", artifactsResponse?.TotalCount);

            if (artifactsResponse?.Artifacts != null)
            {
                result.AddRange(artifactsResponse.Artifacts);
            }

            if (artifactsResponse?.Artifacts?.Count < _maximumPerPage)
                break;

            page++;
        }

        return result;
    }

    public async ValueTask<List<Artifact>> GetAllOlderThan(string owner, string repo, int olderThanDays = 3, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all artifacts older than {days} days...", olderThanDays);

        List<Artifact> allArtifacts = await GetAllForRepo(owner, repo, cancellationToken).NoSync();

        var results = new List<Artifact>();

        for (var i = 0; i < allArtifacts.Count; i++)
        {
            Artifact? artifact = allArtifacts[i];

            if (artifact?.CreatedAt == null)
                continue;

            var ageDays = (int) (DateTime.UtcNow - artifact.CreatedAt.Value.DateTime).TotalDays;

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

        GitHubOpenApiClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        for (var i = 0; i < artifacts.Count; i++)
        {
            Artifact artifact = artifacts[i];
            if (artifact.Id == null)
                continue;

            var ageDays = (int) (DateTime.UtcNow - artifact.CreatedAt!.Value.DateTime).TotalDays;

            _logger.LogInformation("Deleting artifact {artifactName} ({artifactId}) that's {age} days old...", artifact.Name, artifact.Id, ageDays);

            await client.Repos[owner][repositoryName].Actions.Artifacts[artifact.Id.Value].DeleteAsync(cancellationToken: cancellationToken).NoSync();

            await DelayUtil.Delay(500, _logger, cancellationToken).NoSync();
        }
    }
}