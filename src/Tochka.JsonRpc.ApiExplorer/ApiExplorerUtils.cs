using Tochka.JsonRpc.Server.Serialization;

namespace Tochka.JsonRpc.ApiExplorer;

/// <summary>
/// Helpers for ApiExplorer logic
/// </summary>
public static class ApiExplorerUtils
{
    /// <summary>
    /// Generate document name based on Type name of serializer options provider
    /// </summary>
    public static string GetDocumentName(string defaultName, Type? serializerOptionsProviderType)
    {
        if (serializerOptionsProviderType == null)
        {
            return defaultName;
        }

        var caseName = serializerOptionsProviderType.Name
            .Replace(nameof(IJsonSerializerOptionsProvider)[1..], "")
            .ToLowerInvariant();
        return $"{defaultName}_{caseName}";
    }
}
