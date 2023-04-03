using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Server.Services;

[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Call is official name")]
public interface INestedContextFactory
{
    HttpContext Create(HttpContext context, IUntypedCall call, Encoding requestEncoding);
}