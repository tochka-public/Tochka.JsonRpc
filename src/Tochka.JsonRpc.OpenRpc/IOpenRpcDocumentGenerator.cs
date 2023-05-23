using Tochka.JsonRpc.OpenRpc.Models;

namespace Tochka.JsonRpc.OpenRpc;

public interface IOpenRpcDocumentGenerator
{
    Models.OpenRpc Generate(OpenRpcInfo info, string documentName, Uri host);
}
