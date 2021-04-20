using System;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Server.Exceptions
{
    /// <summary>
    /// Special exception which is converted into response with given code, message and data
    /// </summary>
    public class JsonRpcErrorResponseException : Exception
    {
        public IError Error { get; }
        
        public JsonRpcErrorResponseException(IError error)
        {
            Error = error;
        }
    }
}
