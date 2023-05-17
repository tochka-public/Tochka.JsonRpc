using System.Reflection;
using System.Reflection.Emit;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.ApiExplorer;

public class TypeEmitter : ITypeEmitter
{
    private readonly ModuleBuilder moduleBuilder;

    public TypeEmitter()
    {
        var assemblyName = new AssemblyName(Constants.GeneratedModelsAssemblyId);
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
        moduleBuilder = assemblyBuilder.DefineDynamicModule(Constants.GeneratedModelsAssemblyId);
    }

    public Type CreateRequestType(string methodName, Type baseParamsType, IReadOnlyDictionary<string, Type> defaultBoundParams, Type? serializerOptionsProviderType)
    {
        var responseTypeName = $"{methodName} request";
        var paramsType = GetParamsType($"{methodName} params", baseParamsType, defaultBoundParams);
        var responseType = typeof(Request<>).MakeGenericType(paramsType);
        return GenerateTypeWithInfoAttribute(responseTypeName, responseType, serializerOptionsProviderType, methodName);
    }

    public Type CreateResponseType(string methodName, Type resultType, Type? serializerOptionsProviderType)
    {
        var responseTypeName = $"{methodName} response";
        var responseType = typeof(Response<>).MakeGenericType(resultType);
        return GenerateTypeWithInfoAttribute(responseTypeName, responseType, serializerOptionsProviderType, methodName);
    }

    /// <summary>
    /// Combine multiple arguments into one type or use type directly if impossible to create descendant
    /// </summary>
    /// <returns></returns>
    private Type GetParamsType(string name, Type baseParamsType, IReadOnlyDictionary<string, Type> defaultBoundParams)
    {
        // Can't inherit from sealed, don't want to deal with valuetypes, default public constructor required
        if (baseParamsType.IsSealed || baseParamsType.IsValueType || baseParamsType.GetConstructor(Type.EmptyTypes) == null)
        {
            // if (defaultBoundParams.Count > 0)
            // {
            //     log.LogWarning("Can not inherit from {}, ignored {} extra properties", baseParamsType.Name, defaultBoundParams.Count);
            // }
            return baseParamsType;
        }

        // don't need to derive, use type as is
        if (defaultBoundParams.Count == 0)
        {
            return baseParamsType;
        }

        var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public, baseParamsType);
        foreach (var property in defaultBoundParams)
        {
            CreateProperty(typeBuilder, property.Key, property.Value);
        }

        return typeBuilder.CreateType()!;
    }

    /// <summary>
    /// Create new type with attribute
    /// </summary>
    /// <returns></returns>
    private Type GenerateTypeWithInfoAttribute(string name, Type baseType, Type? serializerOptionsProviderType, string methodName)
    {
        var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public, baseType);

        var attrType = typeof(JsonRpcTypeMetadataAttribute);
        var attrConstructor = attrType.GetConstructor(new[] { typeof(Type), typeof(string) })!;
        var attrParams = new object?[] { serializerOptionsProviderType, methodName };
        var attrBuilder = new CustomAttributeBuilder(attrConstructor, attrParams);
        typeBuilder.SetCustomAttribute(attrBuilder);

        return typeBuilder.CreateType()!;
    }

    // https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.typebuilder.defineproperty?view=net-6.0
    private static void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
    {
        var fieldName = $"_{propertyName}";
        var fieldBuilder = typeBuilder.DefineField(fieldName, propertyType, FieldAttributes.Private);

        // The property set and get methods require special flags
        const MethodAttributes getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

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
    }
}
