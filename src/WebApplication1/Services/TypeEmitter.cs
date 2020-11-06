using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Server.Attributes;

namespace WebApplication1.Services
{
    /*public abstract class JsonRpcModel
    {
        public abstract Type GetSerializerType();
        public abstract Type GetModelType();
    }

    public abstract class JsonRpcModel<TSerializer,TModel> : JsonRpcModel
    {
        public override Type GetSerializerType() => typeof(TSerializer);
        public override Type GetModelType() => typeof(TModel);
    }*/

    public interface ITypeEmitter
    {
        Type CreateModelType(string actionName, Type genericCallType, Type baseBodyType, IReadOnlyDictionary<string, Type> properties, Type jsonRpcRequestSerializer);

    }

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

        public Type CreateModelType(string actionName, Type genericCallType, Type baseBodyType, IReadOnlyDictionary<string, Type> properties, Type jsonRpcRequestSerializer)
        {
            log.LogDebug("actionName {}, genericModelType {}, baseBodyType {}, properties.Count {}, serializer {}", actionName, genericCallType.Name, baseBodyType.Name, properties.Count, jsonRpcRequestSerializer.Name);
            lock (lockObject)
            {
                var bodyTypeName = $"generated_body_{actionName}_{genericCallType.Name}".Replace('.', '_').Replace('`', '_');
                var modelTypeName = $"generated_model_{actionName}_{genericCallType.Name}".Replace('.', '_').Replace('`', '_');

                var bodyType = GetBodyType(bodyTypeName, baseBodyType, properties);
                var callType = GenerateCallTypeWithAttribute(modelTypeName, genericCallType, bodyType, jsonRpcRequestSerializer);
                var attr = callType.GetCustomAttribute<JsonRpcSerializerAttribute>();
                log.LogDebug("Type {}, attribute {}", callType, attr?.SerializerType);
                return callType;

                if (bodyType is TypeBuilder bodyTypeBuilder)
                {
                    // generates type T: combined base and extra properties

                    

                    //var test = typeof(JsonRpcModel<,>).MakeGenericType(jsonRpcRequestSerializer, bodyTypeBuilder);
                    //var typeBuilder = moduleBuilder.DefineType(modelTypeName, TypeAttributes.Public, test);
                    var typeBuilder = moduleBuilder.DefineType(modelTypeName, TypeAttributes.Public);

                    var attrType = typeof(JsonRpcSerializerAttribute);
                    var attrConstructor = attrType.GetConstructor(new[] { typeof(Type) });
                    var attrParams = new object[] { jsonRpcRequestSerializer };
                    var attrBuilder = new CustomAttributeBuilder(attrConstructor, attrParams);
                    typeBuilder.SetCustomAttribute(attrBuilder);

                    var x = bodyTypeBuilder.CreateType();  // ?
                    var modelType = typeBuilder.CreateType();
                    log.LogDebug("Set modelType {} (generated) for {}", bodyTypeBuilder, modelTypeName);

                    var result = genericCallType.MakeGenericType(modelType);  // Request<T2> or Response<T2>. later we unwrap T2 as T

                    try
                    {
                        //var wtf = result.GenericTypeArguments.First().GetCustomAttribute<JsonRpcModelAttribute>().ModelType;
                        //if (wtf == null) throw new DivideByZeroException();

                    }
                    catch (Exception e)
                    {
                        log.LogError(e, "damn!");
                    }

                    return result;
                }
                else
                {
                    // just use existing type
                    //var test = typeof(JsonRpcModel<,>).MakeGenericType(jsonRpcRequestSerializer, bodyType);
                    //var typeBuilder = moduleBuilder.DefineType(modelTypeName, TypeAttributes.Public, test);
                    var typeBuilder = moduleBuilder.DefineType(modelTypeName, TypeAttributes.Public);

                    var attrType = typeof(JsonRpcSerializerAttribute);
                    var attrConstructor = attrType.GetConstructor(new[] { typeof(Type) });
                    var attrParams = new object[] { jsonRpcRequestSerializer };
                    var attrBuilder = new CustomAttributeBuilder(attrConstructor, attrParams);
                    typeBuilder.SetCustomAttribute(attrBuilder);

                    var modelType = typeBuilder.CreateType();
                    log.LogDebug("Set modelType {} for {}", bodyType, modelTypeName);

                    var result = genericCallType.MakeGenericType(modelType);  // Request<T2> or Response<T2>. later we unwrap T2 as T
                    return result;
                }
                //var wrapperModelType = GenerateWrapperlWithAttribute(modelTypeName, jsonRpcRequestSerializer, bodyType);  // generates type T2 : JsonRpcModel with [JsonRpcModel(setializer, T)]
                //var result = genericCallType.MakeGenericType(t2);  // Request<T2> or Response<T2>. later we unwrap T2 as T
                //log.LogDebug("Generated types: body {}, wrapper {}, result {}", bodyType, t2, result);
                //
                //var attr = result.GenericTypeArguments.First().GetCustomAttribute<JsonRpcModelAttribute>();
                //log.LogDebug("Attribute: {}, {}", attr?.SerializerType, attr?.ModelType);
                //
                //return result;
            }
        }

        /// <summary>
        /// Combine multiple arguments into one type or use type if impossible to create descendant
        /// </summary>
        /// <param name="name"></param>
        /// <param name="baseBodyType"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private Type GetBodyType(string name, Type baseBodyType, IReadOnlyDictionary<string, Type> properties)
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
            
            var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public, baseBodyType);
            foreach (var property in properties)
            {
                CreateProperty(typeBuilder, property.Key, property.Value);
            }

            return typeBuilder.CreateType();
        }

        /// <summary>
        /// Create new type with attribute
        /// </summary>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <param name="jsonRpcRequestSerializer"></param>
        /// <returns></returns>
        private Type GenerateCallTypeWithAttribute(string name, Type genericCallType, Type bodyType, Type jsonRpcRequestSerializer)
        {
            var baseCallType = genericCallType.MakeGenericType(bodyType);
            var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public, baseCallType);

            var attrType = typeof(JsonRpcSerializerAttribute);
            var attrConstructor = attrType.GetConstructor(new []{ typeof(Type) });
            var attrParams = new object[] {jsonRpcRequestSerializer};
            var attrBuilder = new CustomAttributeBuilder(attrConstructor, attrParams);
            typeBuilder.SetCustomAttribute(attrBuilder);

            return typeBuilder.CreateType();
        }

        private void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldName = $"_{propertyName}";
            var fieldBuilder = typeBuilder.DefineField(fieldName, propertyType, FieldAttributes.Private);
            // The property set and get methods require a special set of attributes.
            var getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            // Define the 'get' accessor method.
            var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", getSetAttributes, propertyType, Type.EmptyTypes);
            var propertyGetGenerator = getMethodBuilder.GetILGenerator();
            propertyGetGenerator.Emit(OpCodes.Ldarg_0);
            propertyGetGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            propertyGetGenerator.Emit(OpCodes.Ret);
            // Define the 'set' accessor method.
            var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", getSetAttributes, null, new[] { propertyType });
            var propertySetGenerator = setMethodBuilder.GetILGenerator();
            propertySetGenerator.Emit(OpCodes.Ldarg_0);
            propertySetGenerator.Emit(OpCodes.Ldarg_1);
            propertySetGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            propertySetGenerator.Emit(OpCodes.Ret);
            // Lastly, we must map the two methods created above to a PropertyBuilder and their corresponding behaviors, 'get' and 'set' respectively.
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
    }
}