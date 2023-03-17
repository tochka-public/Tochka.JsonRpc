using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.V1.Server.IntegrationTests.Cases
{
    /// <summary>
    /// Test empty or error responses. "The Server MUST NOT reply to a Notification. Client would not be aware of any errors"
    /// </summary>
    public class NoParamsNotifications
    {
        public static IEnumerable Cases => Notifications.Select(x => new TestCaseData(x.data, x.expected));

        private static IEnumerable<(object data, object expected)> Notifications
        {
            get
            {
                yield return (
                    new UntypedNotification()
                    {
                        Method = "values.void"
                    },
                    string.Empty
                );
            }
        }
    }
}
