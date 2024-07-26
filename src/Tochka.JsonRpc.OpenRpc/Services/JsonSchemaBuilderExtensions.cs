using Json.Schema;

namespace Tochka.JsonRpc.OpenRpc.Services;

internal static class JsonSchemaBuilderExtensions
{
    public static JsonSchemaBuilder TryAppendTitle(this JsonSchemaBuilder builder, string? propertySummary)
    {
        if (propertySummary is { Length: > 0 })
        {
            builder.Title(propertySummary);
        }

        return builder;
    }
}