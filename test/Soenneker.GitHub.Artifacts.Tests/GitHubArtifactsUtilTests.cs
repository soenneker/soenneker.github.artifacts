using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Soenneker.GitHub.Artifacts.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.GitHub.Artifacts.Tests;

[Collection("Collection")]
public class GitHubArtifactsUtilTests : FixturedUnitTest
{
    private readonly IGitHubArtifactsUtil _util;

    public GitHubArtifactsUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IGitHubArtifactsUtil>(true);
    }
}
