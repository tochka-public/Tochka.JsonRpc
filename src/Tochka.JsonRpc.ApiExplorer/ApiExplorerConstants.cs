using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.ApiExplorer;

/// <summary>
/// All ApiExplorer constants
/// </summary>
[ExcludeFromCodeCoverage]
public static class ApiExplorerConstants
{
    /// <summary>
    /// Assembly name for models, generated for JSON-RPC requests and responses in autodoc
    /// </summary>
    public const string GeneratedModelsAssemblyName = "JsonRpcGen";

    /// <summary>
    /// Key to use in ApiDescription properties for storing JSON-RPC method name
    /// </summary>
    public const string MethodNameProperty = "methodName";
}
