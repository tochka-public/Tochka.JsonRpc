using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Net.Http.Headers;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Pipeline;

namespace Tochka.JsonRpc.Server;

internal static class Utils
{
    public static bool IsJsonRpcController(TypeInfo typeInfo) => typeof(JsonRpcController).IsAssignableFrom(typeInfo);

    /// <summary>
    /// Find attribute on action or controller
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="actionModel"></param>
    /// <returns></returns>
    public static TAttribute GetAttributeTransitive<TAttribute>(ActionModel actionModel)
        where TAttribute : Attribute
    {
        var actionAttr = actionModel.Attributes.OfType<TAttribute>().SingleOrDefault();
        var controllerAttr = actionModel.Controller.Attributes.OfType<TAttribute>().SingleOrDefault();
        return actionAttr ?? controllerAttr;
    }

    /// <summary>
    /// Find attribute on parameter
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="parameterModel"></param>
    /// <returns></returns>
    public static TAttribute GetAttribute<TAttribute>(ParameterModel parameterModel)
        where TAttribute : Attribute =>
        parameterModel.Attributes.OfType<TAttribute>().SingleOrDefault();

    /// <summary>
    /// HTTP 2xx
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static bool IsGoodHttpCode(int code) => 200 <= code && code <= 299;

    public static IJsonRpcSerializer GetSerializer(IEnumerable<IJsonRpcSerializer> serializers, Type type) => serializers.First(x => x.GetType() == type);

    /// <summary>
    /// Ignore non JSON Rpc requests
    /// </summary>
    /// <param name="request"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public static bool ProbablyIsJsonRpc(HttpRequest request, MediaTypeHeaderValue contentType)
    {
        if (request.Method != HttpMethods.Post)
        {
            return false;
        }

        if (contentType == null)
        {
            return false;
        }

        if (!contentType.MediaType.Equals(JsonRpcConstants.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    public static Type GetActionResultType(HttpContext context)
    {
        if (context.Items.TryGetValue(JsonRpcConstants.ActionResultTypeItemKey, out var actionResultType))
        {
            return actionResultType as Type;
        }

        return null;
    }
}
