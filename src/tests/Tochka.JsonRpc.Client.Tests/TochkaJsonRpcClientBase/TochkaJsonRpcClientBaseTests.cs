using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Client.Tochka;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Tests.TochkaJsonRpcClientBase;

[TestFixture]
public class TochkaJsonRpcClientBaseTests
{
    private HttpClient httpClient;
    private Mock<IJsonRpcSerializer> serializerMock;
    private Mock<IOptions<TochkaTestClientOptions>> optionsMock;
    private Mock<IJsonRpcIdGenerator> generatorMock;
    private Mock<ILogger<TochkaTestClient>> loggerMock;

    [SetUp]
    public void SetUp()
    {
        var handlerMock = new MockHttpMessageHandler();
        httpClient = handlerMock.ToHttpClient();
        serializerMock = new Mock<IJsonRpcSerializer>();
        optionsMock = new Mock<IOptions<TochkaTestClientOptions>>();
        generatorMock = new Mock<IJsonRpcIdGenerator>();
        loggerMock = new Mock<ILogger<TochkaTestClient>>();
    }

    [Test]
    public void TochkaJsonRpcClientBase_AuthenticationKeyIsNull_ThrowsArgumentException()
    {
        optionsMock.SetupGet(o => o.Value).Returns(new TochkaTestClientOptions
        {
            Url = "https://foo.bar",
            AuthenticationKey = null
        });

        var act = () => { new TochkaTestClient(httpClient, serializerMock.Object, new HeaderJsonRpcSerializer(), optionsMock.Object, generatorMock.Object, loggerMock.Object); };
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void TochkaJsonRpcClientBase_AuthenticationKeyIsNotNull_AddsKeyToDefaultAuthenticationHeader()
    {
        var expectedKey = "expected-key";

        optionsMock.SetupGet(o => o.Value).Returns(new TochkaTestClientOptions
        {
            Url = "https://foo.bar",
            AuthenticationKey = expectedKey
        });

        new TochkaTestClient(httpClient, serializerMock.Object, new HeaderJsonRpcSerializer(), optionsMock.Object, generatorMock.Object, loggerMock.Object);

        var values = httpClient.DefaultRequestHeaders.GetValues(TochkaConstants.DefaultAuthenticationHeader);
        values.Single().Should().Be(expectedKey);
    }

    [Test]
    public void TochkaJsonRpcClientBase_AuthenticationHeaderIsOverridden_AddKeyToOverriddenHeader()
    {
        var expectedHeader = "my-custom-header";

        optionsMock.SetupGet(o => o.Value).Returns(new TochkaTestClientOptions
        {
            Url = "https://foo.bar",
            AuthenticationKey = "some-key",
            AuthenticationHeader = expectedHeader
        });

        new TochkaTestClient(httpClient, serializerMock.Object, new HeaderJsonRpcSerializer(), optionsMock.Object, generatorMock.Object, loggerMock.Object);

        var values = httpClient.DefaultRequestHeaders.GetValues(expectedHeader);
        values.Single().Should().NotBeNull();
    }
}
