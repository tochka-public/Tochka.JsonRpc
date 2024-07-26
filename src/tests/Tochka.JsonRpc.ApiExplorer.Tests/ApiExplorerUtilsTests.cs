using FluentAssertions;
using NUnit.Framework;

namespace Tochka.JsonRpc.ApiExplorer.Tests;

[TestFixture]
public class ApiExplorerUtilsTests
{
    [Test]
    public void GetDocumentName_TypeIsNull_ReturnDefault()
    {
        var defaultName = "default";

        var result = ApiExplorerUtils.GetDocumentName(defaultName, null);

        result.Should().Be(defaultName);
    }

    [Test]
    public void GetDocumentName_TypeNameEndsWithInterfaceName_AddLowercaseTypeNameStart()
    {
        var defaultName = "default";

        var result = ApiExplorerUtils.GetDocumentName(defaultName, typeof(SnakeCaseJsonSerializerOptionsProvider));

        result.Should().Be($"{defaultName}_snakecase");
    }

    [Test]
    public void GetDocumentName_TypeNameStartsWithInterfaceName_AddLowercaseTypeNameEnd()
    {
        var defaultName = "default";

        var result = ApiExplorerUtils.GetDocumentName(defaultName, typeof(JsonSerializerOptionsProviderCamelCase));

        result.Should().Be($"{defaultName}_camelcase");
    }

    [Test]
    public void GetDocumentName_TypeNameDoesntContainInterfaceName_AddLowercaseTypeName()
    {
        var defaultName = "default";

        var result = ApiExplorerUtils.GetDocumentName(defaultName, typeof(PascalCaseProvider));

        result.Should().Be($"{defaultName}_pascalcaseprovider");
    }

    private sealed record SnakeCaseJsonSerializerOptionsProvider;

    private sealed record JsonSerializerOptionsProviderCamelCase;

    private sealed record PascalCaseProvider;
}
