using Json.Schema;
using Tochka.JsonRpc.OpenRpc.Models;

namespace Tochka.JsonRpc.OpenRpc.Services;

internal static class JsonSchemaBuilderExtensions
{
    public static JsonSchemaBuilder AppendXmlDocs(this JsonSchemaBuilder builder, XmlDocValuesWrapper xmlDocs)
    {
        var documentation = xmlDocs.Summary;

        if (!string.IsNullOrEmpty(xmlDocs.Remarks))
        {
            if (!string.IsNullOrEmpty(documentation))
            {
                documentation += OpenRpcUiNewLine;
            }

            documentation += xmlDocs.Remarks;
        }

        if (!string.IsNullOrEmpty(documentation))
        {
            // OpenRPC UI does not accept line breaks without carriage return
            documentation = documentation.Replace(Environment.NewLine, OpenRpcUiNewLine);

            var newLineIndex = documentation.IndexOf(OpenRpcUiNewLine, StringComparison.Ordinal);
            if (newLineIndex >= 0)
            {
                builder.Title(documentation[..newLineIndex] + "...")
                    .Description(documentation);
            }
            else
            {
                builder.Title(documentation);
            }
        }

        return builder;
    }

    private const string OpenRpcUiNewLine = "\n\r";
}
