using Tochka.JsonRpc.Server.Serialization;

namespace Tochka.JsonRpc.ApiExplorer;

public static class Utils
{
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
