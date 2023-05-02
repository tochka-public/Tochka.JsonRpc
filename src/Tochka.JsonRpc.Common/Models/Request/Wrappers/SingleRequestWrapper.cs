using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers;

[ExcludeFromCodeCoverage]
public sealed record SingleRequestWrapper(JsonDocument Call) : IRequestWrapper;
