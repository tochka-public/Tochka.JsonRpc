using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Server.Attributes;

namespace Tochka.JsonRpc.ApiExplorer
{
    /// <summary>
    /// Creates types for proper schema generation. See https://jonathancrozier.com/blog/time-to-reflect-how-to-add-properties-to-a-c-sharp-object-dynamically-at-runtime
    /// </summary>
    public class TypeEmitter : ITypeEmitter
    {
        private readonly ILogger log;
        private readonly ModuleBuilder moduleBuilder;
        private readonly object lockObject = new object();

        public const string AssemblyId = "JsonRpcGeneratedModelTypes";

        public TypeEmitter(ILogger<TypeEmitter> log)
        {
            this.log = log;
            var assemblyName = new AssemblyName(AssemblyId);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(AssemblyId);
        }

        public Type CreateRequestType(string actionName, Type baseBodyType, IReadOnlyDictionary<string, Type> properties, Type jsonRpcRequestSerializer, XElement actionXmlDoc)
        {
            lock (lockObject)
            {
                var bodyTypeName = $"{actionName}Params".Replace('.', '_').Replace('`', '_');
                var requestTypeName = $"{actionName}Request".Replace('.', '_').Replace('`', '_');

                var bodyType = GetBodyType(bodyTypeName, baseBodyType, properties, actionXmlDoc);
                var requestType = typeof(Request<>).MakeGenericType(bodyType);
                return GenerateTypeWithAttributes(requestTypeName, requestType, jsonRpcRequestSerializer, actionName);
            }
        }

        public Type CreateResponseType(string actionName, Type bodyType, Type jsonRpcRequestSerializer)
        {
            lock (lockObject)
            {
                var responseTypeName = $"{actionName}Response".Replace('.', '_').Replace('`', '_');
                var responseType = typeof(Response<>).MakeGenericType(bodyType);
                return GenerateTypeWithAttributes(responseTypeName, responseType, jsonRpcRequestSerializer, actionName);
            }
        }

        /// <summary>
        /// Combine multiple arguments into one type or use type directly if impossible to create descendant
        /// </summary>
        /// <returns></returns>
        private Type GetBodyType(string name, Type baseBodyType, IReadOnlyDictionary<string, Type> properties, XElement actionXmlDoc)
        {
            // Can't inherit from sealed, don't want to deal with valuetypes, default public constructor required
            if (baseBodyType.IsSealed || baseBodyType.IsValueType || baseBodyType.GetConstructor(Type.EmptyTypes) == null)
            {
                if (properties.Count > 0)
                {
                    log.LogWarning("Can not inherit from {}, ignored {} extra properties", baseBodyType.Name, properties.Count);
                }
                return baseBodyType;
            }

            // don't need to derive, use type as is
            if (properties.Count == 0)
            {
                return baseBodyType;
            }
            
            var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public, baseBodyType);
            foreach (var property in properties)
            {
                var description = GetXmlParam(actionXmlDoc, property.Key);
                CreateProperty(typeBuilder, property.Key, property.Value, description);
            }

            return typeBuilder.CreateType();
        }

        private void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType, string description)
        {
            var fieldName = $"_{propertyName}";
            var fieldBuilder = typeBuilder.DefineField(fieldName, propertyType, FieldAttributes.Private);
            // The property set and get methods require special flags
            var getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            // Define 'get' accessor method
            var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", getSetAttributes, propertyType, Type.EmptyTypes);
            var propertyGetGenerator = getMethodBuilder.GetILGenerator();
            propertyGetGenerator.Emit(OpCodes.Ldarg_0);
            propertyGetGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            propertyGetGenerator.Emit(OpCodes.Ret);
            // Define 'set' accessor method
            var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", getSetAttributes, null, new[] { propertyType });
            var propertySetGenerator = setMethodBuilder.GetILGenerator();
            propertySetGenerator.Emit(OpCodes.Ldarg_0);
            propertySetGenerator.Emit(OpCodes.Ldarg_1);
            propertySetGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            propertySetGenerator.Emit(OpCodes.Ret);
            // Map getter-setter to a PropertyBuilder and their corresponding behaviors
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);

            var attrType = typeof(DisplayAttribute);
            var attrConstructor = attrType.GetConstructor(Type.EmptyTypes);
            var attrParams = new object[] {};
            var props = new PropertyInfo[] { attrType.GetProperty(nameof(DisplayAttribute.Description)) };
            var propValues = new object[] { description };
            var attrBuilder = new CustomAttributeBuilder(attrConstructor, attrParams, props, propValues);
            propertyBuilder.SetCustomAttribute(attrBuilder);
        }

        /// <summary>
        /// Create new type with attribute
        /// </summary>
        /// <returns></returns>
        private Type GenerateTypeWithAttributes(string name, Type baseType, Type jsonRpcRequestSerializer, string actionName)
        {
            var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public, baseType);

            var attrType = typeof(JsonRpcTypeInfoAttribute);
            var attrConstructor = attrType.GetConstructor(new []{ typeof(Type), typeof(string) });
            var attrParams = new object[] {jsonRpcRequestSerializer, actionName};
            var attrBuilder = new CustomAttributeBuilder(attrConstructor, attrParams);
            typeBuilder.SetCustomAttribute(attrBuilder);

            return typeBuilder.CreateType();
        }

        /// <summary>
        /// Finds "param" xmldoc for given name
        /// </summary>
        /// <param name="actionXmlDoc"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        private string GetXmlParam(XElement actionXmlDoc, string paramName)
        {
            return actionXmlDoc?.Elements("param").FirstOrDefault(x => x.Attributes().Any(a => a.Name == "name" && a.Value == paramName))?.Value ?? string.Empty;
        }
    }
}