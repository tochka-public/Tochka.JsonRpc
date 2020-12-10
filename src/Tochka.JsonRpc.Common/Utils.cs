using System;
using System.Collections;
using System.Collections.Generic;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Common
{
    public static class Utils
    {
        public static string GetSwaggerFriendlyDocumentName(Type serializerType, Type defaultSerializer)
        {
            // use simple name for document with default configured serializer
            // TODO: is it possible to get from Configuration?
            if (serializerType == defaultSerializer)
            {
                return JsonRpcConstants.ApiDocumentName.ToLowerInvariant();
            }

            return $"{JsonRpcConstants.ApiDocumentName}_{serializerType.Name.Replace("JsonRpcSerializer", string.Empty)}".ToLowerInvariant();
        }

        public static bool IsCollection(Type type)
        {
            return type.GetInterface(nameof(ICollection)) != null;
        }
    }
}