using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FluentAssertions;
using Json.Schema;
using Json.Schema.Generation;
using NUnit.Framework;
using Tochka.JsonRpc.OpenRpc.Services;
using Yoh.Text.Json.NamingPolicies;

namespace Tochka.JsonRpc.OpenRpc.Tests.Services;

[TestFixture]
internal class OpenRpcSchemaGeneratorTests
{
    private OpenRpcSchemaGenerator schemaGenerator;

    [SetUp]
    public void Setup() => schemaGenerator = new OpenRpcSchemaGenerator();

    [Test]
    public void CreateOrRef_CollectionWithSimpleItems_ReturnSchemaItself()
    {
        var type = typeof(IEnumerable<string>);
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Array)
            .Items(new JsonSchemaBuilder()
                .FromType<string>()
                .Build())
            .Build();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>();
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_CollectionWithCustomSimpleItems_ReturnSchemaItself()
    {
        var type = typeof(IEnumerable<CustomSimpleType>);
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Array)
            .Items(new JsonSchemaBuilder()
                .FromType<CustomSimpleType>()
                .Build())
            .Build();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>();
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_CollectionWithEnumItems_ReturnSchemaWithRefForItems()
    {
        var type = typeof(IEnumerable<Enum>);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(Enum)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Array)
            .Items(new JsonSchemaBuilder()
                .Ref($"#/components/schemas/{expectedTypeName}")
                .Build())
            .Build();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Enum("one", "two")
                .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_CollectionWithItemsWithProperties_ReturnSchemaWithRefForItems()
    {
        var type = typeof(IEnumerable<TypeWithProperties>);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithProperties)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Array)
            .Items(new JsonSchemaBuilder()
                .Ref($"#/components/schemas/{expectedTypeName}")
                .Build())
            .Build();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedAnotherTypeName = $"{MethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["int_property"] = new JsonSchemaBuilder().FromType<int>().Build(),
                    ["string_property"] = new JsonSchemaBuilder().FromType<string>().Build(),
                    ["nested_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeName}").Build(),
                    ["another_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedAnotherTypeName}").Build()
                })
                .Build(),
            [expectedAnotherTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().Build()
                })
                .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_Enum_ReturnRef()
    {
        var type = typeof(Enum);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(Enum)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .Build();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Enum("one", "two")
                .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_SimpleType_ReturnSchemaItself()
    {
        var type = typeof(string);
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedSchema = new JsonSchemaBuilder()
            .FromType<string>()
            .Build();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>();
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_CustomSimpleType_ReturnSchemaItself()
    {
        var type = typeof(CustomSimpleType);
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedSchema = new JsonSchemaBuilder()
            .FromType<CustomSimpleType>()
            .Build();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>();
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_TypeWithProperties_ReturnRef()
    {
        var type = typeof(TypeWithProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithProperties)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .Build();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedAnotherTypeName = $"{MethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["int_property"] = new JsonSchemaBuilder().FromType<int>().Build(),
                    ["string_property"] = new JsonSchemaBuilder().FromType<string>().Build(),
                    ["nested_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeName}").Build(),
                    ["another_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedAnotherTypeName}").Build()
                })
                .Build(),
            [expectedAnotherTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().Build()
                })
                .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_AlreadyRegisteredEnum_DontRegisterAgain()
    {
        var type = typeof(Enum);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);
        schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(Enum)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .Build();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Enum("one", "two")
                .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_AlreadyRegisteredTypeWithProperties_DontRegisterAgain()
    {
        var type = typeof(TypeWithProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);
        schemaGenerator.CreateOrRef(typeof(AnotherTypeWithProperties), MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithProperties)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .Build();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedAnotherTypeName = $"{MethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["int_property"] = new JsonSchemaBuilder().FromType<int>().Build(),
                    ["string_property"] = new JsonSchemaBuilder().FromType<string>().Build(),
                    ["nested_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeName}").Build(),
                    ["another_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedAnotherTypeName}").Build()
                })
                .Build(),
            [expectedAnotherTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().Build()
                })
                .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_AlreadyRegisteredTypeWithPropertiesWithAnotherMethodName_RegisterAgain()
    {
        var type = typeof(AnotherTypeWithProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };
        var anotherMethodName = "anotherMethodName";

        var result1 = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);
        var result2 = schemaGenerator.CreateOrRef(type, anotherMethodName, jsonSerializerOptions);

        var expectedTypeName1 = $"{MethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedTypeName2 = $"{anotherMethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedSchema1 = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName1}")
            .Build();
        var expectedSchema2 = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName2}")
            .Build();
        result1.Should().BeEquivalentTo(expectedSchema1);
        result2.Should().BeEquivalentTo(expectedSchema2);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName1] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().Build()
                })
                .Build(),
            [expectedTypeName2] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().Build()
                })
                .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void GetAllSchemas_ChangingCollection_DontAffectInnerCollection()
    {
        var schemas = schemaGenerator.GetAllSchemas();
        schemas.Should().BeEmpty();

        schemas["123"] = JsonSchema.Empty;
        schemas.Should().NotBeEmpty();

        schemaGenerator.GetAllSchemas().Should().BeEmpty();
    }

    private const string MethodName = "methodName";

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum Enum
    {
        One,
        Two
    }

    private record TypeWithProperties(int IntProperty, string StringProperty, TypeWithProperties NestedProperty, AnotherTypeWithProperties AnotherProperty);

    private record AnotherTypeWithProperties(bool BoolProperty);

    private record CustomSimpleType;
}
