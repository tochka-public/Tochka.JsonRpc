using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.V1.Server.Models;

namespace Tochka.JsonRpc.V1.Server.Exceptions
{
    [ExcludeFromCodeCoverage]
    internal class JsonRpcInternalException : Exception
    {
        private IActionResult ActionResult { get; }
        private MethodMetadata MethodMetadata { get; }
        public int? HttpCode { get; }

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
                    sb.Append($"{nameof(ActionResult)} is [{ActionResult?.GetType().Name}]: [{ActionResult}]");
                }

                if (MethodMetadata != null)
                {
                    sb.AppendLine();
                    sb.Append($"{nameof(MethodMetadata)}: [{MethodMetadata}]");
                }

                if (HttpCode != null)
                {
                    sb.AppendLine();
                    sb.Append($"{nameof(HttpCode)}: [{HttpCode}]");
                }

                return sb.ToString();
            }
        }
    }
}