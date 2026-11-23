// Decompiled implementation of TagSeq classes for TASK-0018
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Tvxd.Meps
{
    // Minimal contract stubs so TagSeqCad073 can compile with csc (placed before TagSeqDir073 so not merged)
    public class TagItem033
    {
        public string handle;
        public string tagHandle;
        public string text;
        public ObjectId owner;
        public string blockName;
        public bool hasTag;
        public string tagValue;
    }

    public class TagChannel033 { }

    public class TagSnapshot033
    {
        public List<TagItem033> items;
        public TagChannel033 channel;
    }

    public class TagApply033
    {
        public int modified;
        public int created;
        public int deleted;
    }

    public static class TagCad033
    {
        public static TagApply033 ApplyTexts(Database db, TagSnapshot033 snap, IList<string> texts, double height, Action<string> guard)
        {
            throw null;
        }
    }

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
        public string id;
        public string name;
        public string oldText;
        public double x;
        public double y;
    }

    public sealed class TagSeqOptions073
    {
        public string direction;
        public string prefix;
        public long start;
        public int width;
        public double toleranceY;

        public TagSeqOptions073()
        {
            direction = "L-R";
            prefix = "";
            start = 1;
            width = 1;
            toleranceY = 1.0;
        }
    }

    public sealed class TagSeqRow073
    {
        public int ordinal;
        public string id;
        public string name;
        public string oldText;
        public string newText;
        public double x;
        public double y;
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
            // For a continuous route, X is the primary key. Y is only a tie-breaker.
            list.Sort(Order(direction));

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

    public static class TagSeqCad073
    {
        public static List<TagSeqEntry073> Entries(Database db, TagSnapshot033 snap, Matrix3d wcsToUcs)
        {
            if (db == null || snap == null) throw new InvalidOperationException("Lựa chọn không hợp lệ.");
            var list = new List<TagSeqEntry073>();
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (var item in snap.items)
                {
                    var br = tr.GetObject(item.owner, OpenMode.ForRead) as BlockReference;
                    if (br == null) throw new InvalidOperationException("Block không hợp lệ: " + item.handle);
                    Point3d pt;
                    try
                    {
                        var ext = br.GeometricExtents;
                        pt = new Point3d(
                            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
                            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5,
                            (ext.MinPoint.Z + ext.MaxPoint.Z) * 0.5);
                    }
                    catch
                    {
                        pt = br.Position;
                    }
                    var transformed = pt.TransformBy(wcsToUcs);
                    list.Add(new TagSeqEntry073 {
                        id = item.handle,
                        name = item.blockName,
                        oldText = item.hasTag ? item.text : null,
                        x = transformed.X,
                        y = transformed.Y
                    });
                }
                tr.Commit();
            }
            return list;
        }

        public static TagApply033 Apply(Database db, TagSnapshot033 snap, IList<TagSeqRow073> plan, double height, Action<string> guard)
        {
            if (snap == null || plan == null || plan.Count != snap.items.Count)
                throw new InvalidOperationException("Bảng đánh số không khớp lựa chọn; bản vẽ không đổi.");
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in plan)
            {
                if (row == null || row.id == null || dict.ContainsKey(row.id))
                    throw new InvalidOperationException("Bảng đánh số có block trùng/thiếu; bản vẽ không đổi.");
                dict[row.id] = row.newText;
            }
            var texts = new List<string>();
            foreach (var item in snap.items)
            {
                string text;
                if (!dict.TryGetValue(item.handle, out text))
                    throw new InvalidOperationException("Block " + item.handle + " không có trong bảng đánh số; bản vẽ không đổi.");
                texts.Add(text);
            }
            return TagCad033.ApplyTexts(db, snap, texts, height, guard);
        }
    }
}
