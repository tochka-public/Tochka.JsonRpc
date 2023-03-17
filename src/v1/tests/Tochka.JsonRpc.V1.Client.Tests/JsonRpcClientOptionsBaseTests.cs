using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Client.Settings;

namespace Tochka.JsonRpc.V1.Client.Tests
{
    public class JsonRpcClientOptionsBaseTests
    {
        [Test]
        public void Test_Timeout_DefaultValue()
        {
            var mock = new Mock<JsonRpcClientOptionsBase>()
            {
                CallBase = true
            };

            var value = mock.Object.Timeout;

            value.Should().Be(TimeSpan.FromSeconds(10));
        }

        [Test]
        public void Test_Timeout_Set()
        {
            var mock = new Mock<JsonRpcClientOptionsBase>()
            {
                CallBase = true
            };

            mock.Object.Timeout = TimeSpan.Zero;

            mock.Object.Timeout.Should().Be(TimeSpan.Zero);
        }
    }
}