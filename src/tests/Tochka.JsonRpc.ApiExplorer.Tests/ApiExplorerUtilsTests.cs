using FluentAssertions;
using NUnit.Framework;

namespace Tochka.JsonRpc.ApiExplorer.Tests;

[TestFixture]
internal class ApiExplorerUtilsTests
{
    [Test]
    public void GetDocumentName_TypeIsNull_ReturnDefault()
    {
        var result = ApiExplorerUtils.GetDocumentName(null);

        result.Should().Be(ApiExplorerConstants.DefaultDocumentName);
    }

    [Test]
    public void GetDocumentName_TypeNameEndsWithInterfaceName_AddLowercaseTypeNameStart()
    {
        var result = ApiExplorerUtils.GetDocumentName(typeof(SnakeCaseJsonSerializerOptionsProvider));

        result.Should().Be($"{ApiExplorerConstants.DefaultDocumentName}_snakecase");
    }

    [Test]
    public void GetDocumentName_TypeNameStartsWithInterfaceName_AddLowercaseTypeNameEnd()
    {
        var result = ApiExplorerUtils.GetDocumentName(typeof(JsonSerializerOptionsProviderCamelCase));

        result.Should().Be($"{ApiExplorerConstants.DefaultDocumentName}_camelcase");
    }

    [Test]
    public void GetDocumentName_TypeNameDoesntContainInterfaceName_AddLowercaseTypeName()
    {
        var result = ApiExplorerUtils.GetDocumentName(typeof(PascalCaseProvider));

        result.Should().Be($"{ApiExplorerConstants.DefaultDocumentName}_pascalcaseprovider");
    }

    private record SnakeCaseJsonSerializerOptionsProvider;

    private record JsonSerializerOptionsProviderCamelCase;

    private record PascalCaseProvider;
}
