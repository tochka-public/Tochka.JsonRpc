using Newtonsoft.Json;

namespace WebAppWithJsonRpcClient.RandomOrg.Models;

public class GetRandomIntsRequest : RandomOrgRequestBase
{
    [JsonProperty("n")]
    public int QuantityInts { get; set; }
    public int Min { get; set; }
    public int Max { get; set; }
    public bool Replacement { get; set; }
}