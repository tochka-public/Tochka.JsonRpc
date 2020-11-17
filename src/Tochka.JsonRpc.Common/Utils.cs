using System;
using System.Collections;
using System.Collections.Generic;

namespace Tochka.JsonRpc.Common
{
    public static class Utils
    {
        public static string GetSwaggerFriendlyDocumentName(Type serializerType)
        {
            return $"{JsonRpcConstants.JsonRpcSwaggerPrefix}_{serializerType.Name.Replace("JsonRpcSerializer", string.Empty)}".ToLowerInvariant();
        }

        public static bool IsCollection(Type type)
        {
            return type.GetInterface(nameof(ICollection)) != null;
        }
    }
}