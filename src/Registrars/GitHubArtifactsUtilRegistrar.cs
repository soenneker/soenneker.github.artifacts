using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.GitHub.Artifacts.Abstract;
using Soenneker.GitHub.Client.Registrars;

namespace Soenneker.GitHub.Artifacts.Registrars;

/// <summary>
/// A utility library for GitHub Artifacts operations
/// </summary>
public static class GitHubArtifactsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IGitHubArtifactsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static void AddGitHubArtifactsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddGitHubClientUtilAsSingleton();
        services.TryAddSingleton<IGitHubArtifactsUtil, GitHubArtifactsUtil>();
    }

    /// <summary>
    /// Adds <see cref="IGitHubArtifactsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static void AddGitHubArtifactsUtilAsScoped(this IServiceCollection services)
    {
        services.AddGitHubClientUtilAsSingleton();
        services.TryAddScoped<IGitHubArtifactsUtil, GitHubArtifactsUtil>();
    }
}