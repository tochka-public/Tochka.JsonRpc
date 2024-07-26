using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FluentAssertions;
using Json.Schema;
using Moq;
using Namotion.Reflection;
using NUnit.Framework;
using Tochka.JsonRpc.OpenRpc.Services;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.OpenRpc.Tests.Services;

[TestFixture]
public class OpenRpcContentDescriptorGeneratorTests
{
    private Mock<IOpenRpcSchemaGenerator> schemaGeneratorMock;
    private OpenRpcContentDescriptorGenerator contentDescriptorGenerator;

    [SetUp]
    public void Setup()
    {
        schemaGeneratorMock = new Mock<IOpenRpcSchemaGenerator>();

        contentDescriptorGenerator = new OpenRpcContentDescriptorGenerator(schemaGeneratorMock.Object);
    }

    [Test]
    public void GenerateForType_UseSerializedTypeNameAsName()
    {
        var type = typeof(TypeWithXmlDocs).ToContextualType();
        var propertyNamingPolicyMock = new Mock<JsonNamingPolicy>();
        var serializedName = "serializedName";
        propertyNamingPolicyMock.Setup(o => o.ConvertName(type.Name))
            .Returns(serializedName)
            .Verifiable();
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = propertyNamingPolicyMock.Object };

        var result = contentDescriptorGenerator.GenerateForType(type, MethodName, jsonSerializerOptions);

        result.Name.Should().Be(serializedName);
        propertyNamingPolicyMock.Verify();
    }

    [Test]
    public void GenerateForType_UseGeneratedJsonSchema()
    {
        var type = typeof(TypeWithXmlDocs).ToContextualType();
        var jsonSerializerOptions = new JsonSerializerOptions();
        var jsonSchema = JsonSchema.Empty;
        schemaGeneratorMock.Setup(g => g.CreateOrRef(type, MethodName, jsonSerializerOptions))
            .Returns(jsonSchema)
            .Verifiable();

        var result = contentDescriptorGenerator.GenerateForType(type, MethodName, jsonSerializerOptions);

        result.Schema.Should().Be(jsonSchema);
        schemaGeneratorMock.Verify();
    }

    [Test]
    public void GenerateForType_TypeHasXmlDocs_UseSummaryAndRemarks()
    {
        var type = typeof(TypeWithXmlDocs).ToContextualType();
        var jsonSerializerOptions = new JsonSerializerOptions();
        schemaGeneratorMock.Setup(g => g.CreateOrRef(type, MethodName, jsonSerializerOptions))
            .Returns(JsonSchema.Empty);

        var result = contentDescriptorGenerator.GenerateForType(type, MethodName, jsonSerializerOptions);

        result.Summary.Should().BeEquivalentTo("summary");
        result.Description.Should().BeEquivalentTo("description");
    }

    [Test]
    public void GenerateForType_MarkAsNotRequired()
    {
        var type = typeof(TypeWithXmlDocs).ToContextualType();
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = contentDescriptorGenerator.GenerateForType(type, MethodName, jsonSerializerOptions);

        result.Required.Should().BeFalse();
    }

    [Test]
    public void GenerateForType_TypeHasObsoleteAttribute_MarkAsDeprecated()
    {
#pragma warning disable CS0612
        var type = typeof(ObsoleteType).ToContextualType();
#pragma warning restore CS0612
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = contentDescriptorGenerator.GenerateForType(type, MethodName, jsonSerializerOptions);

        result.Deprecated.Should().BeTrue();
    }

    [Test]
    public void GenerateForParameter_UsePropertyNameAsName()
    {
        var property = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.WithDocs)).ToContextualProperty();
        var propertyName = "propertyName";
        var parameterMetadata = new JsonRpcParameterMetadata(propertyName, 0, BindingStyle.Default, true, "originalName", typeof(string));
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = contentDescriptorGenerator.GenerateForParameter(property, MethodName, parameterMetadata, jsonSerializerOptions);

        result.Name.Should().BeEquivalentTo(propertyName);
    }

    [Test]
    public void GenerateForParameter_UseGeneratedJsonSchema()
    {
        var property = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.WithDocs)).ToContextualProperty();
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, true, "originalName", typeof(string));
        var jsonSerializerOptions = new JsonSerializerOptions();
        var jsonSchema = JsonSchema.Empty;
        schemaGeneratorMock.Setup(g => g.CreateOrRef(property.PropertyType, MethodName, jsonSerializerOptions))
            .Returns(jsonSchema)
            .Verifiable();

        var result = contentDescriptorGenerator.GenerateForParameter(property, MethodName, parameterMetadata, jsonSerializerOptions);

        result.Schema.Should().Be(jsonSchema);
        schemaGeneratorMock.Verify();
    }

    [Test]
    public void GenerateForParameter_PropertyHasXmlDocs_UseSummaryAndRemarks()
    {
        var property = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.WithDocs)).ToContextualProperty();
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, true, "originalName", typeof(string));
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = contentDescriptorGenerator.GenerateForParameter(property, MethodName, parameterMetadata, jsonSerializerOptions);

        result.Summary.Should().BeEquivalentTo("summary");
        result.Description.Should().BeEquivalentTo("description");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GenerateForParameter_MarkRequiredBasedOnMetadataOptional(bool optional)
    {
        var property = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.WithDocs)).ToContextualProperty();
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, optional, "originalName", typeof(string));
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = contentDescriptorGenerator.GenerateForParameter(property, MethodName, parameterMetadata, jsonSerializerOptions);

        result.Required.Should().Be(!optional);
    }

    [Test]
    public void GenerateForParameter_PropertyHasObsoleteAttribute_MarkAsDeprecated()
    {
#pragma warning disable CS0612
        var property = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.Obsolete)).ToContextualProperty();
