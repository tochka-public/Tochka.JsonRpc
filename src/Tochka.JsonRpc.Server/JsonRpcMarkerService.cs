namespace Tochka.JsonRpc.Server;

/// <summary>
/// A marker class used to determine if all the JsonRpc services were added
/// to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" /> before MVC is configured.
/// </summary>
internal class JsonRpcMarkerService
{
}
