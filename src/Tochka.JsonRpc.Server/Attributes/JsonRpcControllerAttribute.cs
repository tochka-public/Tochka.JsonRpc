using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Attributes;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class)]
public sealed class JsonRpcControllerAttribute : Attribute
{
}
