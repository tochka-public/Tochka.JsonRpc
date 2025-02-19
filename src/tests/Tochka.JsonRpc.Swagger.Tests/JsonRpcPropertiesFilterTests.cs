using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Moq;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Swagger.Tests;

[TestFixture]
public class JsonRpcPropertiesFilterTests
{
    private JsonRpcPropertiesFilter propertiesFilter;

    [SetUp]
    public void Setup() => propertiesFilter = new JsonRpcPropertiesFilter();

    [Test]
    public void Apply_Id_PatchIdSchema()
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(IRpcId), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Nullable.Should().BeTrue();
        schema.AdditionalPropertiesAllowed.Should().BeFalse();
        schema.Example.Should().BeEquivalentTo(new OpenApiString("1"));
        schema.OneOf.Should().BeEquivalentTo(new List<OpenApiSchema> { new() { Type = "string" }, new() { Type = "number" } });
    }

    [Test]
    public void Apply_CallWithoutId_DontPatchIdProperty()
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().NotContain(static p => p.Key == JsonRpcConstants.IdProperty);
    }

    [Test]
    public void Apply_CallWithCorrectIdName_LeaveIdProperty()
    {
        var idSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.IdProperty] = idSchema
            }
        };
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.IdProperty && p.Value == idSchema);
    }

    [Test]
    public void Apply_CallWithBrokenIdName_FixIdPropertyName()
    {
        var idSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.IdProperty.ToUpperInvariant()] = idSchema
            }
        };
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.IdProperty && p.Value == idSchema);
    }

    [Test]
    public void Apply_CallWithoutMethod_DontPatchMethodProperty()
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().NotContain(static p => p.Key == JsonRpcConstants.MethodProperty);
    }

    [Test]
    public void Apply_CallWithCorrectMethodName_PatchMethodProperty()
    {
        var methodSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.MethodProperty] = methodSchema
            }
        };
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.MethodProperty && p.Value == methodSchema);
        methodSchema.Nullable.Should().BeFalse();
    }

    [Test]
    public void Apply_CallWithCorrectMethodNameAndMetadata_PatchMethodPropertyAndAddExample()
    {
        var methodSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.MethodProperty] = methodSchema
            }
        };
        var context = new SchemaFilterContext(typeof(CallWithMetadata), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.MethodProperty && p.Value == methodSchema);
        methodSchema.Nullable.Should().BeFalse();
        methodSchema.Example.Should().BeEquivalentTo(new OpenApiString(MethodName));
    }

    [Test]
    public void Apply_CallWithBrokenMethodName_FixMethodPropertyName()
    {
        var methodSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.MethodProperty.ToUpperInvariant()] = methodSchema
            }
        };
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.MethodProperty && p.Value == methodSchema);
        methodSchema.Nullable.Should().BeFalse();
    }

    [Test]
    public void Apply_CallWithoutVersion_DontPatchVersionProperty()
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().NotContain(static p => p.Key == JsonRpcConstants.JsonrpcVersionProperty);
    }

    [Test]
    public void Apply_CallWithCorrectVersionName_PatchVersionProperty()
    {
        var versionSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.JsonrpcVersionProperty] = versionSchema
            }
        };
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.JsonrpcVersionProperty && p.Value == versionSchema);
        versionSchema.Nullable.Should().BeFalse();
        versionSchema.Example.Should().BeEquivalentTo(new OpenApiString(JsonRpcConstants.Version));
    }

    [Test]
    public void Apply_CallWithBrokenVersionName_FixVersionPropertyName()
    {
        var versionSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.JsonrpcVersionProperty.ToUpperInvariant()] = versionSchema
            }
        };
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.JsonrpcVersionProperty && p.Value == versionSchema);
        versionSchema.Nullable.Should().BeFalse();
        versionSchema.Example.Should().BeEquivalentTo(new OpenApiString(JsonRpcConstants.Version));
    }

    [Test]
    public void Apply_CallWithoutParams_DontPatchParamsProperty()
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().NotContain(static p => p.Key == JsonRpcConstants.ParamsProperty);
    }

    [Test]
    public void Apply_CallWithCorrectParamsName_LeaveParamsProperty()
    {
        var paramsSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.ParamsProperty] = paramsSchema
            }
        };
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.ParamsProperty && p.Value == paramsSchema);
    }

    [Test]
    public void Apply_CallWithBrokenParamsName_FixParamsPropertyName()
    {
        var paramsSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.ParamsProperty.ToUpperInvariant()] = paramsSchema
            }
        };
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.ParamsProperty && p.Value == paramsSchema);
    }

    [Test]
    public void Apply_Call_AddRequiredProperties()
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(ICall), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Required.Should().Contain(JsonRpcConstants.MethodProperty);
        schema.Required.Should().Contain(JsonRpcConstants.JsonrpcVersionProperty);
    }

    [Test]
    public void Apply_ResponseWithoutId_DontPatchIdProperty()
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(IResponse), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().NotContain(static p => p.Key == JsonRpcConstants.IdProperty);
    }

    [Test]
    public void Apply_ResponseWithCorrectIdName_LeaveIdProperty()
    {
        var idSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.IdProperty] = idSchema
            }
        };
        var context = new SchemaFilterContext(typeof(IResponse), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.IdProperty && p.Value == idSchema);
    }

    [Test]
    public void Apply_ResponseWithBrokenIdName_FixIdPropertyName()
    {
        var idSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.IdProperty.ToUpperInvariant()] = idSchema
            }
        };
        var context = new SchemaFilterContext(typeof(IResponse), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.IdProperty && p.Value == idSchema);
    }

    [Test]
    public void Apply_ResponseWithoutVersion_DontPatchVersionProperty()
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(IResponse), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().NotContain(static p => p.Key == JsonRpcConstants.JsonrpcVersionProperty);
    }

    [Test]
    public void Apply_ResponseWithCorrectVersionName_PatchVersionProperty()
    {
        var versionSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.JsonrpcVersionProperty] = versionSchema
            }
        };
        var context = new SchemaFilterContext(typeof(IResponse), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.JsonrpcVersionProperty && p.Value == versionSchema);
        versionSchema.Nullable.Should().BeFalse();
        versionSchema.Example.Should().BeEquivalentTo(new OpenApiString(JsonRpcConstants.Version));
    }

    [Test]
    public void Apply_ResponseWithBrokenVersionName_FixVersionPropertyName()
    {
        var versionSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.JsonrpcVersionProperty.ToUpperInvariant()] = versionSchema
            }
        };
        var context = new SchemaFilterContext(typeof(IResponse), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.JsonrpcVersionProperty && p.Value == versionSchema);
        versionSchema.Nullable.Should().BeFalse();
        versionSchema.Example.Should().BeEquivalentTo(new OpenApiString(JsonRpcConstants.Version));
    }

    [Test]
    public void Apply_ResponseWithoutResult_DontPatchResultProperty()
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(IResponse), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().NotContain(static p => p.Key == JsonRpcConstants.ResultProperty);
    }

    [Test]
    public void Apply_ResponseWithCorrectResultName_LeaveResultProperty()
    {
        var resultSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.ResultProperty] = resultSchema
            }
        };
        var context = new SchemaFilterContext(typeof(IResponse), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.ResultProperty && p.Value == resultSchema);
    }

    [Test]
    public void Apply_ResponseWithBrokenResultName_FixResultPropertyName()
    {
        var resultSchema = new OpenApiSchema();
        var schema = new OpenApiSchema
        {
            Properties =
            {
                [JsonRpcConstants.ResultProperty.ToUpperInvariant()] = resultSchema
            }
        };
        var context = new SchemaFilterContext(typeof(IResponse), Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        propertiesFilter.Apply(schema, context);

        schema.Properties.Should().Contain(p => p.Key == JsonRpcConstants.ResultProperty && p.Value == resultSchema);
    }

    private const string MethodName = "method";

    [JsonRpcTypeMetadata(typeof(object), MethodName)]
    private sealed record CallWithMetadata
    (
        string Jsonrpc,
        string Method
    ) : ICall
    {
        public IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions) => throw new NotImplementedException();
    }
}
