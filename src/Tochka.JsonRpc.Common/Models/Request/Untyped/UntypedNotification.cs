using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

[ExcludeFromCodeCoverage]
public class UntypedNotification : Notification<JsonDocument?>, IUntypedCall
{
    /// <inheritdoc />
    public string RawJson { get; set; }
}
