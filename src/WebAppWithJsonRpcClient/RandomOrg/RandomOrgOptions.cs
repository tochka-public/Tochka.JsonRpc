using Tochka.JsonRpc.Client.Settings;

namespace WebAppWithJsonRpcClient.RandomOrg;

public class RandomOrgOptions : JsonRpcClientOptionsBase
{
    public string? Token { get; set; }
}