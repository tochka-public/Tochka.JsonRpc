using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Settings;

[SuppressMessage("Naming", "CA1720:Identifiers should not contain type names", Justification = "Object is official name")]
public enum BindingStyle
{
    Default,
    Object,
    Array
}