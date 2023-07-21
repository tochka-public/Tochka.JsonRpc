using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Tochka.JsonRpc.OpenRpc.Models;

namespace Tochka.JsonRpc.OpenRpc;

/// <summary>
/// Options to configure OpenRPC document generation
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed class OpenRpcOptions
{
    /// <summary>
    /// Route to OpenRPC document
    /// </summary>
    /// <remarks>
    /// Must contain template parameter with <see cref="OpenRpcConstants.DocumentTemplateParameterName" />
    /// </remarks>
    public string DocumentPath { get; set; } = OpenRpcConstants.DefaultDocumentPath;

    /// <summary>
    /// Metadata for documents by their name
    /// </summary>
    public Dictionary<string, OpenRpcInfo> Docs { get; set; } = new();

    /// <summary>
    /// Name of default server in OpenRPC document
    /// </summary>
    public string DefaultServerName { get; set; } = OpenRpcConstants.DefaultServerName;

    /// <summary>
    /// Ignore any actions with <see cref="ObsoleteAttribute" />
    /// </summary>
    /// <remarks>
    /// False by default
    /// </remarks>
    public bool IgnoreObsoleteActions { get; set; }

    /// <summary>
    /// Strategy for selecting actions to describe
    /// </summary>
    public Func<string, ApiDescription, bool> DocInclusionPredicate { get; set; } = static (_, _) => true;
}
