namespace Tochka.JsonRpc.Common.Models.Response.Errors;

public interface IError
{
    int Code { get; }

    // SHOULD be limited to a concise single sentence.
    string Message { get; }
}
