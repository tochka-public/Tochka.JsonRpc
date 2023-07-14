using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.ApiExplorer;

/// <summary>
/// All ApiExplorer constants
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static class ApiExplorerConstants
{
    /// <summary>
    /// Assembly name for models, generated for JSON-RPC requests and responses in autodoc
    /// </summary>
    public const string GeneratedModelsAssemblyName = "JsonRpcGeneratedModelTypes";

    /// <summary>
    /// Default name for autodoc documents, generated for JSON-RPC API
    /// </summary>
    public const string DefaultDocumentName = "jsonrpc";

    /// <summary>
    /// Default title for autodoc documents, generated for JSON-RPC API
    /// </summary>
    public const string DefaultDocumentTitle = "JSON-RPC";

    /// <summary>
    /// Default version for autodoc documents, generated for JSON-RPC API
    /// </summary>
    public const string DefaultDocumentVersion = "v1";

    /// <summary>
    /// Key to use in ApiDescription properties for storing JSON-RPC method name
    /// </summary>
    public const string MethodNameProperty = "methodName";
}
