using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers;

[ExcludeFromCodeCoverage]
public sealed record BadRequestWrapper(Exception Exception) : IRequestWrapper;
