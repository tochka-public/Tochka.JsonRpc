namespace Tochka.JsonRpc.Server.Settings;

public enum JsonRpcMethodStyle
{
    /// <summary>
    /// Default. Treat rpc "method" as "controller.action"
    /// </summary>
    ControllerAndAction,

    /// <summary>
    /// Treat rpc "method" as "action", looking for action in every rpc controller
    /// </summary>
    ActionOnly
}
