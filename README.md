[![](https://img.shields.io/nuget/v/soenneker.github.artifacts.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.github.artifacts/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.github.artifacts/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.github.artifacts/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.github.artifacts.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.github.artifacts/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.github.artifacts/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.github.artifacts/actions/workflows/codeql.yml)

# Soenneker.GitHub.Artifacts

Lists GitHub Actions artifacts and removes selected or expired artifacts from repositories.

## Installation

```bash
dotnet add package Soenneker.GitHub.Artifacts
```

## Configure and register

```json
{
  "GH": {
    "Token": "your-github-token"
  }
}
```

```csharp
using Soenneker.GitHub.Artifacts.Registrars;

services.AddGitHubArtifactsUtilAsSingleton();
```

The token needs read access to Actions artifacts for listing and Actions write access for deletion.

## List artifacts

```csharp
using Soenneker.GitHub.Artifacts.Abstract;

public sealed class ArtifactReader(IGitHubArtifactsUtil artifacts)
{
    public ValueTask<List<Artifact>> Get(
        string owner,
        string repository,
        CancellationToken cancellationToken) =>
        artifacts.GetAllForRepo(owner, repository, cancellationToken);
}
```

`GetAllForRepo()` follows GitHub's paged artifact response until the final page. `GetAllForOwner()` first enumerates the owner's repositories and then retrieves every repository's artifacts sequentially; use it deliberately for owners with many repositories.

## Delete old artifacts

```csharp
await artifacts.DeleteOldArtifacts(
    owner: "example-org",
    repo: "example-repository",
    keepWithinDays: 14,
    cancellationToken);
```

Deletion is permanent. `keepWithinDays` retains artifacts whose integer age is at most that value and deletes artifacts strictly older than it. Artifacts without a creation timestamp are not selected by `DeleteOldArtifacts()`. `DeleteArtifacts()` deletes exactly the supplied artifacts that have an ID and pauses briefly between requests.
