using ConsoleAppWithJsonRpcClient.RandomOrg.Models;
using Tochka.JsonRpc.Client;

namespace ConsoleAppWithJsonRpcClient.RandomOrg;

public interface IRandomOrgClient : IJsonRpcClient
{
    Task<GetRandomIntsResponse?> GetRandomInts(int quantityInts, int min, int max, bool replacement, CancellationToken cancellationToken);
}