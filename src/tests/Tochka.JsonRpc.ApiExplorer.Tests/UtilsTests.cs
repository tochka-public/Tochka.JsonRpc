using FluentAssertions;
using NUnit.Framework;

namespace Tochka.JsonRpc.ApiExplorer.Tests;

[TestFixture]
internal class UtilsTests
{
    [Test]
    public void GetDocumentName_TypeIsNull_ReturnDefault()
    {
        var result = Utils.GetDocumentName(null);

        result.Should().Be(ApiExplorerConstants.DefaultDocumentName);
    }

    [Test]
    public void GetDocumentName_TypeNameEndsWithInterfaceName_AddLowercaseTypeNameStart()
    {
        var result = Utils.GetDocumentName(typeof(SnakeCaseJsonSerializerOptionsProvider));

        result.Should().Be($"{ApiExplorerConstants.DefaultDocumentName}_snakecase");
    }

    [Test]
    public void GetDocumentName_TypeNameStartsWithInterfaceName_AddLowercaseTypeNameEnd()
    {
        var result = Utils.GetDocumentName(typeof(JsonSerializerOptionsProviderCamelCase));

        result.Should().Be($"{ApiExplorerConstants.DefaultDocumentName}_camelcase");
    }

    [Test]
    public void GetDocumentName_TypeNameDoesntContainInterfaceName_AddLowercaseTypeName()
    {
        var result = Utils.GetDocumentName(typeof(PascalCaseProvider));

        result.Should().Be($"{ApiExplorerConstants.DefaultDocumentName}_pascalcaseprovider");
    }

    private record SnakeCaseJsonSerializerOptionsProvider;

    private record JsonSerializerOptionsProviderCamelCase;

    private record PascalCaseProvider;
}
