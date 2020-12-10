using System;
using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Models
{
    [ExcludeFromCodeCoverage]
    public class ParameterMetadata
    {
        public JsonName Name { get; }
        public Type Type { get; }
        public int Index { get; }
        public BindingStyle BindingStyle { get; }
        public bool IsOptional { get; }

        public ParameterMetadata(JsonName name, Type type, int index, BindingStyle bindingStyle, bool isOptional)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            Index = index;
            BindingStyle = bindingStyle;
            IsOptional = isOptional;
        }

        public override string ToString()
        {
            return $"{Name} ({Index}), {nameof(IsOptional)}:{IsOptional}, {BindingStyle}";
        }
    }
}