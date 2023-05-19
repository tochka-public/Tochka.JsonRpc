using Tochka.JsonRpc.OpenRpc.Models;

namespace Tochka.JsonRpc.OpenRpc;

public sealed class OpenRpcOptions
{
    public string DocumentPath { get; set; } = OpenRpcConstants.DefaultDocumentPath;
    public Dictionary<string, Info> Docs { get; set; } = new();
}
