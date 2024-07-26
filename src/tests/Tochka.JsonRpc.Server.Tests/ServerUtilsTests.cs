using System;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests;

[TestFixture]
public class ServerUtilsTests
{
    [Test]
    public void GetDataJsonSerializerOptions_NoCustomSerializer_ReturnDefaultDataJsonSerializerOptions()
    {
        var metadata = Array.Empty<object>();
        var options = new JsonRpcServerOptions();
        var serializerOptionsProviders = Array.Empty<IJsonSerializerOptionsProvider>();

        var result = ServerUtils.GetDataJsonSerializerOptions(metadata, options, serializerOptionsProviders);

        result.Should().Be(options.DefaultDataJsonSerializerOptions);
    }

    [Test]
    public void GetDataJsonSerializerOptions_CustomSerializerNotRegistered_Throw()
    {
        var metadata = new object[]
        {
            new JsonRpcSerializerOptionsAttribute(typeof(SnakeCaseJsonSerializerOptionsProvider))
        };
        var options = new JsonRpcServerOptions();
        var serializerOptionsProviders = Array.Empty<IJsonSerializerOptionsProvider>();

        var action = () => ServerUtils.GetDataJsonSerializerOptions(metadata, options, serializerOptionsProviders);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetDataJsonSerializerOptions_CustomSerializerRegistered_ReturnCustomSerializer()
    {
        var metadata = new object[]
        {
            new JsonRpcSerializerOptionsAttribute(typeof(CamelCaseJsonSerializerOptionsProvider))
        };
        var options = new JsonRpcServerOptions();
        var provider = new CamelCaseJsonSerializerOptionsProvider();
        var serializerOptionsProviders = new[]
        {
            provider
        };

        var result = ServerUtils.GetDataJsonSerializerOptions(metadata, options, serializerOptionsProviders);

        result.Should().Be(provider.Options);
    }

    [Test]
    public void GetJsonSerializerOptions_CustomSerializerNotRegistered_Throw()
    {
        var serializerOptionsProviders = Array.Empty<IJsonSerializerOptionsProvider>();

        var action = () => ServerUtils.GetJsonSerializerOptions(serializerOptionsProviders, typeof(SnakeCaseJsonSerializerOptionsProvider));

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetJsonSerializerOptions_CustomSerializerRegistered_ReturnCustomSerializer()
    {
        var provider = new CamelCaseJsonSerializerOptionsProvider();
        var serializerOptionsProviders = new[]
        {
            provider
        };

        var result = ServerUtils.GetJsonSerializerOptions(serializerOptionsProviders, typeof(CamelCaseJsonSerializerOptionsProvider));

        result.Should().Be(provider.Options);
    }
}
