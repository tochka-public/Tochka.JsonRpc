using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Moq;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Swagger.Tests;

[TestFixture]
public class JsonRpcSchemaGeneratorTests
{
    private SchemaGeneratorOptions schemaGeneratorOptions;
    private List<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private JsonRpcServerOptions jsonRpcServerOptions;
    private Mock<JsonRpcSchemaGenerator> schemaGeneratorMock;

    [SetUp]
    public void Setup()
    {
        schemaGeneratorOptions = new SchemaGeneratorOptions();
        serializerOptionsProviders = new List<IJsonSerializerOptionsProvider>();
        jsonRpcServerOptions = new JsonRpcServerOptions();

        schemaGeneratorMock = new Mock<JsonRpcSchemaGenerator>(schemaGeneratorOptions, serializerOptionsProviders, Options.Create(jsonRpcServerOptions))
        {
            CallBase = true
        };
    }

    [Test]
    public void GenerateSchema_NoMetadata_UseDefaultDataJsonSerializerOptions()
    {
        var modelType = Mock.Of<Type>();
        var schemaRepository = new SchemaRepository();
        var serializerOptions = jsonRpcServerOptions.DefaultDataJsonSerializerOptions;
        var generatedSchema = new OpenApiSchema();
        schemaGeneratorMock.Setup(g => g.UseDefaultGenerator(modelType, schemaRepository, null, null, null, serializerOptions))
            .Returns(generatedSchema)
            .Verifiable();

        var result = schemaGeneratorMock.Object.GenerateSchema(modelType, schemaRepository);

        result.Should().Be(generatedSchema);
        schemaGeneratorMock.Verify();
    }

    [Test]
    public void GenerateSchema_HasMetadataButProviderNotRegistered_Throw()
    {
        var modelType = typeof(TypeWithMetadata);
        var schemaRepository = new SchemaRepository();

        var action = () => schemaGeneratorMock.Object.GenerateSchema(modelType, schemaRepository);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GenerateSchema_HasMetadataAndProviderRegistered_UseJsonSerializerOptionsProviderFromMetadata()
    {
        var modelType = typeof(TypeWithMetadata);
        var jsonSerializerOptionsProvider = new SnakeCaseJsonSerializerOptionsProvider();
        serializerOptionsProviders.Add(jsonSerializerOptionsProvider);
        var schemaRepository = new SchemaRepository();
        var generatedSchema = new OpenApiSchema();
        schemaGeneratorMock.Setup(g => g.UseDefaultGenerator(modelType, schemaRepository, null, null, null, jsonSerializerOptionsProvider.Options))
            .Returns(generatedSchema)
            .Verifiable();

        var result = schemaGeneratorMock.Object.GenerateSchema(modelType, schemaRepository);

        result.Should().Be(generatedSchema);
        schemaGeneratorMock.Verify();
    }

    private const string MethodName = "method";

    [JsonRpcTypeMetadata(typeof(SnakeCaseJsonSerializerOptionsProvider), MethodName)]
    private sealed record TypeWithMetadata;
}
