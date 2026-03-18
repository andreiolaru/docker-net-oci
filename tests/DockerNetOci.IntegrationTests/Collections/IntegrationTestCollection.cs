using DockerNetOci.IntegrationTests.Fixtures;

namespace DockerNetOci.IntegrationTests.Collections;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<OracleContainerFixture>
{
    public const string Name = "Oracle Integration Tests";
}
