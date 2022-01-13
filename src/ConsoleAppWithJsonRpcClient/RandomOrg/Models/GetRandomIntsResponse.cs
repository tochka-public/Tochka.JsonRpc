namespace ConsoleAppWithJsonRpcClient.RandomOrg.Models;

public class GetRandomIntsResponse : RandomOrgResponseBase
{
    public Random? Random { get; set; }
}

public class Random
{
    public List<int>? Data { get; set; }
    public DateTime CompletionTime { get; set; }
}