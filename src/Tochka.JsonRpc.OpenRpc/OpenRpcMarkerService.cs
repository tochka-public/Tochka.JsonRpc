using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.OpenRpc;

/// <summary>
/// A marker class used to determine if all the OpenRpc services were added
/// to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" /> before middleware is configured.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenRpcMarkerService
{
}
