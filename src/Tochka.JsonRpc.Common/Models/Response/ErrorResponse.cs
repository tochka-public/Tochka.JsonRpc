using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Common.Models.Response;

[ExcludeFromCodeCoverage]
public record ErrorResponse<TError>(IRpcId Id, Error<TError> Error, string Jsonrpc = JsonRpcConstants.Version) : IResponse;
