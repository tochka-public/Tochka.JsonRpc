using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.TestUtils.Integration;
using Tochka.JsonRpc.V1.Common.Models.Response.Errors;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.Server;
using Tochka.JsonRpc.V1.Server.Attributes;
using Tochka.JsonRpc.V1.Server.Pipeline;
using Tochka.JsonRpc.V1.Server.Services;
using Tochka.JsonRpc.V1.Server.Settings;

namespace Tochka.JsonRpc.Client.Tests.WebApplication.Controllers;

public class SimpleJsonRpcController : JsonRpcController
{
    private readonly IResponseProvider responseProvider;
    private readonly IRequestValidator requestValidator;
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;

    public SimpleJsonRpcController(IResponseProvider responseProvider, IRequestValidator requestValidator, IJsonRpcErrorFactory jsonRpcErrorFactory)
    {
        this.responseProvider = responseProvider;
        this.requestValidator = requestValidator;
        this.jsonRpcErrorFactory = jsonRpcErrorFactory;
    }

    public TestData ProcessAnything([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    [JsonRpcMethodStyle(MethodStyle.ActionOnly)]
    public TestData ActionOnly([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    [JsonRpcMethodStyle(MethodStyle.ControllerAndAction)]
    public TestData ControllerAndAction([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    public TestData BindingStyleDefault(TestData data) => Process(data);

    public TestData BindingStyleObject([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    public TestData BindingStyleArray([FromParams(BindingStyle.Array)] List<TestData> data) => Process(data.First());

    public TestData NoParams() => responseProvider.GetJsonRpcResponse();

    public TestData NullParams([FromParams(BindingStyle.Object)] object? data)
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }

    public TestData DefaultParams(string? data = "123")
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }

    [JsonRpcSerializer(typeof(SnakeCaseJsonRpcSerializer))]
    public TestData SnakeCaseParams([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    [JsonRpcSerializer(typeof(CamelCaseJsonRpcSerializer))]
    public TestData CamelCaseParams([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    [Route("/custom/action")]
    public TestData CustomActionRoute([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    public TestData ThrowException() => throw new ArgumentException();

    public IError ReturnErrorFromFactory() => jsonRpcErrorFactory.InvalidParams("errorMessage");

    public ActionResult ReturnMvcError() => BadRequest("errorMessage");

    public TestData ErrorThrowAsResponseException()
    {
        jsonRpcErrorFactory.InvalidParams("errorMessage").ThrowAsResponseException();
        return responseProvider.GetJsonRpcResponse();
    }

    private TestData Process(TestData data)
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }
}
