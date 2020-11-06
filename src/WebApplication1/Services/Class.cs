using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Attributes;
using DataType = Swashbuckle.AspNetCore.SwaggerGen.DataType;

namespace WebApplication1.Services
{
    public class SchemaGenerator : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions generatorOptions;
        private readonly ISerializerDataContractResolver defaultResolver;
        private readonly IEnumerable<IJsonRpcSerializer> serializers;

        public SchemaGenerator(SchemaGeneratorOptions generatorOptions, ISerializerDataContractResolver defaultResolver, IEnumerable<IJsonRpcSerializer> serializers)
        {
            this.generatorOptions = generatorOptions;
            this.defaultResolver = defaultResolver;
            this.serializers = serializers;
        }

        private ISerializerDataContractResolver GetResolver(Type type)
        {
            var serializerType = type.GetCustomAttribute<JsonRpcSerializerAttribute>()?.SerializerType;
            if (serializerType == null)
            {
                return defaultResolver;
            }

            var serializer = serializers.First(x => x.GetType() == serializerType);
            return new NewtonsoftDataContractResolver(generatorOptions, serializer.Settings);
        }

        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository, MemberInfo memberInfo = null, ParameterInfo parameterInfo = null)
        {
            return GenerateSchema(type, schemaRepository, null, memberInfo, parameterInfo);
        }

        private OpenApiSchema GenerateSchema(Type type,SchemaRepository schemaRepository,ISerializerDataContractResolver resolver,MemberInfo memberInfo = null,ParameterInfo parameterInfo = null)
        {
            try
            {
                var schema = TryGetCustomTypeMapping(type, out Func<OpenApiSchema> mapping)
                    ? mapping()
                    : GenerateSchemaForType(type, schemaRepository, resolver);

                if (memberInfo != null)
                {
                    ApplyMemberMetadata(schema, type, memberInfo);
                }
                else if (parameterInfo != null)
                {
                    ApplyParameterMetadata(schema, type, parameterInfo);
                }

                if (schema.Reference == null)
                {
                    ApplyFilters(schema, type, schemaRepository, memberInfo, parameterInfo);
                }

                return schema;
            }
            catch (Exception ex)
            {
                throw new SchemaGeneratorException(
                    message: $"Failed to generate Schema for type - {type}. See inner exception",
                    innerException: ex);
            }
        }

        private bool TryGetCustomTypeMapping(Type type, out Func<OpenApiSchema> mapping)
        {
            if (generatorOptions.CustomTypeMappings.TryGetValue(type, out mapping))
            {
                return true;
            }

            if (type.IsConstructedGenericType &&
                generatorOptions.CustomTypeMappings.TryGetValue(type.GetGenericTypeDefinition(), out mapping))
            {
                return true;
            }

            return false;
        }

        private OpenApiSchema GenerateSchemaForType(Type type, SchemaRepository schemaRepository, ISerializerDataContractResolver resolver)
        {
            if (type.IsNullable(out Type innerType))
            {
                return GenerateSchemaForType(innerType, schemaRepository, null);
            }

            if (type.IsAssignableToOneOf(typeof(IFormFile), typeof(FileResult)))
            {
                return new OpenApiSchema { Type = "string", Format = "binary" };
            }

            Func<OpenApiSchema> definitionFactory;
            bool returnAsReference;

            resolver = resolver ?? GetResolver(type);

            var dataContract = resolver.GetDataContractForType(type);

            switch (dataContract.DataType)
            {
                case DataType.Boolean:
                case DataType.Integer:
                case DataType.Number:
                case DataType.String:
                    {
                        definitionFactory = () => GeneratePrimitiveSchema(dataContract);
                        returnAsReference = type.IsEnum && !generatorOptions.UseInlineDefinitionsForEnums;
                        break;
                    }

                case DataType.Array:
                    {
                        definitionFactory = () => GenerateArraySchema(dataContract, schemaRepository, resolver);
                        returnAsReference = type == dataContract.ArrayItemType;
                        break;
                    }

                case DataType.Dictionary:
                    {
                        definitionFactory = () => GenerateDictionarySchema(dataContract, schemaRepository, resolver);
                        returnAsReference = type == dataContract.DictionaryValueType;
                        break;
                    }

                case DataType.Object:
                    {
                        if (generatorOptions.UseOneOfForPolymorphism && IsBaseTypeWithKnownSubTypes(type, out IEnumerable<Type> subTypes))
                        {
                            definitionFactory = () => GeneratePolymorphicSchema(dataContract, schemaRepository, subTypes, resolver);
                            returnAsReference = false;
                        }
                        else
                        {
                            definitionFactory = () => GenerateObjectSchema(dataContract, schemaRepository, resolver);
                            returnAsReference = true;
                        }

                        break;
                    }

                default:
                    {
                        definitionFactory = () => new OpenApiSchema();
                        returnAsReference = false;
                        break;
                    }
            }

            return returnAsReference
                ? GenerateReferencedSchema(type, schemaRepository, definitionFactory)
                : definitionFactory();
        }

        private OpenApiSchema GeneratePrimitiveSchema(DataContract dataContract)
        {
            var schema = new OpenApiSchema
            {
                Type = dataContract.DataType.ToString().ToLower(CultureInfo.InvariantCulture),
                Format = dataContract.DataFormat
            };

            if (dataContract.EnumValues != null)
            {
                schema.Enum = dataContract.EnumValues
                    .Distinct()
                    .Select(value => OpenApiAnyFactory.CreateFor(schema, value))
                    .ToList();
            }

            return schema;
        }

        private OpenApiSchema GenerateArraySchema(DataContract dataContract, SchemaRepository schemaRepository, ISerializerDataContractResolver resolver)
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = GenerateSchema(dataContract.ArrayItemType, schemaRepository, resolver),
                UniqueItems = dataContract.UnderlyingType.IsSet() ? (bool?)true : null
            };
        }

        private OpenApiSchema GenerateDictionarySchema(DataContract dataContract, SchemaRepository schemaRepository, ISerializerDataContractResolver resolver)
        {
            if (dataContract.DictionaryKeys != null)
            {
                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = dataContract.DictionaryKeys.ToDictionary(
                        name => name,
                        name => GenerateSchema(dataContract.DictionaryValueType, schemaRepository, resolver))
                };
            }
            else
            {
                return new OpenApiSchema
                {
                    Type = "object",
                    AdditionalPropertiesAllowed = true,
                    AdditionalProperties = GenerateSchema(dataContract.DictionaryValueType, schemaRepository, resolver)
                };
            }
        }

        private bool IsBaseTypeWithKnownSubTypes(Type type, out IEnumerable<Type> subTypes)
        {
            subTypes = generatorOptions.SubTypesSelector(type);

            return subTypes.Any();
        }

        private bool IsKnownSubType(Type type, out Type baseType)
        {
            if ((type.BaseType != null) && (type.BaseType != typeof(object) && generatorOptions.SubTypesSelector(type.BaseType).Contains(type)))
            {
                baseType = type.BaseType;
                return true;
            }

            baseType = null;
            return false;
        }

        private OpenApiSchema GeneratePolymorphicSchema(DataContract dataContract, SchemaRepository schemaRepository, IEnumerable<Type> subTypes, ISerializerDataContractResolver resolver)
        {
            var schema = new OpenApiSchema
            {
                OneOf = new List<OpenApiSchema>(),
                Discriminator = TryGetDiscriminatorName(dataContract, out string discriminatorName)
                    ? new OpenApiDiscriminator { PropertyName = discriminatorName }
                    : null
            };

            var subTypesDataContracts = subTypes
                .Select(subType => defaultResolver.GetDataContractForType(subType));

            var allowedDataContracts = dataContract.UnderlyingType.IsAbstract
                ? subTypesDataContracts
                : new[] { dataContract }.Union(subTypesDataContracts);

            foreach (var allowedDataContract in allowedDataContracts)
            {
                var allowedSchema = GenerateReferencedSchema(
                    allowedDataContract.UnderlyingType,
                    schemaRepository,
                    () => GenerateObjectSchema(allowedDataContract, schemaRepository, resolver));

                schema.OneOf.Add(allowedSchema);

                if (schema.Discriminator != null && TryGetDiscriminatorValue(allowedDataContract, out string discriminatorValue))
                {
                    schema.Discriminator.Mapping.Add(discriminatorValue, allowedSchema.Reference.ReferenceV3);
                }
            }

            return schema;
        }

        private bool TryGetDiscriminatorName(DataContract dataContract, out string discriminatorName)
        {
            discriminatorName = (generatorOptions.DiscriminatorNameSelector != null)
                ? generatorOptions.DiscriminatorNameSelector(dataContract.UnderlyingType)
                : dataContract.ObjectTypeNameProperty;

            return (discriminatorName != null);
        }

        private bool TryGetDiscriminatorValue(DataContract dataContract, out string discriminatorValue)
        {
            discriminatorValue = (generatorOptions.DiscriminatorValueSelector != null)
                ? generatorOptions.DiscriminatorValueSelector(dataContract.UnderlyingType)
                : dataContract.ObjectTypeNameValue;

            return (discriminatorValue != null);
        }

        private OpenApiSchema GenerateObjectSchema(DataContract dataContract, SchemaRepository schemaRepository, ISerializerDataContractResolver resolver)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                AdditionalPropertiesAllowed = false
            };

            // By default, all properties will be defined in this schema
            // However, if "Inheritance" behavior is enabled (see below), this set will be reduced to declared properties only
            var applicableDataProperties = dataContract.ObjectProperties;

            if (generatorOptions.UseOneOfForPolymorphism || generatorOptions.UseAllOfForInheritance)
            {
                if (IsBaseTypeWithKnownSubTypes(dataContract.UnderlyingType, out IEnumerable<Type> subTypes))
                {
                    if (generatorOptions.UseAllOfForInheritance)
                    {
                        // Ensure a schema for all known sub types is generated and added to the repository
                        foreach (var subType in subTypes)
                        {
                            var subTypeContract = defaultResolver.GetDataContractForType(subType);
                            GenerateReferencedSchema(subType, schemaRepository, () => GenerateObjectSchema(subTypeContract, schemaRepository, resolver));
                        }
                    }

                    if (generatorOptions.UseOneOfForPolymorphism
                        && TryGetDiscriminatorName(dataContract, out string discriminatorName))
                    {
                        schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string" });
                        schema.Required.Add(discriminatorName);
                    }
                }

                if (IsKnownSubType(dataContract.UnderlyingType, out Type baseType))
                {
                    var baseTypeContract = defaultResolver.GetDataContractForType(baseType);

                    if (generatorOptions.UseAllOfForInheritance)
                    {
                        schema.AllOf = new List<OpenApiSchema>
                        {
                            GenerateReferencedSchema(baseType, schemaRepository, () => GenerateObjectSchema(baseTypeContract, schemaRepository, resolver))
                        };

                        // Reduce the set of properties to be defined in this schema to declared properties only
                        applicableDataProperties = applicableDataProperties
                            .Where(dataProperty => dataProperty.MemberInfo?.DeclaringType == dataContract.UnderlyingType);
                    }

                    if (generatorOptions.UseOneOfForPolymorphism && !generatorOptions.UseAllOfForInheritance
                        && TryGetDiscriminatorName(baseTypeContract, out string discriminatorName))
                    {
                        schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string" });
                        schema.Required.Add(discriminatorName);
                    }
                }
            }

            foreach (var dataProperty in applicableDataProperties)
            {
                var customAttributes = dataProperty.MemberInfo?.GetInlineAndMetadataAttributes() ?? Enumerable.Empty<object>();

                if (generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any())
                    continue;

                schema.Properties[dataProperty.Name] = GeneratePropertySchema(dataProperty, schemaRepository, resolver);

                if (dataProperty.IsRequired || customAttributes.OfType<RequiredAttribute>().Any()
                    && !schema.Required.Contains(dataProperty.Name))
                {
                    schema.Required.Add(dataProperty.Name);
                }
            }

            if (dataContract.ObjectExtensionDataType != null)
            {
                schema.AdditionalPropertiesAllowed = true;
                schema.AdditionalProperties = GenerateSchema(dataContract.ObjectExtensionDataType, schemaRepository, resolver);
            }

            return schema;
        }

        private OpenApiSchema GeneratePropertySchema(DataProperty dataProperty, SchemaRepository schemaRepository, ISerializerDataContractResolver resolver)
        {
            var schema = GenerateSchema(dataProperty.MemberType, schemaRepository, resolver, memberInfo: dataProperty.MemberInfo);

            if (schema.Reference == null)
            {
                schema.Nullable = schema.Nullable && dataProperty.IsNullable;
                schema.ReadOnly = dataProperty.IsReadOnly;
                schema.WriteOnly = dataProperty.IsWriteOnly;
            }

            return schema;
        }

        private OpenApiSchema GenerateReferencedSchema(
            Type type,
            SchemaRepository schemaRepository,
            Func<OpenApiSchema> definitionFactory)
        {
            if (schemaRepository.TryLookupByType(type, out OpenApiSchema referenceSchema))
                return referenceSchema;

            var schemaId = generatorOptions.SchemaIdSelector(type);

            schemaRepository.RegisterType(type, schemaId);

            var schema = definitionFactory();
            ApplyFilters(schema, type, schemaRepository);

            return schemaRepository.AddDefinition(schemaId, schema);
        }

        private void ApplyMemberMetadata(OpenApiSchema schema, Type type, MemberInfo memberInfo)
        {
            if (schema.Reference != null && generatorOptions.UseAllOfToExtendReferenceSchemas)
            {
                schema.AllOf = new[] { new OpenApiSchema { Reference = schema.Reference } };
                schema.Reference = null;
            }

            if (schema.Reference == null)
            {
                schema.Nullable = type.IsReferenceOrNullableType();

                schema.ApplyCustomAttributes(memberInfo.GetInlineAndMetadataAttributes());
            }
        }

        private void ApplyParameterMetadata(OpenApiSchema schema, Type type, ParameterInfo parameterInfo)
        {
            if (schema.Reference != null && generatorOptions.UseAllOfToExtendReferenceSchemas)
            {
                schema.AllOf = new[] { new OpenApiSchema { Reference = schema.Reference } };
                schema.Reference = null;
            }

            if (schema.Reference == null)
            {
                schema.Nullable = type.IsReferenceOrNullableType();

                schema.ApplyCustomAttributes(parameterInfo.GetCustomAttributes());

                if (parameterInfo.HasDefaultValue)
                {
                    schema.Default = OpenApiAnyFactory.CreateFor(schema, parameterInfo.DefaultValue);
                }
            }
        }

        private void ApplyFilters(
            OpenApiSchema schema,
            Type type,
            SchemaRepository schemaRepository,
            MemberInfo memberInfo = null,
            ParameterInfo parameterInfo = null)
        {
            var filterContext = new SchemaFilterContext(
                type: type,
                schemaGenerator: this,
                schemaRepository: schemaRepository,
                memberInfo: memberInfo,
                parameterInfo: parameterInfo);

            foreach (var filter in generatorOptions.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }
        }
    }
}