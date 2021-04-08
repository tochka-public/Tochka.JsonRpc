using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace Tochka.JsonRpc.ApiExplorer
{
    public interface ITypeEmitter
    {
        Type CreateRequestType(string actionName, Type baseBodyType, IReadOnlyDictionary<string, Type> properties, Type jsonRpcRequestSerializer, XElement actionXmlDoc);
        Type CreateResponseType(string actionName, Type bodyType, Type jsonRpcRequestSerializer);
    }
}