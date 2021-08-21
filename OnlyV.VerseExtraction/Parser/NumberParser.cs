using System;
using System.Text;

namespace OnlyV.VerseExtraction.Parser
{
    internal static class NumberParser
    {
        public static bool TryParseNumber(string str, out int value)
        {
            StringBuilder sb = new StringBuilder();
            for (int n = 0; n < str.Length; ++n)
            {
                var val = char.GetNumericValue(str, n);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (val == -1)
                {
                    value = 0;
                    return false;
                }

                sb.Append(val);
            }

            try
            {
                value = Convert.ToInt32(sb.ToString());
            }
            catch (Exception)
            {
                value = 0;
            }

            return value != 0;
        }
    }
}
