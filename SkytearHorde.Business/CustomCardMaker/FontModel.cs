using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkytearHorde.Business.CustomCardMaker
{
    public class FontModel
    {
        public string NormalFont { get; set; }
        public string BoldFont { get; set; }

        public List<string> UnsupportedChars { get; set; }
        public FontModel? Fallback { get; set; }

        public FontModel(string normalFont, string boldFont)
        {
            NormalFont = normalFont;
            BoldFont = boldFont;

            UnsupportedChars = new List<string>();
        }
    }
}
