using Xunit;

namespace LineItem.Test.Integration.Setup;

/// <summary>
///     Registers a collection of tests that share the ApiFixture.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<ApiFixture>
{
}