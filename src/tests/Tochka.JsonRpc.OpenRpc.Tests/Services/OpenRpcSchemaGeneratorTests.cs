using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Json.Schema;
using Json.Schema.Generation;
using NUnit.Framework;
using Tochka.JsonRpc.OpenRpc.Services;

namespace Tochka.JsonRpc.OpenRpc.Tests.Services;

[TestFixture]
[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "For tests")]
[SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "For tests")]
internal sealed class OpenRpcSchemaGeneratorTests
{
    private OpenRpcSchemaGenerator schemaGenerator;

    [SetUp]
    public void Setup() => schemaGenerator = new OpenRpcSchemaGenerator();

    [Test]
    public void CreateOrRef_CollectionWithSimpleItems_ReturnSchemaItself()
    {
        var type = typeof(IEnumerable<string>);
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Array)
            .Items(new JsonSchemaBuilder()
                .FromType<string>()
                .BuildWithoutUri())
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>();
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_CollectionWithCustomSimpleItems_ReturnSchemaItself()
    {
        var type = typeof(IEnumerable<CustomSimpleType>);
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Array)
            .Items(new JsonSchemaBuilder()
                .FromType<CustomSimpleType>()
                .BuildWithoutUri())
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>();
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_CollectionWithEnumItems_ReturnSchemaWithRefForItems()
    {
        var type = typeof(IEnumerable<Enum>);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(Enum)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Array)
            .Items(new JsonSchemaBuilder()
                .Ref($"#/components/schemas/{expectedTypeName}")
                .BuildWithoutUri())
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Enum("one", "two")
                .OneOf(new JsonSchemaBuilder().Const("one"), new JsonSchemaBuilder().Const("two"))
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_CollectionWithItemsWithProperties_ReturnSchemaWithRefForItems()
    {
        var type = typeof(IEnumerable<TypeWithProperties>);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithProperties)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Array)
            .Items(new JsonSchemaBuilder()
                .Ref($"#/components/schemas/{expectedTypeName}")
                .BuildWithoutUri())
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedAnotherTypeName = $"{MethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["int_property"] = new JsonSchemaBuilder().FromType<int>().BuildWithoutUri(),
                    ["string_property"] = new JsonSchemaBuilder().FromType<string>().BuildWithoutUri(),
                    ["nested_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeName}").BuildWithoutUri(),
                    ["another_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedAnotherTypeName}").BuildWithoutUri()
                })
                .Required("int_property", "string_property", "nested_property", "another_property")
                .BuildWithoutUri(),
            [expectedAnotherTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().BuildWithoutUri()
                })
                .Required("bool_property")
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_Enum_ReturnRef()
    {
        var type = typeof(Enum);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(Enum)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Enum("one", "two")
                .OneOf(new JsonSchemaBuilder().Const("one"), new JsonSchemaBuilder().Const("two"))
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_NullableEnum_ReturnRef()
    {
        var type = typeof(Enum?);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(Enum)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Enum("one", "two")
                .OneOf(new JsonSchemaBuilder().Const("one"), new JsonSchemaBuilder().Const("two"))
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_SimpleType_ReturnSchemaItself()
    {
        var type = typeof(string);
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedSchema = new JsonSchemaBuilder()
            .FromType<string>()
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>();
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_CustomSimpleType_ReturnSchemaItself()
    {
        var type = typeof(CustomSimpleType);
        var jsonSerializerOptions = new JsonSerializerOptions();

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedSchema = new JsonSchemaBuilder()
            .FromType<CustomSimpleType>()
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>();
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_TypeWithProperties_ReturnRef()
    {
        var type = typeof(TypeWithProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithProperties)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedAnotherTypeName = $"{MethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["int_property"] = new JsonSchemaBuilder().FromType<int>().BuildWithoutUri(),
                    ["string_property"] = new JsonSchemaBuilder().FromType<string>().BuildWithoutUri(),
                    ["nested_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeName}").BuildWithoutUri(),
                    ["another_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedAnotherTypeName}").BuildWithoutUri()
                })
                .Required("int_property", "string_property", "nested_property", "another_property")
                .BuildWithoutUri(),
            [expectedAnotherTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().BuildWithoutUri()
                })
                .Required("bool_property")
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_TypeWithSomeNullableProperties_ReturnRef()
    {
        var type = typeof(TypeWithSomeNullableProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithSomeNullableProperties)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedAnotherTypeName = $"{MethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["int_property"] = new JsonSchemaBuilder().FromType<int>().BuildWithoutUri(),
                    ["string_property"] = new JsonSchemaBuilder().FromType<string>().BuildWithoutUri(),
                    ["nested_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeName}").BuildWithoutUri(),
                    ["another_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedAnotherTypeName}").BuildWithoutUri()
                })
                .Required("string_property", "nested_property")
                .BuildWithoutUri(),
            [expectedAnotherTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().BuildWithoutUri()
                })
                .Required("bool_property")
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_AlreadyRegisteredEnum_DontRegisterAgain()
    {
        var type = typeof(Enum);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);
        schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(Enum)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Enum("one", "two")
                .OneOf(new JsonSchemaBuilder().Const("one"), new JsonSchemaBuilder().Const("two"))
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_AlreadyRegisteredTypeWithProperties_DontRegisterAgain()
    {
        var type = typeof(TypeWithProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var result = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);
        schemaGenerator.CreateOrRef(typeof(AnotherTypeWithProperties), null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithProperties)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .BuildWithoutUri();
        result.Should().BeEquivalentTo(expectedSchema);
        var expectedAnotherTypeName = $"{MethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["int_property"] = new JsonSchemaBuilder().FromType<int>().BuildWithoutUri(),
                    ["string_property"] = new JsonSchemaBuilder().FromType<string>().BuildWithoutUri(),
                    ["nested_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeName}").BuildWithoutUri(),
                    ["another_property"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedAnotherTypeName}").BuildWithoutUri()
                })
                .Required("int_property", "string_property", "nested_property", "another_property")
                .BuildWithoutUri(),
            [expectedAnotherTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().BuildWithoutUri()
                })
                .Required("bool_property")
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_AlreadyRegisteredTypeWithPropertiesWithAnotherMethodName_RegisterAgain()
    {
        var type = typeof(AnotherTypeWithProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var anotherMethodName = "anotherMethodName";

        var result1 = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);
        var result2 = schemaGenerator.CreateOrRef(type, null, anotherMethodName, jsonSerializerOptions);

        var expectedTypeName1 = $"{MethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedTypeName2 = $"{anotherMethodName} {nameof(AnotherTypeWithProperties)}";
        var expectedSchema1 = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName1}")
            .BuildWithoutUri();
        var expectedSchema2 = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName2}")
            .BuildWithoutUri();
        result1.Should().BeEquivalentTo(expectedSchema1);
        result2.Should().BeEquivalentTo(expectedSchema2);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName1] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().BuildWithoutUri()
                })
                .Required("bool_property")
                .BuildWithoutUri(),
            [expectedTypeName2] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["bool_property"] = new JsonSchemaBuilder().FromType<bool>().BuildWithoutUri()
                })
                .Required("bool_property")
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_EnumValuesFormatedAsDeclared()
    {
        var type = typeof(Enum2);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(Enum2)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .BuildWithoutUri();
        actualSchema.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Enum("Value1", "ValueValue2", "value3", "value_value4")
                .OneOf(new JsonSchemaBuilder().Const("Value1"), 
                       new JsonSchemaBuilder().Const("ValueValue2"), 
                       new JsonSchemaBuilder().Const("value3"), 
                       new JsonSchemaBuilder().Const("value_value4"))
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }
    
    [Test]
    public void CreateOrRef_EnumValueSummaryCollectedAsDescription()
    {
        var type = typeof(EnumWithSummary);
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new JsonStringEnumConverter() }
        };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(EnumWithSummary)}";
        var expectedSchema = new JsonSchemaBuilder()
                             .Ref($"#/components/schemas/{expectedTypeName}")
                             .BuildWithoutUri();
        actualSchema.Should().BeEquivalentTo(expectedSchema);

        var actualSchemas = schemaGenerator.GetAllSchemas();
        actualSchemas[expectedTypeName].Keywords.Count.Should().Be(2);
        
        var keywords = actualSchemas[expectedTypeName].Keywords.ToArray();
        keywords[0].Should().BeAssignableTo(typeof(EnumKeyword));
        var enumKeywords = ((EnumKeyword)keywords[0]).Values.Select(static x => x.ToString()).ToArray();
        enumKeywords.Should().BeEquivalentTo("One", "Two");

        var oneOfKeyword = keywords[1];
        oneOfKeyword.Should().BeAssignableTo(typeof(OneOfKeyword));
        var oneOfKeywordSchemas = ((OneOfKeyword)oneOfKeyword).Schemas;
        oneOfKeywordSchemas.Count.Should().Be(2);
        
        var firstEnumValueOneOfKeywordSchemas = oneOfKeywordSchemas[0].Keywords.ToArray();
        firstEnumValueOneOfKeywordSchemas.Length.Should().Be(2);
        firstEnumValueOneOfKeywordSchemas[0].Should().BeAssignableTo(typeof(ConstKeyword));
        ((ConstKeyword)firstEnumValueOneOfKeywordSchemas[0]).Value.ToString().Should().Be("One");
        firstEnumValueOneOfKeywordSchemas[1].Should().BeAssignableTo(typeof(DescriptionKeyword));
        ((DescriptionKeyword)firstEnumValueOneOfKeywordSchemas[1]).Value.Should().Be("One Summary");
        
        var secondEnumValueOneOfKeywordSchemas = oneOfKeywordSchemas[1].Keywords.ToArray();
        secondEnumValueOneOfKeywordSchemas.Length.Should().Be(2);
        secondEnumValueOneOfKeywordSchemas[0].Should().BeAssignableTo(typeof(ConstKeyword));
        ((ConstKeyword)secondEnumValueOneOfKeywordSchemas[0]).Value.ToString().Should().Be("Two");
        secondEnumValueOneOfKeywordSchemas[1].Should().BeAssignableTo(typeof(DescriptionKeyword));
        ((DescriptionKeyword)secondEnumValueOneOfKeywordSchemas[1]).Value.Should().Be("Two Summary");
    }

    [Test]
    public void CreateOrRef_DefaultSimpleTypesFormattedAsString()
    {
        var type = typeof(TypeWithSimpleProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithSimpleProperties)}";
        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedTypeName}")
            .BuildWithoutUri();
        actualSchema.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["date_time"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.DateTime).BuildWithoutUri(),
                    ["date_time_offset"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.DateTime).BuildWithoutUri(),
                    ["date_only"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.Date).BuildWithoutUri(),
                    ["time_only"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.Time).BuildWithoutUri(),
                    ["time_span"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.Duration).BuildWithoutUri(),
                    ["guid"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.Uuid).BuildWithoutUri()
                })
                .Required("date_time", "date_time_offset", "date_only", "time_only", "time_span", "guid")
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_SummariesFromResultObjectPropertiesCollectedAsTitlesOnJsonSchema()
    {
        var type = typeof(TypeWithSummaries);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithSummaries)}";
        var expectedTypeNameInner = $"{MethodName} {nameof(TypeWithSummariesInner)}";
        var expectedTypeNameInnerEnum = $"{MethodName} {nameof(TypeWithSummariesInnerEnum)}";

        actualSchema.Should()
            .BeEquivalentTo(new JsonSchemaBuilder()
                .Ref($"#/components/schemas/{expectedTypeName}")
                .BuildWithoutUri());

        var actualSchemas = schemaGenerator.GetAllSchemas();

        var expectedSchemas = new Dictionary<string, JsonSchema>
        {
            [expectedTypeNameInner] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["inner_prop1"] = new JsonSchemaBuilder().Type(SchemaValueType.String)
                        .Title("InnerProp1")
                        .BuildWithoutUri()
                })
                .Required("inner_prop1")
                .BuildWithoutUri(),
            [expectedTypeNameInnerEnum] = new JsonSchemaBuilder()
                .Enum("bla")
                .OneOf(new JsonSchemaBuilder().Const("bla"))
                .BuildWithoutUri(),
            [expectedTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["prop1"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeNameInner}")
                        .Title("Prop1")
                        .BuildWithoutUri(),
                    ["prop2"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeNameInner}")
                        .Title("Prop2")
                        .BuildWithoutUri(),
                    ["prop3"] = new JsonSchemaBuilder().Type(SchemaValueType.Array)
                        .Items(new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeNameInner}")
                            .BuildWithoutUri())
                        .Title("Prop3")
                        .BuildWithoutUri(),
                    ["prop4"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeNameInnerEnum}")
                        .Title("Prop4")
                        .BuildWithoutUri(),
                    ["prop5"] = new JsonSchemaBuilder().Type(SchemaValueType.String)
                        .Format(Formats.Duration)
                        .Title("Prop5")
                        .BuildWithoutUri()
                })
                .Required("prop1", "prop2", "prop3", "prop4", "prop5")
                .BuildWithoutUri()
        };

        actualSchemas.Count.Should().Be(expectedSchemas.Count);

        var actualKeys = actualSchemas.Keys.ToList();
        var actualValues = actualSchemas.Values.ToList();

        var expectedKeys = expectedSchemas.Keys.ToList();
        var expectedValues = expectedSchemas.Values.ToList();

        actualKeys.Count.Should().Be(expectedKeys.Count);
        actualValues.Count.Should().Be(expectedValues.Count);

        for (var i = 0; i < expectedSchemas.Count; i++)
        {
            var actualKey = actualKeys[i];
            var actualValue = actualValues[i];

            var expectedKey = expectedKeys[i];
            var expectedValue = expectedValues[i];

            actualKey.Should().BeEquivalentTo(expectedKey);
            actualValue.Should().BeEquivalentTo(expectedValue);
        }
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

    [Test]
    public void CreateOrRef_TypeWithGenericPropertyParsedCorrectly_OneTypeArgument()
    {
        var type = typeof(TypeWithGenericProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedParentTypeName = $"{MethodName} {nameof(TypeWithGenericProperties)}";
        var expectedProperty1Name = $"{MethodName} GenericOneType`1[TypeWithGenericPropertiesFirstEnum]";
        var expectedProperty2Name = $"{MethodName} GenericOneType`1[TypeWithGenericPropertiesSecondEnum]";
        var expectedFirstEnumTypeName = $"{MethodName} {nameof(TypeWithGenericPropertiesFirstEnum)}";
        var expectedSecondEnumTypeName = $"{MethodName} {nameof(TypeWithGenericPropertiesSecondEnum)}";

        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedParentTypeName}")
            .BuildWithoutUri();

        actualSchema.Should().BeEquivalentTo(expectedSchema);

        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedFirstEnumTypeName] = new JsonSchemaBuilder()
                .Enum("type_with_generic_properties_first_enum")
                .OneOf(new JsonSchemaBuilder().Const("type_with_generic_properties_first_enum"))
                .BuildWithoutUri(),
            [expectedSecondEnumTypeName] = new JsonSchemaBuilder()
                .Enum("type_with_generic_properties_second_enum")
                .OneOf(new JsonSchemaBuilder().Const("type_with_generic_properties_second_enum"))
                .BuildWithoutUri(),
            [expectedProperty1Name] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["generic_property"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedFirstEnumTypeName}")
                        .BuildWithoutUri()
                })
                .Required("generic_property")
                .BuildWithoutUri(),
            [expectedProperty2Name] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["generic_property"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedSecondEnumTypeName}")
                        .BuildWithoutUri()
                })
                .BuildWithoutUri(),
            [expectedParentTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["property1"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedProperty1Name}")
                        .BuildWithoutUri(),
                    ["property2"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedProperty2Name}")
                        .BuildWithoutUri()
                })
                .Required("property1", "property2")
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_TypeWithSomeNullableGenericPropertiesHasCorrectRequiredState_Prop1Optional_Prop2Required()
    {
        var type = typeof(TypeWithSomeNullableGenericProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedParentTypeName = $"{MethodName} {nameof(TypeWithSomeNullableGenericProperties)}";
        var expectedProperty1Name = $"{MethodName} GenericOneType`1[TypeWithGenericPropertiesFirstEnum]";
        var expectedProperty2Name = $"{MethodName} GenericOneType`1[TypeWithGenericPropertiesSecondEnum]";
        var expectedFirstEnumTypeName = $"{MethodName} {nameof(TypeWithGenericPropertiesFirstEnum)}";
        var expectedSecondEnumTypeName = $"{MethodName} {nameof(TypeWithGenericPropertiesSecondEnum)}";

        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedParentTypeName}")
            .BuildWithoutUri();

        actualSchema.Should().BeEquivalentTo(expectedSchema);

        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedFirstEnumTypeName] = new JsonSchemaBuilder()
                .Enum("type_with_generic_properties_first_enum")
                .OneOf(new JsonSchemaBuilder().Const("type_with_generic_properties_first_enum"))
                .BuildWithoutUri(),
            [expectedSecondEnumTypeName] = new JsonSchemaBuilder()
                .Enum("type_with_generic_properties_second_enum")
                .OneOf(new JsonSchemaBuilder().Const("type_with_generic_properties_second_enum"))
                .BuildWithoutUri(),
            [expectedProperty1Name] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["generic_property"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedFirstEnumTypeName}")
                        .BuildWithoutUri()
                })
                .Required("generic_property")
                .BuildWithoutUri(),
            [expectedProperty2Name] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["generic_property"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedSecondEnumTypeName}")
                        .BuildWithoutUri()
                })
                .BuildWithoutUri(),
            [expectedParentTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["property1"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedProperty1Name}")
                        .BuildWithoutUri(),
                    ["property2"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedProperty2Name}")
                        .BuildWithoutUri()
                })
                .Required("property2")
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_TypeWithGenericPropertyParsedCorrectly_TwoTypeArgument()
    {
        var type = typeof(TypeWithGenericTwoTypesProperty);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedParentTypeName = $"{MethodName} {nameof(TypeWithGenericTwoTypesProperty)}";
        var expectedProperty1TypeName = $"{MethodName} GenericTwoType`2[String,Boolean]";

        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedParentTypeName}")
            .BuildWithoutUri();

        actualSchema.Should().BeEquivalentTo(expectedSchema);

        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedProperty1TypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["generic_property1"] = new JsonSchemaBuilder()
                        .Type(SchemaValueType.String)
                        .BuildWithoutUri(),
                    ["generic_property2"] = new JsonSchemaBuilder()
                        .Type(SchemaValueType.Boolean)
                        .BuildWithoutUri()
                })
                .Required("generic_property1", "generic_property2")
                .BuildWithoutUri(),
            [expectedParentTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["property1"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedProperty1TypeName}")
                        .BuildWithoutUri()
                })
                .Required("property1")
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_TypeWithSomeNullableGenericPropertiesHasCorrectRequiredState_AllOptional()
    {
        var type = typeof(TypeWithNullableGenericTwoTypesProperty);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedParentTypeName = $"{MethodName} {nameof(TypeWithNullableGenericTwoTypesProperty)}";
        var expectedProperty1TypeName = $"{MethodName} GenericTwoType`2[String,Boolean]";

        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedParentTypeName}")
            .BuildWithoutUri();

        actualSchema.Should().BeEquivalentTo(expectedSchema);

        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedProperty1TypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["generic_property1"] = new JsonSchemaBuilder()
                        .Type(SchemaValueType.String)
                        .BuildWithoutUri(),
                    ["generic_property2"] = new JsonSchemaBuilder()
                        .Type(SchemaValueType.Boolean)
                        .BuildWithoutUri()
                })
                .BuildWithoutUri(),
            [expectedParentTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["property1"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedProperty1TypeName}")
                        .BuildWithoutUri()
                })
                .BuildWithoutUri()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_TypeWithGenericPropertyParsedCorrectly_ChildGeneric()
    {
        var type = typeof(TypeWithChildGenericTypyProperty);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedParentTypeName = $"{MethodName} {nameof(TypeWithChildGenericTypyProperty)}";
        var expectedProperty1Name = $"{MethodName} GenericTwoType`2[String,GenericOneType`1[Boolean]]";
        var expectedProperty1ChildGenericTypeName = $"{MethodName} GenericOneType`1[Boolean]";

        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedParentTypeName}")
            .BuildWithoutUri();

        actualSchema.Should().BeEquivalentTo(expectedSchema);

        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedProperty1ChildGenericTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["generic_property"] = new JsonSchemaBuilder()
                        .Type(SchemaValueType.Boolean)
                        .BuildWithoutUri()
                })
                .Required("generic_property")
                .BuildWithoutUri(),
            [expectedProperty1Name] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["generic_property1"] = new JsonSchemaBuilder()
                        .Type(SchemaValueType.String)
                        .BuildWithoutUri(),
                    ["generic_property2"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedProperty1ChildGenericTypeName}")
                        .BuildWithoutUri()
                })
                .Required("generic_property1", "generic_property2")
                .BuildWithoutUri(),
            [expectedParentTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["property1"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedProperty1Name}")
                        .BuildWithoutUri()
                })
                .Required("property1")
                .BuildWithoutUri()
        };

        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_TypeWithGenericPropertyHasCorrectRequiredState_ChildGenericNullable()
    {
        var type = typeof(TypeWithNullableChildGenericTypyProperty);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var expectedParentTypeName = $"{MethodName} {nameof(TypeWithNullableChildGenericTypyProperty)}";
        var expectedProperty1Name = $"{MethodName} GenericTwoType`2[String,GenericOneType`1[Boolean]]";
        var expectedProperty1ChildGenericTypeName = $"{MethodName} GenericOneType`1[Boolean]";

        var expectedSchema = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{expectedParentTypeName}")
            .BuildWithoutUri();

        actualSchema.Should().BeEquivalentTo(expectedSchema);

        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedProperty1ChildGenericTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["generic_property"] = new JsonSchemaBuilder()
                        .Type(SchemaValueType.Boolean)
                        .BuildWithoutUri()
                })
                .Required("generic_property")
                .BuildWithoutUri(),
            [expectedProperty1Name] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["generic_property1"] = new JsonSchemaBuilder()
                        .Type(SchemaValueType.String)
                        .BuildWithoutUri(),
                    ["generic_property2"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedProperty1ChildGenericTypeName}")
                        .BuildWithoutUri()
                })
                .Required("generic_property1")
                .BuildWithoutUri(),
            [expectedParentTypeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["property1"] = new JsonSchemaBuilder()
                        .Ref($"#/components/schemas/{expectedProperty1Name}")
                        .BuildWithoutUri()
                })
                .Required("property1")
                .BuildWithoutUri()
        };

        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }

    [Test]
    public void CreateOrRef_SystemTextJsonAttributesHandling()
    {
        var type = typeof(TypeWithJsonAttributes);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        var actualSchema = schemaGenerator.CreateOrRef(type, null, MethodName, jsonSerializerOptions);

        var typeName = $"{MethodName} {nameof(TypeWithJsonAttributes)}";
        var typeNameInnerEnum = $"{MethodName} {nameof(TypeWithJsonAttributesEnum)}";

        actualSchema.Should().BeEquivalentTo(new JsonSchemaBuilder().Ref($"#/components/schemas/{typeName}").BuildWithoutUri());

        var expectedSchemas = new Dictionary<string, JsonSchema>
        {
            [typeNameInnerEnum] = new JsonSchemaBuilder()
                                  .Enum("MyEnumValue")                
                                  .OneOf(new JsonSchemaBuilder().Const("MyEnumValue"))
                                  .BuildWithoutUri(),
            [typeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(new Dictionary<string, JsonSchema>
                {
                    ["custom_name_1"] = new JsonSchemaBuilder().Type(SchemaValueType.String).BuildWithoutUri(),
                    ["custom_name_2"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{typeNameInnerEnum}").BuildWithoutUri()
                })
                .Required("custom_name_1", "custom_name_2")
                .BuildWithoutUri()
        };

        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedSchemas);
    }

    private const string MethodName = "methodName";

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum Enum
    {
        One,
        Two
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum Enum2
    {
        Value1,
        ValueValue2,
        value3,
        value_value4
    }
    
    private enum EnumWithSummary
    {
        /// <summary>
        /// One Summary
        /// </summary>
        One,
        
        /// <summary>
        /// Two Summary
        /// </summary>
        Two
    }

    private enum TypeWithSummariesInnerEnum
    {
        Bla
    }

    private enum TypeWithGenericPropertiesFirstEnum
    {
        TypeWithGenericPropertiesFirstEnum
    }

    private enum TypeWithGenericPropertiesSecondEnum
    {
        TypeWithGenericPropertiesSecondEnum
    }

    private enum TypeWithJsonAttributesEnum
    {
        MyEnumValue
    }

    private sealed class TypeWithSummaries
    {
        /// <summary>
        /// Prop1
        /// </summary>
        public TypeWithSummariesInner Prop1 { get; set; }

        /// <summary>
        /// Prop2
        /// </summary>
        public TypeWithSummariesInner Prop2 { get; set; }

        /// <summary>
        /// Prop3
        /// </summary>
        public TypeWithSummariesInner[] Prop3 { get; set; }

        /// <summary>
        /// Prop4
        /// </summary>
        public TypeWithSummariesInnerEnum Prop4 { get; set; }

        /// <summary>
        /// Prop5
        /// </summary>
        public TimeSpan Prop5 { get; set; }
    }

    private sealed class TypeWithSummariesInner
    {
        /// <summary>
        /// InnerProp1
        /// </summary>
        public string InnerProp1 { get; set; }
    }

    private sealed class TypeWithGenericProperties
    {
        public GenericOneType<TypeWithGenericPropertiesFirstEnum> Property1 { get; set; }
        public GenericOneType<TypeWithGenericPropertiesSecondEnum?> Property2 { get; set; }
    }

    private sealed class TypeWithSomeNullableGenericProperties
    {
        public GenericOneType<TypeWithGenericPropertiesFirstEnum>? Property1 { get; set; }
        public GenericOneType<TypeWithGenericPropertiesSecondEnum?> Property2 { get; set; }
    }

    private sealed class GenericOneType<T>
    {
        public T GenericProperty { get; set; }
    }

    private sealed class TypeWithGenericTwoTypesProperty
    {
        public GenericTwoType<string, bool> Property1 { get; set; }
    }

    private sealed class TypeWithNullableGenericTwoTypesProperty
    {
        public GenericTwoType<string?, bool?>? Property1 { get; set; }
    }

    private sealed class GenericTwoType<T, U>
    {
        public T GenericProperty1 { get; set; }
        public U GenericProperty2 { get; set; }
    }

    private sealed class TypeWithChildGenericTypyProperty
    {
        public GenericTwoType<string, GenericOneType<bool>> Property1 { get; set; }
    }

    private sealed class TypeWithNullableChildGenericTypyProperty
    {
        public GenericTwoType<string, GenericOneType<bool>?> Property1 { get; set; }
    }

    private sealed class TypeWithJsonAttributes
    {
        [JsonPropertyName("custom_name_1")]
        public string Prop1 { get; set; }

        [JsonIgnore]
        public string Prop2 { get; set; }

        [JsonPropertyName("custom_name_2")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TypeWithJsonAttributesEnum Prop3 { get; set; }
    }

    private sealed record TypeWithProperties
    (
        int IntProperty,
        string StringProperty,
        TypeWithProperties NestedProperty,
        AnotherTypeWithProperties AnotherProperty
    );

    private sealed record TypeWithSomeNullableProperties
    (
        int? IntProperty,
        string StringProperty,
        TypeWithSomeNullableProperties NestedProperty,
        AnotherTypeWithProperties? AnotherProperty
    );

    private sealed record AnotherTypeWithProperties
    (
        bool BoolProperty
    );

    private sealed record TypeWithSimpleProperties
    (
        DateTime DateTime,
        DateTimeOffset DateTimeOffset,
        DateOnly DateOnly,
        TimeOnly TimeOnly,
        TimeSpan TimeSpan,
        Guid Guid
    );

    private sealed record CustomSimpleType;
}
