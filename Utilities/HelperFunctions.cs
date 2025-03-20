using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Utilities
{
    public static class HelperFunctions
    {
        public static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Decode HTML entities (e.g., &amp; -> &, &lt; -> <, etc.)
            string decodedInput = HttpUtility.HtmlDecode(input);

            // Remove all HTML tags
            string plainText = Regex.Replace(decodedInput, "<.*?>", string.Empty);

            return plainText;
        }

    }
}
