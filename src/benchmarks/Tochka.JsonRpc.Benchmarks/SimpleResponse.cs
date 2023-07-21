using JetBrains.Annotations;

namespace Tochka.JsonRpc.Benchmarks;

[UsedImplicitly]
internal record SimpleResponse<T>(T? Result);
