using Json.Schema;

namespace Tochka.JsonRpc.OpenRpc.Services;

internal static class JsonSchemaBuilderExtensions
{
    public static JsonSchemaBuilder TryAppendTitle(this JsonSchemaBuilder builder, string? summary)
    {
        if (summary is { Length: > 0 })
        {
            var newLineIndex = summary.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (newLineIndex >= 0)
            {
                builder.Title(summary[..newLineIndex] + "...")
                       .Description(summary);
            }
            else
            {
                builder.Title(summary);
            }
        }
        return builder;
    }
}
