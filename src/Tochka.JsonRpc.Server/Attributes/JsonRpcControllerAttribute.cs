using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Server.Attributes;

/// <inheritdoc />
/// <summary>
/// Attribute to mark controller as JSON-RPC controller
/// </summary>
/// <remarks>
/// You don't need to add this attribute to your JSON-RPC controllers manually,
/// inheriting from <see cref="JsonRpcControllerBase" /> is enough
/// </remarks>
[PublicAPI]
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class)]
public sealed class JsonRpcControllerAttribute : Attribute
{
}
