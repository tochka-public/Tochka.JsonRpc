namespace WebAppWithJsonRpcClient.RandomOrg.Models;

public class RandomOrgResponseBase
{
    public int BitsUsed { get; set; }
    public int BitsLeft { get; set; }
    public int RequestsLeft { get; set; }
    public int AdvisoryDelay { get; set; }
}