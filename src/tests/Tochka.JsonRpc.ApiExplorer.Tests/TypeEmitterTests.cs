using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Server.Serialization;

namespace Tochka.JsonRpc.ApiExplorer.Tests;

[TestFixture]
public class TypeEmitterTests
{
    private TypeEmitter typeEmitter;

    [SetUp]
    public void Setup() => typeEmitter = new TypeEmitter(Mock.Of<ILogger<TypeEmitter>>());

    [Test]
    public void CreateRequestType_TypeFullNameHasMethodNameAndActionFullName()
    {
        var baseParamsType = typeof(object);
        var defaultBoundParams = new Dictionary<string, Type>();
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        requestType.FullName.Should().Be($"{MethodName} request ({ActionFullName})");
    }

    [Test]
    public void CreateRequestType_SameActionSecondTime_ReturnTypeAndDontThrow()
    {
        var baseParamsType = typeof(object);
        var defaultBoundParams = new Dictionary<string, Type>();
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType1 = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);
        var requestType2 = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        requestType1.Should().Be(requestType2);
    }

    [Test]
    public void CreateRequestType_TypeInheritedFromRequest()
    {
        var baseParamsType = typeof(object);
        var defaultBoundParams = new Dictionary<string, Type>();
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        requestType.Should().BeAssignableTo<Request<object>>();
    }

    [Test]
    public void CreateRequestType_BaseTypeWithEmptyConstructor_ParamsHasAllPropertiesFromBaseTypeAndDefaultBoundParams()
    {
        var baseParamsType = typeof(ClassWithEmptyConstructor);
        var defaultBoundParams = new Dictionary<string, Type>
        {
            ["C"] = typeof(int),
            ["D"] = typeof(object)
        };
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var paramsType = requestType.GetProperty(nameof(Request<object>.Params)).PropertyType;
        var properties = paramsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var baseTypeProperty in baseParamsType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            properties.Should().Contain(p => p.Name == baseTypeProperty.Name && p.PropertyType == baseTypeProperty.PropertyType);
        }

        foreach (var defaultBoundParam in defaultBoundParams)
        {
            properties.Should().Contain(p => p.Name == defaultBoundParam.Key && p.PropertyType == defaultBoundParam.Value);
        }
    }

    [Test]
    public void CreateRequestType_BaseTypeWithEmptyConstructor_HasMetadataAttribute()
    {
        var baseParamsType = typeof(ClassWithEmptyConstructor);
        var defaultBoundParams = new Dictionary<string, Type>();
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var attributes = requestType.GetCustomAttributes();
        attributes.Should().ContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateRequestType_BaseTypeNotPublic_ParamsTypeIsBaseType()
    {
        var baseParamsType = typeof(NonPublicClass);
        var defaultBoundParams = new Dictionary<string, Type>
        {
            ["C"] = typeof(int),
            ["D"] = typeof(object)
        };
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var paramsType = requestType.GetProperty(nameof(Request<ClassWithoutEmptyConstructor>.Params)).PropertyType;
        paramsType.Should().Be(baseParamsType);
    }

    [Test]
    public void CreateRequestType_BaseTypeNotPublic_NoMetadataAttribute()
    {
        var baseParamsType = typeof(NonPublicClass);
        var defaultBoundParams = new Dictionary<string, Type>();
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var attributes = requestType.GetCustomAttributes();
        attributes.Should().NotContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateRequestType_BaseTypeIsNested_ParamsTypeIsBaseType()
    {
        var baseParamsType = typeof(NestedClass);
        var defaultBoundParams = new Dictionary<string, Type>
        {
            ["C"] = typeof(int),
            ["D"] = typeof(object)
        };
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var paramsType = requestType.GetProperty(nameof(Request<ClassWithoutEmptyConstructor>.Params)).PropertyType;
        paramsType.Should().Be(baseParamsType);
    }

    [Test]
    public void CreateRequestType_BaseTypeIsNested_NoMetadataAttribute()
    {
        var baseParamsType = typeof(NestedClass);
        var defaultBoundParams = new Dictionary<string, Type>();
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var attributes = requestType.GetCustomAttributes();
        attributes.Should().NotContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateRequestType_BaseTypeIsSealed_ParamsTypeIsBaseType()
    {
        var baseParamsType = typeof(SealedClass);
        var defaultBoundParams = new Dictionary<string, Type>
        {
            ["C"] = typeof(int),
            ["D"] = typeof(object)
        };
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var paramsType = requestType.GetProperty(nameof(Request<ClassWithoutEmptyConstructor>.Params)).PropertyType;
        paramsType.Should().Be(baseParamsType);
    }

    [Test]
    public void CreateRequestType_BaseTypeIsSealed_HasMetadataAttribute()
    {
        var baseParamsType = typeof(SealedClass);
        var defaultBoundParams = new Dictionary<string, Type>();
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var attributes = requestType.GetCustomAttributes();
        attributes.Should().ContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateRequestType_BaseTypeIsValueType_ParamsTypeIsObject()
    {
        var baseParamsType = typeof(int);
        var defaultBoundParams = new Dictionary<string, Type>
        {
            ["C"] = typeof(int),
            ["D"] = typeof(object)
        };
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var paramsType = requestType.GetProperty(nameof(Request<ClassWithoutEmptyConstructor>.Params)).PropertyType;
        paramsType.Should().Be<object>();
    }

    [Test]
    public void CreateRequestType_BaseTypeIsValueType_HasMetadataAttribute()
    {
        var baseParamsType = typeof(int);
        var defaultBoundParams = new Dictionary<string, Type>();
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var attributes = requestType.GetCustomAttributes();
        attributes.Should().ContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateRequestType_BaseTypeWithoutEmptyConstructor_ParamsTypeIsBaseType()
    {
        var baseParamsType = typeof(ClassWithoutEmptyConstructor);
        var defaultBoundParams = new Dictionary<string, Type>
        {
            ["C"] = typeof(int),
            ["D"] = typeof(object)
        };
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var paramsType = requestType.GetProperty(nameof(Request<ClassWithoutEmptyConstructor>.Params)).PropertyType;
        paramsType.Should().Be(baseParamsType);
    }

    [Test]
    public void CreateRequestType_BaseTypeWithoutEmptyConstructor_HasMetadataAttribute()
    {
        var baseParamsType = typeof(ClassWithoutEmptyConstructor);
        var defaultBoundParams = new Dictionary<string, Type>();
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var requestType = typeEmitter.CreateRequestType(ActionFullName, MethodName, baseParamsType, defaultBoundParams, serializerOptionsProviderType);

        var attributes = requestType.GetCustomAttributes();
        attributes.Should().ContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateResponseType_TypeFullNameHasMethodNameAndActionFullName()
    {
        var resultType = typeof(object);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        responseType.FullName.Should().Be($"{MethodName} response ({ActionFullName})");
    }

    [Test]
    public void CreateResponseType_SameActionSecondTime_ReturnTypeAndDontThrow()
    {
        var resultType = typeof(object);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType1 = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);
        var responseType2 = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        responseType1.Should().Be(responseType2);
    }

    [Test]
    public void CreateResponseType_TypeInheritedFromResponse()
    {
        var resultType = typeof(object);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        responseType.Should().BeAssignableTo<Response<object>>();
    }

    [Test]
    public void CreateResponseType_ResultTypeWithEmptyConstructor_GenericArgumentIsResultType()
    {
        var resultType = typeof(ClassWithEmptyConstructor);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        responseType.Should().BeAssignableTo<Response<ClassWithEmptyConstructor>>();
    }

    [Test]
    public void CreateResponseType_ResultTypeWithEmptyConstructor_HasMetadataAttribute()
    {
        var resultType = typeof(ClassWithEmptyConstructor);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        var attributes = responseType.GetCustomAttributes();
        attributes.Should().ContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateResponseType_ResultTypeNotPublic_GenericArgumentIsResultType()
    {
        var resultType = typeof(NonPublicClass);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        responseType.Should().BeAssignableTo<Response<NonPublicClass>>();
    }

    [Test]
    public void CreateResponseType_ResultTypeNotPublic_NoMetadataAttribute()
    {
        var resultType = typeof(NonPublicClass);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        var attributes = responseType.GetCustomAttributes();
        attributes.Should().NotContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateResponseType_ResultTypeIsNested_GenericArgumentIsResultType()
    {
        var resultType = typeof(NestedClass);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        responseType.Should().BeAssignableTo<Response<NestedClass>>();
    }

    [Test]
    public void CreateResponseType_ResultTypeIsNested_NoMetadataAttribute()
    {
        var resultType = typeof(NestedClass);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        var attributes = responseType.GetCustomAttributes();
        attributes.Should().NotContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateResponseType_ResultTypeIsSealed_GenericArgumentIsResultType()
    {
        var resultType = typeof(SealedClass);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        responseType.Should().BeAssignableTo<Response<SealedClass>>();
    }

    [Test]
    public void CreateResponseType_ResultTypeIsSealed_HasMetadataAttribute()
    {
        var resultType = typeof(SealedClass);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        var attributes = responseType.GetCustomAttributes();
        attributes.Should().ContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateResponseType_ResultTypeIsValueType_GenericArgumentIsResultType()
    {
        var resultType = typeof(int);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        responseType.Should().BeAssignableTo<Response<int>>();
    }

    [Test]
    public void CreateResponseType_ResultTypeIsValueType_HasMetadataAttribute()
    {
        var resultType = typeof(int);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        var attributes = responseType.GetCustomAttributes();
        attributes.Should().ContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateResponseType_ResultTypeWithoutEmptyConstructor_GenericArgumentIsResultType()
    {
        var resultType = typeof(ClassWithoutEmptyConstructor);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        responseType.Should().BeAssignableTo<Response<ClassWithoutEmptyConstructor>>();
    }

    [Test]
    public void CreateResponseType_ResultTypeWithoutEmptyConstructor_HasMetadataAttribute()
    {
        var resultType = typeof(ClassWithoutEmptyConstructor);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        var attributes = responseType.GetCustomAttributes();
        attributes.Should().ContainEquivalentOf(new JsonRpcTypeMetadataAttribute(serializerOptionsProviderType, MethodName));
    }

    [Test]
    public void CreateResponseType_ResultTypeIsVoid_GenericArgumentIsObject()
    {
        var resultType = typeof(void);
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);

        var responseType = typeEmitter.CreateResponseType(ActionFullName, MethodName, resultType, serializerOptionsProviderType);

        responseType.Should().BeAssignableTo<Response<object>>();
    }

    private const string ActionFullName = "namespace.controller.action";
    private const string MethodName = "method";

    // ReSharper disable once MemberCanBePrivate.Global
    public record NestedClass;
}

// ReSharper disable once MemberCanBeInternal

// ReSharper disable once MemberCanBeInternal

// ReSharper disable once MemberCanBeInternal