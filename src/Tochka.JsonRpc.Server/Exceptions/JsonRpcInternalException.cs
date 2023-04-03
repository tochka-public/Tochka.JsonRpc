using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server.Models;

namespace Tochka.JsonRpc.Server.Exceptions;

[ExcludeFromCodeCoverage]
public class JsonRpcInternalException : Exception
{
    private IActionResult ActionResult { get; }
    private MethodMetadata MethodMetadata { get; }
    public int? HttpCode { get; }

    public JsonRpcInternalException()
    {
    }

    public JsonRpcInternalException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public JsonRpcInternalException(string message) : base(message)
    {
    }

    public JsonRpcInternalException(string message, IActionResult actionResult, MethodMetadata methodMetadata, int? httpCode) : base(message)
    {
        ActionResult = actionResult;
        MethodMetadata = methodMetadata;
        HttpCode = httpCode;
    }

    public override string Message
    {
        get
        {
            var sb = new StringBuilder(base.Message);

            if (ActionResult != null)
            {
                sb.AppendLine();
                sb.Append(CultureInfo.InvariantCulture, $"{nameof(ActionResult)} is [{ActionResult?.GetType().Name}]: [{ActionResult}]");
            }

            if (MethodMetadata != null)
            {
                sb.AppendLine();
                sb.Append(CultureInfo.InvariantCulture, $"{nameof(MethodMetadata)}: [{MethodMetadata}]");
            }

            if (HttpCode != null)
            {
                sb.AppendLine();
                sb.Append(CultureInfo.InvariantCulture, $"{nameof(HttpCode)}: [{HttpCode}]");
            }

            return sb.ToString();
        }
    }
}