using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Client.Settings;

namespace Tochka.JsonRpc.Client.Tests
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