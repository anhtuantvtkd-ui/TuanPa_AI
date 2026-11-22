// Decompiled implementation of TagSeq classes for TASK-0018
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tvxd.Meps
{
    public static class TagSeqDir073
    {
        public const string LR = "L-R";
        public const string RL = "R-L";
        public const string UD = "U-D";
        public const string DU = "D-U";

        public static readonly string[] All = new string[] { LR, RL, UD, DU };

        public static string Caption(string d)
        {
            if (d == LR) return "Trái qua phải (L-R)";
            if (d == RL) return "Phải qua trái (R-L)";
            if (d == UD) return "Trên xuống dưới (U-D)";
            if (d == DU) return "Dưới lên trên (D-U)";
            return d ?? "";
        }

        public static string Check(string d)
        {
            if (string.IsNullOrEmpty(d)) return LR;
            foreach (var item in All)
            {
                if (string.Equals(item, d, StringComparison.OrdinalIgnoreCase)) return item;
            }
            return LR;
        }
    }

    public sealed class TagSeqEntry073
    {
        public string id { get; set; }
        public string name { get; set; }
        public string oldText { get; set; }
        public double x { get; set; }
        public double y { get; set; }
    }

    public sealed class TagSeqOptions073
    {
        public string direction { get; set; } = "L-R";
        public string prefix { get; set; } = "";
        public long start { get; set; } = 1;
        public int width { get; set; } = 1;
        public double toleranceY { get; set; } = 1.0;
    }

    public sealed class TagSeqRow073
    {
        public int ordinal { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string oldText { get; set; }
        public string newText { get; set; }
        public double x { get; set; }
        public double y { get; set; }
    }

    public static class TagSequence073
    {
        public const int MaxWidth = 9;
        public const int MaxCount = 5000;
        public const long MaxNumber = 2147483647L;

        public static string Format(string prefix, long n, int width)
        {
            if (prefix == null) prefix = "";
            return prefix + n.ToString("D" + width, CultureInfo.InvariantCulture);
        }

        private static int Handle(string a, string b)
        {
            if (a == null) a = "";
            if (b == null) b = "";
            int lenCmp = a.Length.CompareTo(b.Length);
            if (lenCmp != 0) return lenCmp;
            return string.CompareOrdinal(a, b);
        }

        public static Comparison<TagSeqEntry073> Order(string d)
        {
            TagSeqDir073.Check(d);
            return (a, b) =>
            {
                int cmp = 0;
                if (d == "L-R")
                {
                    // L-R: X ascending, then Y descending
                    cmp = a.x.CompareTo(b.x);
                    if (cmp == 0) cmp = b.y.CompareTo(a.y);
                }
                else if (d == "R-L")
                {
                    // R-L: X descending, then Y descending
                    cmp = b.x.CompareTo(a.x);
                    if (cmp == 0) cmp = b.y.CompareTo(a.y);
                }
                else if (d == "U-D")
                {
                    // U-D: Y descending, then X ascending
                    cmp = b.y.CompareTo(a.y);
                    if (cmp == 0) cmp = a.x.CompareTo(b.x);
                }
                else // D-U
                {
                    // D-U: Y ascending, then X ascending
                    cmp = a.y.CompareTo(b.y);
                    if (cmp == 0) cmp = a.x.CompareTo(b.x);
                }

                if (cmp != 0) return cmp;
                return Handle(a.id, b.id);
            };
        }

        public static List<TagSeqRow073> Plan(IList<TagSeqEntry073> entries, TagSeqOptions073 o)
        {
            if (entries == null) return new List<TagSeqRow073>();
            var list = entries.ToList();
            string direction = TagSeqDir073.Check(o != null ? o.direction : "L-R");
            double toleranceY = o != null && o.toleranceY > 0 && !double.IsNaN(o.toleranceY) && !double.IsInfinity(o.toleranceY) ? o.toleranceY : 1.0;
            if (direction == TagSeqDir073.LR || direction == TagSeqDir073.RL)
            {
                var bands = new List<List<TagSeqEntry073>>();
                foreach (var entry in list.OrderByDescending(e => e.y).ThenBy(e => e.x).ThenBy(e => e.id, StringComparer.Ordinal))
                {
                    List<TagSeqEntry073> band = null;
                    foreach (var candidate in bands)
                    {
                        double meanY = candidate.Average(e => e.y);
                        if (Math.Abs(entry.y - meanY) <= toleranceY) { band = candidate; break; }
                    }
                    if (band == null) { band = new List<TagSeqEntry073>(); bands.Add(band); }
                    band.Add(entry);
                }
                list = bands.SelectMany(b => (direction == TagSeqDir073.LR
                    ? b.OrderBy(e => e.x).ThenBy(e => e.id, StringComparer.Ordinal)
                    : b.OrderByDescending(e => e.x).ThenBy(e => e.id, StringComparer.Ordinal))).ToList();
            }
            else
            {
                list.Sort(Order(direction));
            }

            var rows = new List<TagSeqRow073>();
            long cur = o != null ? o.start : 1;
            string prefix = o != null ? o.prefix : "";
            int width = o != null ? o.width : 1;

            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                var row = new TagSeqRow073
                {
                    ordinal = i + 1,
                    id = e.id,
                    name = e.name,
                    oldText = e.oldText,
                    x = e.x,
                    y = e.y,
                    newText = Format(prefix, cur + i, width)
                };
                rows.Add(row);
            }
            return rows;
        }
    }
}
