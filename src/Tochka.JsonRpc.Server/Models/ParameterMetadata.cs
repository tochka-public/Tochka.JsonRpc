using System;
using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Models
{
    [ExcludeFromCodeCoverage]
    public class ParameterMetadata
    {
        public BindingStyle BindingStyle { get; }
        public JsonName Name { get; }
        public int Index { get; }
        public bool IsOptional { get; }

        public ParameterMetadata(JsonName name, int index, BindingStyle bindingStyle, bool isOptional)
        {
            BindingStyle = bindingStyle;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Index = index;
            IsOptional = isOptional;
        }

        public override string ToString()
        {
            return $"{Name} ({Index}), {nameof(IsOptional)}:{IsOptional}, {BindingStyle}";
        }
    }
}