using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers;

[ExcludeFromCodeCoverage]
public record BatchRequestWrapper(List<JsonDocument> Calls) : IRequestWrapper;
