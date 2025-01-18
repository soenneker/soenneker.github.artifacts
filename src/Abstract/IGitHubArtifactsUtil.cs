using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.GitHub.Artifacts.Abstract;

/// <summary>
/// A utility library for GitHub Artifacts operations
/// </summary>
public interface IGitHubArtifactsUtil
{
    ValueTask<List<Artifact>> GetAllForOwner(string owner, CancellationToken cancellationToken = default);

    ValueTask<List<Artifact>> GetAllOlderThan(string owner, string repo, int olderThanDays = 3, CancellationToken cancellationToken = default);

    ValueTask DeleteOldArtifacts(string owner, string repo, int keepWithinDays = 3, CancellationToken cancellationToken = default);

    ValueTask DeleteArtifacts(string owner, string repositoryName, List<Artifact> artifacts, CancellationToken cancellationToken = default);
}