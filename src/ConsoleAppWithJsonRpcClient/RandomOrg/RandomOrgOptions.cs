using Tochka.JsonRpc.Client.Settings;

namespace ConsoleAppWithJsonRpcClient.RandomOrg;

public class RandomOrgOptions : JsonRpcClientOptionsBase
{
    public string? Token { get; set; }
}