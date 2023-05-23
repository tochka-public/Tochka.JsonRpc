namespace Tochka.JsonRpc.Server.DependencyInjection;

/// <summary>
/// A marker class used to determine if all the JsonRpc services were added
/// to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" /> before middleware is configured.
/// </summary>
internal class JsonRpcMarkerService
{
}
