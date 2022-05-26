using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Server.Pipeline;

namespace Tochka.JsonRpc.Server.Conventions
{
    /// <summary>
    /// Route JSON Rpc requests to controllers
    /// </summary>
    public class ControllerConvention : IControllerModelConvention
    {
        private readonly ILogger log;

        public ControllerConvention(ILogger<ControllerConvention> log)
        {
            this.log = log;
        }

        public void Apply(ControllerModel controllerModel)
        {
            // only mess with JSON Rpc controllers
            if (!Utils.IsJsonRpcController(controllerModel.ControllerType))
            {
                return;
            }

            // ignore what mvc could have done
            controllerModel.Selectors.Clear();
            controllerModel.Filters.Insert(0, new ServiceFilterAttribute(typeof(JsonRpcFilter)));

            log.LogTrace("{controllerName}: applied {filterName}", controllerModel.DisplayName, nameof(JsonRpcFilter));
        }
    }
}