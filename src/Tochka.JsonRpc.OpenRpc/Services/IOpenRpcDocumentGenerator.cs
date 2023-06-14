using Tochka.JsonRpc.OpenRpc.Models;

namespace Tochka.JsonRpc.OpenRpc.Services;

public interface IOpenRpcDocumentGenerator
{
    Models.OpenRpc Generate(OpenRpcInfo info, string documentName, Uri host);
}
