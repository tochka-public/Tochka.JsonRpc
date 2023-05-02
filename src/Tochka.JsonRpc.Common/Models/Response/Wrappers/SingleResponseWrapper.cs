using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers;

[ExcludeFromCodeCoverage]
public sealed record SingleResponseWrapper(IResponse Response) : IResponseWrapper;
