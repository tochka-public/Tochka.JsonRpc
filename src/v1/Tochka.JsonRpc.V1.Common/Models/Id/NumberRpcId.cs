using System;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.V1.Common.Models.Id
{
    [ExcludeFromCodeCoverage]
    public class NumberRpcId : IRpcId, IEquatable<NumberRpcId>
    {
        public readonly long Number;

        public NumberRpcId(long value)
        {
            Number = value;
        }

        public override string ToString() => $"{Number}";

        public bool Equals(NumberRpcId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Number == other.Number;
        }

        public bool Equals(IRpcId other)
        {
            return Equals(other as NumberRpcId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NumberRpcId) obj);
        }

        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }

        public static bool operator ==(NumberRpcId left, NumberRpcId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NumberRpcId left, NumberRpcId right)
        {
            return !Equals(left, right);
        }
    }
}
