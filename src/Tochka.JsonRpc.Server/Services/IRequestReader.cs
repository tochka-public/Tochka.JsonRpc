using System.Text;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.Server.Services;

public interface IRequestReader
{
    Task<IRequestWrapper> GetRequestWrapper(HttpContext context, Encoding encoding);
}