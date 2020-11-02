using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Creates types for proper schema generation. See https://jonathancrozier.com/blog/time-to-reflect-how-to-add-properties-to-a-c-sharp-object-dynamically-at-runtime
    /// </summary>
    public class ModelTypeEmitter
    {
        private readonly ModuleBuilder moduleBuilder;

        public ModelTypeEmitter()
        {
            var uniqueIdentifier = Guid.NewGuid().ToString();
            var assemblyName = new AssemblyName(uniqueIdentifier);

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(uniqueIdentifier);
        }

        public Type CreateModelType(string actionName, IReadOnlyDictionary<string, Type> properties)
        {
            var typeBuilder = moduleBuilder.DefineType($"{actionName}ParamsModel", TypeAttributes.Public);

            foreach (var property in properties)
            {
                CreateProperty(typeBuilder, property.Key, property.Value);
            }
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