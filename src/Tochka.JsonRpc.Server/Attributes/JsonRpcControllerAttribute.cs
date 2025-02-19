using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Attributes;

/// <inheritdoc />
/// <summary>
/// Attribute to mark controller as JSON-RPC controller
/// </summary>
/// <remarks>
/// You don't need to add this attribute to your JSON-RPC controllers manually,
/// inheriting from <see cref="JsonRpcControllerBase" /> is enough
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class)]
public sealed class JsonRpcControllerAttribute : Attribute
{
}
