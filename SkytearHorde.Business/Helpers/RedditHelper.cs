using Newtonsoft.Json.Linq;
using RedditSharp.Things;
using RedditSharp;

namespace SkytearHorde.Business.Helpers
{
    public class RedditHelper
    {
        
    }

    public class SubmitData
    {
        internal string api_type { get; set; }

        internal string kind { get; set; }

        internal string sr { get; set; }

        internal string uh { get; set; }

        internal string title { get; set; }

        internal string iden { get; set; }

        internal string captcha { get; set; }

        internal bool resubmit { get; set; }

        internal string text { get; set; }

        internal string flair_id { get; set; }

        public SubmitData()
        {
            api_type = "json";
            kind = "self";
        }
    }
}
