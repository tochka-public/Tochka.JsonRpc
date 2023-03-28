using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers;

[ExcludeFromCodeCoverage]
public record BadRequestWrapper(Exception Exception) : IRequestWrapper;
