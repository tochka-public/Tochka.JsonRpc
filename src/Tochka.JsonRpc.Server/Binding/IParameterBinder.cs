using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.Server.Models.Binding;

namespace Tochka.JsonRpc.Server.Binding
{
    public interface IParameterBinder
    {
        Task SetResult(ModelBindingContext context, IParseResult result, string parameterName, JsonRpcBindingContext jsonRpcBindingContext);
    }
}