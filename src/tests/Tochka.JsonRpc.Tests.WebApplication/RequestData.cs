namespace Tochka.JsonRpc.Tests.WebApplication;

public record RequestData<T>(int IntField, T GenericField)
{
    public RequestData() : this(default, default)
    {
    }
}
