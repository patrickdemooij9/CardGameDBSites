using AdServer.Enums;
using DeviceDetectorNET;
using Microsoft.AspNetCore.Http;

namespace AdServer.Checks
{
    public class UserAgentCheck : IRequestCheck
    {
        public bool Check(HttpContext context, TrackingSource source)
        {
            return !new DeviceDetector(context.Request.Headers["User-Agent"].ToString()).IsBot();
        }
    }
}
