using System;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.V1.Common.Models.Id;

namespace Tochka.JsonRpc.V1.Client.Services
{
    public class JsonRpcIdGenerator : IJsonRpcIdGenerator
    {
        private readonly ILogger<JsonRpcIdGenerator> log;

        public JsonRpcIdGenerator(ILogger<JsonRpcIdGenerator> log)
        {
            this.log = log;
        }

        /// <summary>
        /// Creates string ID from GUID
        /// </summary>
        public IRpcId GenerateId()
        {
            var value = Guid.NewGuid().ToString();
            var result = new StringRpcId(value);
            log.LogTrace($"Generated request id [{result}]");
            return result;
        }
    }
}
