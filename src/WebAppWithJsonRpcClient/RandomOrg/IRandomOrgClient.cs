using Tochka.JsonRpc.Client;
using WebAppWithJsonRpcClient.RandomOrg.Models;

namespace WebAppWithJsonRpcClient.RandomOrg;

public interface IRandomOrgClient : IJsonRpcClient
{
    Task<GetRandomIntsResponse?> GetRandomInts(int quantityInts, int min, int max, bool replacement, CancellationToken cancellationToken);
}