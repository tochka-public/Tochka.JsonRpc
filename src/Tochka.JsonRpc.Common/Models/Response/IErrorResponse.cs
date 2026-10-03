using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Common.Models.Response;

[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is official name")]
public interface IErrorResponse : IResponse
{
    public IError Error { get; }
}
