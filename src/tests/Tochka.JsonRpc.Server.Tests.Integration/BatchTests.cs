using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Tests.WebApplication;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Server.Tests.Integration;

internal class BatchTests : IntegrationTestsBase<Program>
{
    private Mock<IResponseProvider> responseProviderMock;
    private Mock<IRequestValidator> requestValidatorMock;

    public override void Setup()
    {
        base.Setup();
        responseProviderMock = new Mock<IResponseProvider>();
        requestValidatorMock = new Mock<IRequestValidator>();
    }

    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services.AddTransient(_ => responseProviderMock.Object);
        services.AddTransient(_ => requestValidatorMock.Object);
    }

    [Test]
    public async Task StringId_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task IntId_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": 123,
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "id": 456,
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": 123,
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": 456,
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task FloatId_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
                                     [
                                         {
                                             "method": "process_anything",
                                             "id": 123.23,
                                             "params": {{TestData.PlainRequiredSnakeCaseJson}},
                                             "jsonrpc": "2.0"
                                         },
                                         {
                                             "method": "process_anything",
                                             "id": 456.23,
                                             "params": {{TestData.PlainRequiredSnakeCaseJson}},
                                             "jsonrpc": "2.0"
                                         }
                                     ]
                                     """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
                                     [
                                         {
                                             "id": 123.23,
                                             "result": {{TestData.PlainFullSnakeCaseJson}},
                                             "jsonrpc": "2.0"
                                         },
                                         {
                                             "id": 456.23,
                                             "result": {{TestData.PlainFullSnakeCaseJson}},
                                             "jsonrpc": "2.0"
                                         }
                                     ]
                                     """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullId_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": null,
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": null,
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task ActionOnly_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "action_only",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "action_only",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task ControllerAndAction_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "simple_json_rpc.controller_and_action",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "simple_json_rpc.controller_and_action",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleDefault_ObjectParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "binding_style_default",
                    "id": "123",
                    "params": {
                        "data": {{TestData.PlainRequiredSnakeCaseJson}}
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_default",
                    "id": "456",
                    "params": {
                        "data": {{TestData.PlainRequiredSnakeCaseJson}}
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleDefault_ArrayParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "binding_style_default",
                    "id": "123",
                    "params": [
                        {{TestData.PlainRequiredSnakeCaseJson}}
                    ],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_default",
                    "id": "456",
                    "params": [
                        {{TestData.PlainRequiredSnakeCaseJson}}
                    ],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleDefault_NullParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_default",
                    "id": "123",
                    "params": null,
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_default",
                    "id": "456",
                    "params": null,
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from null json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from null json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleDefault_WithoutParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_default",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_default",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from missing json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from missing json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleDefault_EmptyObjectParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_default",
                    "id": "123",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_default",
                    "id": "456",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params.data])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params.data])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleDefault_EmptyArrayParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_default",
                    "id": "123",
                    "params": [],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_default",
                    "id": "456",
                    "params": [],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params[0]])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params[0]])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleObject_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "binding_style_object",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_object",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleObject_NullParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_object",
                    "id": "123",
                    "params": null,
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_object",
                    "id": "456",
                    "params": null,
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Can't bind value = [null] by JSON key = [params] to required parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Can't bind value = [null] by JSON key = [params] to required parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleObject_WithoutParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_object",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_object",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleObject_EmptyObjectParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_object",
                    "id": "123",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_object",
                    "id": "456",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "ArrayField": [
                                "The ArrayField field is required."
                            ],
                            "StringField": [
                                "The StringField field is required."
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "ArrayField": [
                                "The ArrayField field is required."
                            ],
                            "StringField": [
                                "The StringField field is required."
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleObject_EmptyArrayParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_object",
                    "id": "123",
                    "params": [],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_object",
                    "id": "456",
                    "params": [],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind array to object parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind array to object parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleArray_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "binding_style_array",
                    "id": "123",
                    "params": [
                        {{TestData.PlainRequiredSnakeCaseJson}}
                    ],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_array",
                    "id": "456",
                    "params": [
                        {{TestData.PlainRequiredSnakeCaseJson}}
                    ],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleArray_NullParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_array",
                    "id": "123",
                    "params": null,
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_array",
                    "id": "456",
                    "params": null,
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Can't bind value = [null] by JSON key = [params] to required parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Can't bind value = [null] by JSON key = [params] to required parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleArray_WithoutParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_array",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_array",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleArray_EmptyObjectParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_array",
                    "id": "123",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_array",
                    "id": "456",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind object to collection parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind object to collection parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task BindingStyleArray_EmptyArrayParams_SetEmptyArray()
    {
        const string requestJson = """
            [
                {
                    "method": "binding_style_array",
                    "id": "123",
                    "params": [],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_array",
                    "id": "456",
                    "params": [],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = new List<TestData>();
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableDefaultParams_NullParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_default_params",
                    "id": "123",
                    "params": null,
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_default_params",
                    "id": "456",
                    "params": null,
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from null json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from null json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableDefaultParams_WithoutParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_default_params",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_default_params",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from missing json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from missing json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableDefaultParams_EmptyObjectParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_default_params",
                    "id": "123",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_default_params",
                    "id": "456",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params.data])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params.data])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableDefaultParams_EmptyArrayParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_default_params",
                    "id": "123",
                    "params": [],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_default_params",
                    "id": "456",
                    "params": [],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params[0]])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params[0]])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableObjectParams_NullParams_SetNull()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_object_params",
                    "id": "123",
                    "params": null,
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_object_params",
                    "id": "456",
                    "params": null,
                    "jsonrpc": "2.0"
                }
            ]
            """;
        object? expectedRequestData = null;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableObjectParams_WithoutParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_object_params",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_object_params",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableObjectParams_EmptyObjectParams_SetEmptyObject()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_object_params",
                    "id": "123",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_object_params",
                    "id": "456",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().NotBeNull();
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableObjectParams_EmptyArrayParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_object_params",
                    "id": "123",
                    "params": [],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_object_params",
                    "id": "456",
                    "params": [],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind array to object parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind array to object parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableArrayParams_NullParams_SetNull()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_array_params",
                    "id": "123",
                    "params": null,
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_array_params",
                    "id": "456",
                    "params": null,
                    "jsonrpc": "2.0"
                }
            ]
            """;
        object? expectedRequestData = null;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableArrayParams_WithoutParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_array_params",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_array_params",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [params])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableArrayParams_EmptyObjectParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_array_params",
                    "id": "123",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_array_params",
                    "id": "456",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind object to collection parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind object to collection parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NullableArrayParams_EmptyArrayParams_SetEmptyArray()
    {
        const string requestJson = """
            [
                {
                    "method": "nullable_array_params",
                    "id": "123",
                    "params": [],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "nullable_array_params",
                    "id": "456",
                    "params": [],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        object? expectedRequestData = new List<object?>();
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultParams_NullParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "default_params",
                    "id": "123",
                    "params": null,
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_params",
                    "id": "456",
                    "params": null,
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from null json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from null json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultParams_WithoutParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "default_params",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_params",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from missing json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind method arguments from missing json params"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultParams_EmptyObjectParams_SetDefault()
    {
        const string requestJson = """
            [
                {
                    "method": "default_params",
                    "id": "123",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_params",
                    "id": "456",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        object? expectedRequestData = "123";
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultParams_EmptyArrayParams_SetDefault()
    {
        const string requestJson = """
            [
                {
                    "method": "default_params",
                    "id": "123",
                    "params": [],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_params",
                    "id": "456",
                    "params": [],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        object? expectedRequestData = "123";
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultObjectParams_NullParams_SetNull()
    {
        const string requestJson = """
            [
                {
                    "method": "default_object_params",
                    "id": "123",
                    "params": null,
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_object_params",
                    "id": "456",
                    "params": null,
                    "jsonrpc": "2.0"
                }
            ]
            """;
        object? expectedRequestData = null;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultObjectParams_WithoutParams_SetDefault()
    {
        const string requestJson = """
            [
                {
                    "method": "default_object_params",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_object_params",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        object? expectedRequestData = "123";
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultObjectParams_EmptyObjectParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "default_object_params",
                    "id": "123",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_object_params",
                    "id": "456",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value = [{}] (JSON key = [params]) - JsonException: The JSON value could not be converted to System.String. Path: $ | LineNumber: 0 | BytePositionInLine: 1."
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value = [{}] (JSON key = [params]) - JsonException: The JSON value could not be converted to System.String. Path: $ | LineNumber: 0 | BytePositionInLine: 1."
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultObjectParams_EmptyArrayParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "default_object_params",
                    "id": "123",
                    "params": [],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_object_params",
                    "id": "456",
                    "params": [],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind array to object parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind array to object parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultArrayParams_NullParams_SetNull()
    {
        const string requestJson = """
            [
                {
                    "method": "default_array_params",
                    "id": "123",
                    "params": null,
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_array_params",
                    "id": "456",
                    "params": null,
                    "jsonrpc": "2.0"
                }
            ]
            """;
        object? expectedRequestData = null;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultArrayParams_WithoutParams_SetDefault()
    {
        const string requestJson = """
            [
                {
                    "method": "default_array_params",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_array_params",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        object? expectedRequestData = null;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultArrayParams_EmptyObjectParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "default_array_params",
                    "id": "123",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_array_params",
                    "id": "456",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind object to collection parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Error while binding value by JSON key = [params] - Can't bind object to collection parameter"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task DefaultArrayParams_EmptyArrayParams_SetEmptyArray()
    {
        const string requestJson = """
            [
                {
                    "method": "default_array_params",
                    "id": "123",
                    "params": [],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "default_array_params",
                    "id": "456",
                    "params": [],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        object? expectedRequestData = new List<object?>();
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NoParams_ProcessSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "no_params",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "no_params",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task SnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "snake_case_params",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "snake_case_params",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task CamelCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "camelCaseParams",
                    "id": "123",
                    "params": {{TestData.PlainRequiredCamelCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "camelCaseParams",
                    "id": "456",
                    "params": {{TestData.PlainRequiredCamelCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullCamelCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullCamelCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NestedSnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "snake_case_params",
                    "id": "123",
                    "params": {{TestData.NestedRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "snake_case_params",
                    "id": "456",
                    "params": {{TestData.NestedRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Nested;
        var responseData = TestData.Nested;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.NestedFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.NestedFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NestedCamelCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "camelCaseParams",
                    "id": "123",
                    "params": {{TestData.NestedRequiredCamelCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "camelCaseParams",
                    "id": "456",
                    "params": {{TestData.NestedRequiredCamelCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Nested;
        var responseData = TestData.Nested;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.NestedFullCamelCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.NestedFullCamelCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task AdditionalParams_DeserializeSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {
                        "bool_field": true,
                        "string_field": "123",
                        "int_field": 123,
                        "double_field": 1.23,
                        "enum_field": "two",
                        "array_field": [
                            1,
                            2,
                            3
                        ],
                        "nullable_field": null,
                        "additional_field": "something"
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "params": {
                        "bool_field": true,
                        "string_field": "123",
                        "int_field": 123,
                        "double_field": 1.23,
                        "enum_field": "two",
                        "array_field": [
                            1,
                            2,
                            3
                        ],
                        "nullable_field": null,
                        "additional_field": "something"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task CustomActionRoute_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "custom_action_route",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "custom_action_route",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync($"{JsonRpcUrl}/custom/action", request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task CustomControllerRoute_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "custom_controller_route",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "custom_controller_route",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync($"{JsonRpcUrl}/custom/controller", request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task RequestAndNotification_ReturnOnlyRequestResponse()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task ThrowException_SerializeSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "throw_exception",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "throw_exception",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32000,
                        "message": "Server error",
                        "data": {
                            "type": "System.ArgumentException",
                            "message": "Value does not fall within the expected range.",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32000,
                        "message": "Server error",
                        "data": {
                            "type": "System.ArgumentException",
                            "message": "Value does not fall within the expected range.",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task ReturnErrorFromFactory_SerializeSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "return_error_from_factory",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "return_error_from_factory",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task ReturnMvcError_SerializeSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "return_mvc_error",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "return_mvc_error",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task ErrorThrowAsResponseException_SerializeSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "error_throw_as_response_exception",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "error_throw_as_response_exception",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task UnknownMethod_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "some_not_existing_method",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "some_not_existing_method",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32601,
                        "message": "Method not found",
                        "data": {
                            "method": "some_not_existing_method"
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32601,
                        "message": "Method not found",
                        "data": {
                            "method": "some_not_existing_method"
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NoMethod_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": null,
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "JSON Rpc call does not have [method] property",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": null,
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "JSON Rpc call does not have [method] property",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task MethodNull_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": null,
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": null,
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task MethodEmpty_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task MethodWhiteSpace_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": " ",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": " ",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task InvalidJsonRpcVersion_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "3.0"
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "3.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Only [2.0] version supported. Got [3.0]",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Only [2.0] version supported. Got [3.0]",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task InvalidJson_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    123
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    123
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            {
                "id": null,
                "error": {
                    "code": -32700,
                    "message": "Parse error",
                    "data": {
                        "type": "System.Text.Json.JsonException",
                        "message": "'1' is an invalid start of a property name. Expected a '\"'. Path: $ | LineNumber: 2 | BytePositionInLine: 8.",
                        "details": null
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task NoJsonRpc_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}}
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}}
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": null,
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "JSON Rpc call does not have [jsonrpc] property",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": null,
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "JSON Rpc call does not have [jsonrpc] property",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    private const string JsonRpcUrl = "/api/jsonrpc";
}
