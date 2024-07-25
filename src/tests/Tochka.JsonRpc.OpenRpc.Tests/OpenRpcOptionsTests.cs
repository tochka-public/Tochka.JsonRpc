using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using NUnit.Framework;

namespace Tochka.JsonRpc.OpenRpc.Tests;

[TestFixture]
public class OpenRpcOptionsTests
{
    [Test]
    public void DocumentSelector_GroupNameEqualToDocName_ReturnTrue()
    {
        var docName = "docName";
        var description = new ApiDescription { GroupName = docName };

        var result = OpenRpcOptions.DocumentSelector(docName, description);

        result.Should().BeTrue();
    }

    [TestCase(null)]
    [TestCase("")]
    public void DocumentSelector_GroupNameEmpty_ReturnFalse(string? groupName)
    {
        var docName = "docName";
        var description = new ApiDescription { GroupName = groupName };

        var result = OpenRpcOptions.DocumentSelector(docName, description);

        result.Should().BeFalse();
    }

    [TestCase("jsonrpc_v1")]
    [TestCase("jsonrpc__v1")]
    [TestCase("jsonrpc_snakecase_v1")]
    [TestCase("jsonrpc_smth_v1")]
    [TestCase("jsonrpc_smth_more_v1")]
    public void DocumentSelector_DocNameStartsAndEndsAsGroupName_ReturnTrue(string? groupName)
    {
        var docName = "jsonrpc_v1";
        var description = new ApiDescription { GroupName = groupName };

        var result = OpenRpcOptions.DocumentSelector(docName, description);

        result.Should().BeTrue();
    }
}
