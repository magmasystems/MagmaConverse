using System.Collections.Generic;
using System.Text;

namespace Magmasystems.Framework
{
    public static class FormattingHelpers
    {
        public static string ToFormattedString(this Dictionary<string, object> dict)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var kvp in dict)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append($"[{kvp.Key}] = {kvp.Value}");
            }

            return sb.ToString();
        }
    }
}
