﻿#region USING_DIRECTIVES
using System.Text.RegularExpressions;
#endregion

namespace Sol.Common
{
    public sealed class CustomIPFormat
    {
        private static readonly Regex _parseRegex = new Regex(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|\*)((\.|$)(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|\*)){0,3}(:[0-9]{4,5})?$", RegexOptions.Compiled);

        public string Content { get; set; }

        public static bool TryParse(string str, out CustomIPFormat res)
        {
            if (_parseRegex.IsMatch(str))
            {
                res = new CustomIPFormat() { Content = str };
                return true;
            }
            else
            {
                res = null;
                return false;
            }
        }
    }
}