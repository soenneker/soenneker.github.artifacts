using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.GitHub.Artifacts.Abstract;
using Soenneker.GitHub.Repositories.Registrars;

namespace Soenneker.GitHub.Artifacts.Registrars;

/// <summary>
/// A utility library for GitHub Artifacts operations
/// </summary>
public static class GitHubArtifactsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IGitHubArtifactsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddGitHubArtifactsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddGitHubRepositoriesUtilAsSingleton().TryAddSingleton<IGitHubArtifactsUtil, GitHubArtifactsUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IGitHubArtifactsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddGitHubArtifactsUtilAsScoped(this IServiceCollection services)
    {
        services.AddGitHubRepositoriesUtilAsScoped().TryAddScoped<IGitHubArtifactsUtil, GitHubArtifactsUtil>();

        return services;
    }
}