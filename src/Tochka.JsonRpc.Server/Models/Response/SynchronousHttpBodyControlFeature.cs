using Microsoft.AspNetCore.Http.Features;

namespace Tochka.JsonRpc.Server.Models.Response
{
    public class SynchronousHttpBodyControlFeature : IHttpBodyControlFeature
    {
        public SynchronousHttpBodyControlFeature()
        {
            AllowSynchronousIO = true;
        }

        public bool AllowSynchronousIO { get; set; }
    }
}