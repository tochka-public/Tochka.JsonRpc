using JetBrains.Annotations;

namespace Tochka.JsonRpc.ApiExplorer;

/// <summary>
/// Service to encapsulate logic of types generation
/// </summary>
[PublicAPI]
public interface ITypeEmitter
{
    /// <summary>
    /// Create <see cref="Type" /> for JSON-RPC request object
    /// </summary>
    /// <param name="actionFullName">Full name of C# action</param>
    /// <param name="methodName">JSON-RPC method name</param>
    /// <param name="baseParamsType">Base Type for params object</param>
    /// <param name="defaultBoundParams">Other params with BindingStyle.Default</param>
    /// <param name="serializerOptionsProviderType">Type of custom serializer options provider, configured by attribute</param>
    /// <returns>Created Type</returns>
    Type CreateRequestType(string actionFullName, string methodName, Type baseParamsType, IReadOnlyDictionary<string, Type> defaultBoundParams, Type? serializerOptionsProviderType);

    /// <summary>
    /// Create <see cref="Type" /> for JSON-RPC response object
    /// </summary>
    /// <param name="actionFullName">Full name of C# action</param>
    /// <param name="methodName">JSON-RPC method name</param>
    /// <param name="resultType">Base Type for result object</param>
    /// <param name="serializerOptionsProviderType">Type of custom serializer options provider, configured by attribute</param>
    /// <returns></returns>
    Type CreateResponseType(string actionFullName, string methodName, Type resultType, Type? serializerOptionsProviderType);
}
