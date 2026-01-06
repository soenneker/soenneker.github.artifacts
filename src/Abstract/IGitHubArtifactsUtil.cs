using Soenneker.GitHub.OpenApiClient.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.GitHub.Artifacts.Abstract;

/// <summary>
/// Provides utilities for retrieving and managing GitHub Actions artifacts at the repository and organization level.
/// </summary>
public interface IGitHubArtifactsUtil
{
    /// <summary>
    /// Retrieves all GitHub Actions artifacts for all repositories under a specified owner.
    /// </summary>
    /// <param name="owner">The GitHub username or organization name.</param>
    /// <param name="startAt">Optional filter to only include repositories created after this date.</param>
    /// <param name="endAt">Optional filter to only include repositories created before this date.</param>
    /// <param name="cancellationToken">A cancellation token for the async operation.</param>
    /// <returns>A list of artifacts across all repositories owned by the given user or organization.</returns>
    ValueTask<List<Artifact>> GetAllForOwner(string owner, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all GitHub Actions artifacts for a specific repository.
    /// </summary>
    /// <param name="owner">The GitHub username or organization name that owns the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <param name="cancellationToken">A cancellation token for the async operation.</param>
    /// <returns>A list of artifacts in the specified repository.</returns>
    ValueTask<List<Artifact>> GetAllForRepo(string owner, string repo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all GitHub Actions artifacts for a repository that are older than a specified number of days.
    /// </summary>
    /// <param name="owner">The GitHub username or organization name.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <param name="olderThanDays">The minimum age in days for an artifact to be included.</param>
    /// <param name="cancellationToken">A cancellation token for the async operation.</param>
    /// <returns>A list of artifacts older than the specified number of days.</returns>
    ValueTask<List<Artifact>> GetAllOlderThan(string owner, string repo, int olderThanDays = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all GitHub Actions artifacts in a repository that are older than a specified number of days.
    /// </summary>
    /// <param name="owner">The GitHub username or organization name.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <param name="keepWithinDays">The number of days within which artifacts should be kept.</param>
    /// <param name="cancellationToken">A cancellation token for the async operation.</param>
    ValueTask DeleteOldArtifacts(string owner, string repo, int keepWithinDays = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a list of GitHub Actions artifacts from a repository.
    /// </summary>
    /// <param name="owner">The GitHub username or organization name.</param>
    /// <param name="repositoryName">The name of the repository.</param>
    /// <param name="artifacts">The list of artifacts to delete.</param>
    /// <param name="cancellationToken">A cancellation token for the async operation.</param>
    ValueTask DeleteArtifacts(string owner, string repositoryName, List<Artifact> artifacts, CancellationToken cancellationToken = default);
}