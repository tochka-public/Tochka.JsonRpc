using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Id;

[ExcludeFromCodeCoverage]
public class StringRpcId : IRpcId, IEquatable<StringRpcId>
{
    public string StringValue { get; }

    public StringRpcId(string value) => StringValue = value ?? throw new ArgumentNullException(nameof(value));

    public bool Equals(StringRpcId? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return StringValue == other.StringValue;
    }

    public bool Equals(IRpcId? other) => Equals(other as StringRpcId);

    public override string ToString() => StringValue;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((StringRpcId) obj);
    }

    public override int GetHashCode() => StringValue.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(StringRpcId left, StringRpcId right) => Equals(left, right);

    public static bool operator !=(StringRpcId left, StringRpcId right) => !Equals(left, right);
}
