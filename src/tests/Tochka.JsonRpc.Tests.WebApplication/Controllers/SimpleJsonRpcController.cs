using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

public class SimpleJsonRpcController : JsonRpcControllerBase
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

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
    /// <summary>
    /// some description
    /// </summary>
    /// <param name="data">param description</param>
    /// <returns>returns description</returns>
    public async Task<TestData> AutoDocExperiments([FromParams(BindingStyle.Object)] RequestData<bool> data, int intField, string defaultBinding, [FromQuery] string? fromQuery, [FromBody] string? fromBody, [FromServices] IResponseProvider rp, CancellationToken token)
    {
        await Task.Delay(1, token);
        return rp.GetJsonRpcResponse();
    }
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

    public TestData ProcessAnything([FromParams(BindingStyle.Object)] TestData data) =>
        Process(data);

    [JsonRpcMethodStyle(JsonRpcMethodStyle.ActionOnly)]
    public TestData ActionOnly([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    [JsonRpcMethodStyle(JsonRpcMethodStyle.ControllerAndAction)]
    public TestData ControllerAndAction([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    public TestData BindingStyleDefault(TestData data) => Process(data);

    public TestData BindingStyleObject([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    public TestData BindingStyleArray([FromParams(BindingStyle.Array)] List<TestData> data)
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }

    public TestData NullableDefaultParams(object? data)
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }

    public TestData NullableObjectParams([FromParams(BindingStyle.Object)] object? data)
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }

    public TestData NullableArrayParams([FromParams(BindingStyle.Array)] List<object?>? data)
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }

    public TestData DefaultParams(string? data = "123")
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }

    public TestData DefaultObjectParams([FromParams(BindingStyle.Object)] string? data = "123")
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }

    public TestData DefaultArrayParams([FromParams(BindingStyle.Array)] List<string?>? data = null)
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }

    public TestData NoParams() => responseProvider.GetJsonRpcResponse();

    [JsonRpcSerializerOptions(typeof(SnakeCaseJsonSerializerOptionsProvider))]
    public TestData SnakeCaseParams([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    [JsonRpcSerializerOptions(typeof(CamelCaseJsonSerializerOptionsProvider))]
    public TestData CamelCaseParams([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    [JsonRpcSerializerOptions(typeof(KebabCaseUpperJsonSerializerOptionsProvider))]
    public TestData KebabCaseUpperCaseParams([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    [Route("/custom/action")]
    public TestData CustomActionRoute([FromParams(BindingStyle.Object)] TestData data) => Process(data);

    public TestData ThrowException() => throw new ArgumentException();

    public IError ReturnErrorFromFactory() => jsonRpcErrorFactory.InvalidParams("errorMessage");

    public ActionResult ReturnMvcError() => BadRequest("errorMessage");

    public TestData ErrorThrowAsResponseException()
    {
        jsonRpcErrorFactory.InvalidParams("errorMessage").ThrowAsException();
        return responseProvider.GetJsonRpcResponse();
    }

    public void VoidMethod()
    {
    }

    public Task TaskMethod() => Task.CompletedTask;

    public IActionResult EmptyOk() => Ok();

    private TestData Process(TestData data)
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }
}
