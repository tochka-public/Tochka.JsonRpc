using Asp.Versioning.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Tochka.JsonRpc.Server.Attributes;

namespace Tochka.JsonRpc.ApiExplorer;

/// <inheritdoc />
/// <summary>
/// Specification for APi versioning to work with JSON-RPC controllers
/// </summary>
public class JsonRpcControllerSpecification : IApiControllerSpecification
{
    public bool IsSatisfiedBy(ControllerModel controller) =>
        controller.Attributes.Any(static a => a is JsonRpcControllerAttribute);
}
