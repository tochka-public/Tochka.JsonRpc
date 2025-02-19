namespace Tochka.JsonRpc.Benchmarks;

public record SimpleResponse<T>
(
    T? Result
);
