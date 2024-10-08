using Json.Schema;

namespace Tochka.JsonRpc.OpenRpc.Services;

internal record XmlDocValues(string? Summary, string? Remarks);

internal static class JsonSchemaBuilderExtensions
{
    public static JsonSchemaBuilder AppendXmlDocs(this JsonSchemaBuilder builder, XmlDocValues xmlDocs)
    {
        const string openRpcUiNewLine = "\n\r";
        
        var documentation = xmlDocs.Summary;
        
        if (!string.IsNullOrEmpty(xmlDocs.Remarks))
        {
            if (!string.IsNullOrEmpty(documentation))
            {
                documentation += openRpcUiNewLine;
            }
            
            documentation += xmlDocs.Remarks;
        }
        
        if (!string.IsNullOrEmpty(documentation))
        {
            // OpenRPC UI does not accept line breaks without carriage return
            documentation = documentation.Replace(Environment.NewLine, openRpcUiNewLine);
            
            var newLineIndex = documentation.IndexOf(openRpcUiNewLine, StringComparison.Ordinal);
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
}
