using AdServer.Enums;
using Microsoft.AspNetCore.Http;

namespace AdServer.Checks
{
    public interface IRequestCheck
    {
        bool Check(HttpContext context, TrackingSource source);
    }
}
