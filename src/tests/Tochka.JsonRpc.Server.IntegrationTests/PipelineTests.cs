using System;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.IntegrationTests.Cases;
using Tochka.JsonRpc.Server.IntegrationTests.Controllers;
using Tochka.JsonRpc.Server.IntegrationTests.Models;
using Tochka.JsonRpc.Server.Models.Binding;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.IntegrationTests
{
    public class PipelineTests
    {
        private WebApplicationFactory<Startup> factory;
        private HttpClient client;
        private readonly IJsonRpcSerializer requestSerializer = new SnakeCaseJsonRpcSerializer();
        private readonly IJsonRpcSerializer headerSerializer = new HeaderJsonRpcSerializer();

        // TODO make it one-time, but preserve logs of LoggingHandler! They are ignored if this is one-time setup
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var testEnvironemt = new TestEnvironment(services =>
            {
            });
            factory = new TestAppFactory();
            client = factory.CreateDefaultClient(new LoggingHandler());
        }

        [TestCaseSource(typeof(NoParamsRequests), nameof(NoParamsRequests.Cases))]
        [TestCaseSource(typeof(ParamsRequests), nameof(ParamsRequests.Cases))]
        [TestCaseSource(typeof(NoParamsNotifications), nameof(NoParamsNotifications.Cases))]
        [TestCaseSource(typeof(HttpCodeRequests), nameof(HttpCodeRequests.Cases))]
        [TestCaseSource(typeof(HttpObjectCodeRequests), nameof(HttpObjectCodeRequests.Cases))]
        [TestCaseSource(typeof(TestDataRequests), nameof(TestDataRequests.Cases))]
        public async Task Test_JsonRpc_SingleRequestResponse(object data, object expected)
        {
            var request = CreateRequest(data);
            var response = await client.SendAsync(request);
            response.StatusCode.Should().Be(200);
            var result = await ReadResponse(response, expected?.GetType());
            result.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.SelectedMemberInfo.MemberType == typeof(IgnoreData)));
        }

        [TestCaseSource(typeof(BadRequests), nameof(BadRequests.Cases))]
        public async Task Test_JsonRpc_SingleStringRequestResponse(string data, object expected)
        {
            var request = CreateStringRequest(data);
            var response = await client.SendAsync(request);
            response.StatusCode.Should().Be(200);
            var result = await ReadResponse(response, expected?.GetType());
            result.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.SelectedMemberInfo.MemberType == typeof(IgnoreData)));
        }

        private HttpRequestMessage CreateRequest(object data)
        {
            var json = JsonConvert.SerializeObject(data, requestSerializer.Settings);
            var content = new StringContent(json);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(JsonRpcConstants.ContentType);
            return new HttpRequestMessage(HttpMethod.Post, "/api/jsonrpc") { Content = content };
        }

        private HttpRequestMessage CreateStringRequest(string data)
        {
            var content = new StringContent(data);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(JsonRpcConstants.ContentType);
            return new HttpRequestMessage(HttpMethod.Post, "/api/jsonrpc") { Content = content };
        }

        private async Task<object> ReadResponse(HttpResponseMessage response, Type type)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            if (type == typeof(string))
            {
                return jsonString;
            }

            var json = JToken.Parse(jsonString);
            var isErrorType = type.GetInterfaces().Any(x => x == typeof(IErrorResponse));
            if (json is JObject jObject && jObject.ContainsKey("error") && !isErrorType)
            {
                Assert.Fail($"Response is error, expected type is NOT error: {type.Name}");
            }

            return JsonConvert.DeserializeObject(jsonString, type, headerSerializer.Settings);
        }
    }
}
