namespace Tochka.JsonRpc.Server.Settings;

/// <summary>
/// Method style representing how to convert action name to JSON-RPC method
/// </summary>
public enum JsonRpcMethodStyle
{
    /// <summary>
    /// Default. Treat JSON-RPC "method" as "controller.action"
    /// </summary>
    ControllerAndAction,

    /// <summary>
    /// Treat JSON-RPC "method" as "action", looking for action in every rpc controller
    /// </summary>
    ActionOnly
}
