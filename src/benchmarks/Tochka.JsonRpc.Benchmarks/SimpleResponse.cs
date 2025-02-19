using JetBrains.Annotations;

namespace Tochka.JsonRpc.Benchmarks;

[UsedImplicitly]
public record SimpleResponse<T>
(
    T? Result
);
