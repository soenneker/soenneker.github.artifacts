using Soenneker.GitHub.Artifacts.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.GitHub.Artifacts.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class GitHubArtifactsUtilTests : HostedUnitTest
{
    private readonly IGitHubArtifactsUtil _util;

    public GitHubArtifactsUtilTests(Host host) : base(host)
    {
        _util = Resolve<IGitHubArtifactsUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
