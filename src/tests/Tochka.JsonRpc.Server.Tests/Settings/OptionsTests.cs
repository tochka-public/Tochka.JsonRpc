using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Settings
{
    public class OptionsTests
    {
        [Test]
        public void Test_JsonRpcMethodOptions_Defaults()
        {
            var options = new JsonRpcMethodOptions();
            options.Route.Value.Should().Be(JsonRpcConstants.DefaultRoute);
            options.MethodStyle.Should().Be(MethodStyle.ControllerAndAction);
            options.RequestSerializer.Should().Be<SnakeCaseRpcSerializer>();
        }

        [Test]
        public void Test_JsonRpcOptions_Defaults()
        {
            var options = new JsonRpcOptions();
            options.AllowRawResponses.Should().Be(false);
            options.DetailedResponseExceptions.Should().Be(false);
            options.DefaultMethodOptions.Should().NotBeNull();
            options.BatchHandling.Should().Be(BatchHandling.Sequential);
            options.HeaderSerializer.Should().Be<HeaderRpcSerializer>();
        }
    }
}