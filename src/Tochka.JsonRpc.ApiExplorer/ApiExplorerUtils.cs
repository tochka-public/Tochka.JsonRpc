using JetBrains.Annotations;
using Tochka.JsonRpc.Server.Serialization;

namespace Tochka.JsonRpc.ApiExplorer;

/// <summary>
/// Helpers for ApiExplorer logic
/// </summary>
[PublicAPI]
public static class ApiExplorerUtils
{
    /// <summary>
    /// Generate document name based on Type name of serializer options provider
    /// </summary>
    public static string GetDocumentName(Type? serializerOptionsProviderType)
    {
        if (serializerOptionsProviderType == null)
        {
            return ApiExplorerConstants.DefaultDocumentName;
        }

        var caseName = serializerOptionsProviderType.Name
            .Replace(nameof(IJsonSerializerOptionsProvider)[1..], "")
            .ToLowerInvariant();
        return $"{ApiExplorerConstants.DefaultDocumentName}_{caseName}";
    }
}