#pragma warning restore CS0612
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, true, "originalName", typeof(string));
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = contentDescriptorGenerator.GenerateForParameter(property, MethodName, parameterMetadata, jsonSerializerOptions);

        result.Deprecated.Should().BeTrue();
    }

    [Test]
    public void GenerateForProperty_UseSerializedPropertyNameAsName()
    {
        var property = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.WithDocs)).ToContextualProperty();
        var propertyNamingPolicyMock = new Mock<JsonNamingPolicy>();
        var serializedName = "serializedName";
        propertyNamingPolicyMock.Setup(o => o.ConvertName(property.Name))
            .Returns(serializedName)
            .Verifiable();
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = propertyNamingPolicyMock.Object };

        var result = contentDescriptorGenerator.GenerateForProperty(property, MethodName, jsonSerializerOptions);

        result.Name.Should().Be(serializedName);
        propertyNamingPolicyMock.Verify();
    }

    [Test]
    public void GenerateForProperty_UseGeneratedJsonSchema()
    {
        var property = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.WithDocs)).ToContextualProperty();
        var jsonSerializerOptions = new JsonSerializerOptions();
        var jsonSchema = JsonSchema.Empty;
        schemaGeneratorMock.Setup(g => g.CreateOrRef(property.PropertyType, MethodName, jsonSerializerOptions))
            .Returns(jsonSchema)
            .Verifiable();

        var result = contentDescriptorGenerator.GenerateForProperty(property, MethodName, jsonSerializerOptions);

        result.Schema.Should().Be(jsonSchema);
        schemaGeneratorMock.Verify();
    }

    [Test]
    public void GenerateForProperty_PropertyHasXmlDocs_UseSummaryAndRemarks()
    {
        var property = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.WithDocs)).ToContextualProperty();
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = contentDescriptorGenerator.GenerateForProperty(property, MethodName, jsonSerializerOptions);

        result.Summary.Should().BeEquivalentTo("summary");
        result.Description.Should().BeEquivalentTo("description");
    }

    [Test]
    public void GenerateForProperty_PropertyHasRequiredAttribute_MarkAsRequired()
    {
        var property = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.Required)).ToContextualProperty();
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = contentDescriptorGenerator.GenerateForProperty(property, MethodName, jsonSerializerOptions);

        result.Required.Should().BeTrue();
    }

    [Test]
    public void GenerateForProperty_PropertyHasObsoleteAttribute_MarkAsDeprecated()
    {
#pragma warning disable CS0612
        var property = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.Obsolete)).ToContextualProperty();
#pragma warning restore CS0612
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = contentDescriptorGenerator.GenerateForProperty(property, MethodName, jsonSerializerOptions);

        result.Deprecated.Should().BeTrue();
    }

    private const string MethodName = "method";

    /// <summary>summary</summary>
    /// <remarks>description</remarks>
    private sealed record TypeWithXmlDocs;

    [Obsolete]
    private sealed record ObsoleteType;

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private sealed record TypeWithProperties
    {
        /// <summary>summary</summary>
        /// <remarks>description</remarks>
        public string WithDocs { get; init; }

        [Required]
        public string Required { get; init; }

        [Obsolete]
        public string Obsolete { get; init; }
    }
}
