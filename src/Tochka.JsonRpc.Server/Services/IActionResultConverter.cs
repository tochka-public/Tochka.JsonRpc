using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Models;

namespace Tochka.JsonRpc.Server.Services
{
    public interface IActionResultConverter
    {
        /// <summary>
        /// Create object result with proper formatter or handle special types to pass raw data
        /// </summary>
        /// <param name="actionResult"></param>
        /// <param name="metadata"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        IActionResult ConvertActionResult(IActionResult actionResult, MethodMetadata metadata, IJsonRpcSerializer serializer);

        IActionResult GetFailedBindingResult(ModelStateDictionary modelState);
    }
}