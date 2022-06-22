using System.Collections.ObjectModel;
using System.Text;

namespace EarthquakeMap2.Utilities;

public static class StringUtil
{
    private const int OFFSET = 65248;
    public static string ToHankaku(in string s)
    {
        var res = new StringBuilder();
        foreach (var c in s)
        {
            var c2 = c - OFFSET;
            if (c2 is >= 33 and < 127) res.Append((char) c2);
            else res.Append(c);
        }

        return res.ToString();
    }
}