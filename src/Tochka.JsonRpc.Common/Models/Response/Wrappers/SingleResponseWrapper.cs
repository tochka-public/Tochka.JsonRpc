using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers;

[ExcludeFromCodeCoverage]
public class SingleResponseWrapper : IResponseWrapper
{
    [SuppressMessage("Naming", "CA1720:Идентификатор содержит имя типа")]
    public IResponse Single { get; set; }
}
