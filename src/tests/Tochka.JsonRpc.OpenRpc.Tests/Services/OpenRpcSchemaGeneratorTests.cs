using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    public void CreateOrRef_NullableEnum_ReturnRef()
    {
        var type = typeof(Enum?);
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
    public void CreateOrRef_EnumValuesFormatedAsDeclared()
    {
        var type = typeof(Enum2);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
        
        var actualSchema = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(Enum2)}";
        var expectedSchema = new JsonSchemaBuilder()
                             .Ref($"#/components/schemas/{expectedTypeName}")
                             .Build();
        actualSchema.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                                 .Enum("Value1", "ValueValue2", "value3", "value_value4")
                                 .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }
    
    [Test]
    public void CreateOrRef_DefaultSimpleTypesFormattedAsString()
    {
        var type = typeof(TypeWithSimpleProperties);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };
        
        var actualSchema = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithSimpleProperties)}";
        var expectedSchema = new JsonSchemaBuilder()
                             .Ref($"#/components/schemas/{expectedTypeName}")
                             .Build();
        actualSchema.Should().BeEquivalentTo(expectedSchema);
        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedTypeName] = new JsonSchemaBuilder()
                                 .Type(SchemaValueType.Object)
                                 .Properties(new Dictionary<string, JsonSchema>
                                 {
                                     ["date_time"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.DateTime).Build(),
                                     ["date_time_offset"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.DateTime).Build(),
                                     ["date_only"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.Date).Build(),
                                     ["time_only"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.Time).Build(),
                                     ["time_span"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.Duration).Build(),
                                     ["guid"] = new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.Uuid).Build()
                                 })
                                 .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }
    
    [Test]
    public void CreateOrRef_SummariesFromResultObjectPropertiesCollectedAsTitlesOnJsonSchema()
    {
        var type = typeof(TypeWithSummaries);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };
        
        var actualSchema = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedTypeName = $"{MethodName} {nameof(TypeWithSummaries)}";
        var expectedTypeNameInner = $"{MethodName} {nameof(TypeWithSummariesInner)}";
        var expectedTypeNameInnerEnum = $"{MethodName} {nameof(TypeWithSummariesInnerEnum)}";

        actualSchema.Should().BeEquivalentTo(new JsonSchemaBuilder()
                                             .Ref($"#/components/schemas/{expectedTypeName}")
                                             .Build());
        
        var actualSchemas = schemaGenerator.GetAllSchemas();
        
        var expectedSchemas = new Dictionary<string, JsonSchema>
        {
            [expectedTypeNameInner] = new JsonSchemaBuilder()
                                      .Type(SchemaValueType.Object)
                                      .Properties(new Dictionary<string, JsonSchema>
                                      {
                                          ["inner_prop1"] = new JsonSchemaBuilder().Type(SchemaValueType.String)
                                                                                   .Title("InnerProp1")
                                                                                   .Build()
                                      })
                                      .Build(),
            [expectedTypeNameInnerEnum] = new JsonSchemaBuilder()
                                          .Enum("bla")
                                          .Build(),
            [expectedTypeName] = new JsonSchemaBuilder()
                                 .Type(SchemaValueType.Object)
                                 .Properties(new Dictionary<string, JsonSchema>
                                 {
                                     ["prop1"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeNameInner}")
                                                                        .Title("Prop1")
                                                                        .Build(),
                                     ["prop2"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeNameInner}")
                                                                        .Title("Prop2")
                                                                        .Build(),
                                     ["prop3"] = new JsonSchemaBuilder().Type(SchemaValueType.Array)
                                                                        .Items(new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeNameInner}")
                                                                                                      .Build())
                                                                        .Title("Prop3")
                                                                        .Build(),
                                     ["prop4"] = new JsonSchemaBuilder().Ref($"#/components/schemas/{expectedTypeNameInnerEnum}")
                                                                        .Title("Prop4")
                                                                        .Build(),
                                     ["prop5"] = new JsonSchemaBuilder().Type(SchemaValueType.String)
                                                                        .Format(Formats.Duration)
                                                                        .Title("Prop5")
                                                                        .Build()
                                 })
                                 .Build(),
        };

        actualSchemas.Count.Should().Be(expectedSchemas.Count);

        var actualKeys = actualSchemas.Keys.ToArray();
        var actualValues = actualSchemas.Values.ToArray();
        
        var expectedKeys = expectedSchemas.Keys.ToArray();
        var expectedValues = expectedSchemas.Values.ToArray();
        
        actualKeys.Length.Should().Be(expectedKeys.Length);
        actualValues.Length.Should().Be(expectedValues.Length);
        
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
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };
        
        var actualSchema = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedParentTypeName = $"{MethodName} {nameof(TypeWithGenericProperties)}";
        var expectedProperty1Name = $"{MethodName} GenericOneType`1[TypeWithGenericPropertiesFirstEnum]";
        var expectedProperty2Name = $"{MethodName} GenericOneType`1[TypeWithGenericPropertiesSecondEnum]";
        var expectedFirstEnumTypeName = $"{MethodName} {nameof(TypeWithGenericPropertiesFirstEnum)}";
        var expectedSecondEnumTypeName = $"{MethodName} {nameof(TypeWithGenericPropertiesSecondEnum)}";

        var expectedSchema = new JsonSchemaBuilder()
                             .Ref($"#/components/schemas/{expectedParentTypeName}")
                             .Build();
        
        actualSchema.Should().BeEquivalentTo(expectedSchema);

        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedFirstEnumTypeName] = new JsonSchemaBuilder()
                                          .Enum("type_with_generic_properties_first_enum")
                                          .Build(),
            [expectedSecondEnumTypeName] = new JsonSchemaBuilder()
                                           .Enum("type_with_generic_properties_second_enum")
                                           .Build(),
            [expectedProperty1Name] = new JsonSchemaBuilder()
                                            .Type(SchemaValueType.Object)
                                            .Properties(new Dictionary<string, JsonSchema>
                                            {
                                                ["generic_property"] = new JsonSchemaBuilder()
                                                                       .Ref($"#/components/schemas/{expectedFirstEnumTypeName}")
                                                                       .Build()
                                            })
                                            .Build(),
            [expectedProperty2Name] = new JsonSchemaBuilder()
                                            .Type(SchemaValueType.Object)
                                            .Properties(new Dictionary<string, JsonSchema>
                                            {
                                                ["generic_property"] = new JsonSchemaBuilder()
                                                                       .Ref($"#/components/schemas/{expectedSecondEnumTypeName}")
                                                                       .Build()
                                            })
                                            .Build(),
            [expectedParentTypeName] = new JsonSchemaBuilder()
                                                      .Type(SchemaValueType.Object)
                                                      .Properties(new Dictionary<string, JsonSchema>
                                                      {
                                                          ["property1"] = new JsonSchemaBuilder()
                                                                          .Ref($"#/components/schemas/{expectedProperty1Name}")
                                                                          .Build(),
                                                          ["property2"] = new JsonSchemaBuilder()
                                                                          .Ref($"#/components/schemas/{expectedProperty2Name}")
                                                                          .Build()
                                                      })
                                                      .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }
    
    [Test]
    public void CreateOrRef_TypeWithGenericPropertyParsedCorrectly_TwoTypeArgument()
    {
        var type = typeof(TypeWithGenericTwoTypesProperty);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };
        
        var actualSchema = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedParentTypeName = $"{MethodName} {nameof(TypeWithGenericTwoTypesProperty)}";
        var expectedProperty1TypeName = $"{MethodName} GenericTwoType`2[String,Boolean]";

        var expectedSchema = new JsonSchemaBuilder()
                             .Ref($"#/components/schemas/{expectedParentTypeName}")
                             .Build();
        
        actualSchema.Should().BeEquivalentTo(expectedSchema);

        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedProperty1TypeName] = new JsonSchemaBuilder()
                                       .Type(SchemaValueType.Object)
                                       .Properties(new Dictionary<string, JsonSchema>
                                       {
                                           ["generic_property1"] = new JsonSchemaBuilder()
                                                               .Type(SchemaValueType.String)
                                                               .Build(),
                                           ["generic_property2"] = new JsonSchemaBuilder()
                                                                .Type(SchemaValueType.Boolean)
                                                                .Build()
                                       })
                                       .Build(),
            [expectedParentTypeName] = new JsonSchemaBuilder()
                                                            .Type(SchemaValueType.Object)
                                                            .Properties(new Dictionary<string, JsonSchema>
                                                            {
                                                                ["property1"] = new JsonSchemaBuilder()
                                                                                .Ref($"#/components/schemas/{expectedProperty1TypeName}")
                                                                                .Build()
                                                            })
                                                            .Build()
        };
        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
    }
    
    [Test]
    public void CreateOrRef_TypeWithGenericPropertyParsedCorrectly_ChildGeneric()
    {
        var type = typeof(TypeWithChildGenericTypyProperty);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower };
        
        var actualSchema = schemaGenerator.CreateOrRef(type, MethodName, jsonSerializerOptions);

        var expectedParentTypeName = $"{MethodName} {nameof(TypeWithChildGenericTypyProperty)}";
        var expectedProperty1Name = $"{MethodName} GenericTwoType`2[String,GenericOneType`1[Boolean]]";
        var expectedProperty1ChildGenericTypeName = $"{MethodName} GenericOneType`1[Boolean]";

        var expectedSchema = new JsonSchemaBuilder()
                             .Ref($"#/components/schemas/{expectedParentTypeName}")
                             .Build();
        
        actualSchema.Should().BeEquivalentTo(expectedSchema);

        var expectedRegistrations = new Dictionary<string, JsonSchema>
        {
            [expectedProperty1ChildGenericTypeName] = new JsonSchemaBuilder()
                                                      .Type(SchemaValueType.Object)
                                                      .Properties(new Dictionary<string, JsonSchema>
                                                      {
                                                          ["generic_property"] = new JsonSchemaBuilder()
                                                                                 .Type(SchemaValueType.Boolean)
                                                                                 .Build()
                                                      }),
            [expectedProperty1Name] = new JsonSchemaBuilder()
                                      .Type(SchemaValueType.Object)
                                      .Properties(new Dictionary<string, JsonSchema>
                                      {
                                          ["generic_property1"] = new JsonSchemaBuilder()
                                                                  .Type(SchemaValueType.String)
                                                                  .Build(),
                                          ["generic_property2"] = new JsonSchemaBuilder()
                                                                  .Ref($"#/components/schemas/{expectedProperty1ChildGenericTypeName}")
                                                                  .Build()
                                      })
                                      .Build(),
            [expectedParentTypeName] = new JsonSchemaBuilder()
                                       .Type(SchemaValueType.Object)
                                       .Properties(new Dictionary<string, JsonSchema>
                                       {
                                           ["property1"] = new JsonSchemaBuilder()
                                                           .Ref($"#/components/schemas/{expectedProperty1Name}")
                                                           .Build()
                                       })
                                       .Build()
        };

        schemaGenerator.GetAllSchemas().Should().BeEquivalentTo(expectedRegistrations);
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

    private record TypeWithProperties(int IntProperty, string StringProperty, TypeWithProperties NestedProperty, AnotherTypeWithProperties AnotherProperty);

    private record AnotherTypeWithProperties(bool BoolProperty);
    
    private record TypeWithSimpleProperties(DateTime DateTime, DateTimeOffset DateTimeOffset, DateOnly DateOnly, TimeOnly TimeOnly, TimeSpan TimeSpan, Guid Guid);
    
    private class TypeWithSummaries
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
    private class TypeWithSummariesInner
    {
        /// <summary>
        /// InnerProp1
        /// </summary>
        public string InnerProp1 { get; set; }
    }
    private enum TypeWithSummariesInnerEnum 
    {
        Bla
    }
    
    private record CustomSimpleType;
    
    private class TypeWithGenericProperties
    {
        public GenericOneType<TypeWithGenericPropertiesFirstEnum> Property1 { get; set; } 
        public GenericOneType<TypeWithGenericPropertiesSecondEnum?> Property2 { get; set; }
    }
    
    private enum TypeWithGenericPropertiesFirstEnum
    {
        TypeWithGenericPropertiesFirstEnum
    }
    
    private enum TypeWithGenericPropertiesSecondEnum
    {
        TypeWithGenericPropertiesSecondEnum
    }

    private class GenericOneType<T>
    {
        public T GenericProperty { get; set; }
    }
    
    private class TypeWithGenericTwoTypesProperty
    {
        public GenericTwoType<string, bool> Property1 { get; set; } 
    }
    
    private class GenericTwoType<T,U>
    {
        public T GenericProperty1 { get; set; }
        public U GenericProperty2 { get; set; }
    }
    
    private class TypeWithChildGenericTypyProperty
    {
        public GenericTwoType<string, GenericOneType<bool>> Property1 { get; set; } 
    }
}
