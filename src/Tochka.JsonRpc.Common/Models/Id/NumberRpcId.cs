using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Id;

[ExcludeFromCodeCoverage]
public class NumberRpcId : IRpcId, IEquatable<NumberRpcId>
{
    public long NumberValue { get; }

    public NumberRpcId(long value) => NumberValue = value;

    public bool Equals(NumberRpcId? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return NumberValue == other.NumberValue;
    }

    public bool Equals(IRpcId? other) => Equals(other as NumberRpcId);

    public override bool Equals(object? obj) => Equals(obj as NumberRpcId);

    public override string ToString() => $"{NumberValue}";

    public override int GetHashCode() => NumberValue.GetHashCode();

    public static bool operator ==(NumberRpcId left, NumberRpcId right) => Equals(left, right);

    public static bool operator !=(NumberRpcId left, NumberRpcId right) => !Equals(left, right);
}

public record NumberRpcIdRecord(long NumberValue) : IRpcId
{
    public virtual bool Equals(IRpcId? other) => Equals(other as NumberRpcIdRecord);

    public override string ToString() => $"{NumberValue}";
}
