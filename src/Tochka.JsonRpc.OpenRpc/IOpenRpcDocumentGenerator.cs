using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.OpenRpc.Models;

namespace Tochka.JsonRpc.OpenRpc;

public interface IOpenRpcDocumentGenerator
{
    Models.OpenRpc Generate(Info info, string documentName, Uri host);
}
