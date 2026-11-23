// Decompiled source of Tvxd.Meps.TagSeqCad073 for TASK-0018
// Provided by Gemini to Codex for integrating GeometricExtents center calculation
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Tvxd.Meps
{
    // Minimal contract stubs so TagSeqPart.cs / TagSeqCad073 can compile with csc
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

    public class TagChannel033
    {
    }

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
            throw new NotImplementedException("Handled by base assembly");
        }
    }

    public static class TagSeqCad073
    {
        public static List<TagSeqEntry073> Entries(Database db, TagSnapshot033 snap, Matrix3d wcsToUcs)
        {
            if (db == null || snap == null)
            {
                throw new InvalidOperationException("Lựa chọn không hợp lệ.");
            }

            var list = new List<TagSeqEntry073>();
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (var item in snap.items)
                {
                    var br = tr.GetObject(item.owner, OpenMode.ForRead) as BlockReference;
                    Point3d pt;
                    try
                    {
                        var ext = br.GeometricExtents;
                        pt = new Point3d(
                            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
                            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5,
                            (ext.MinPoint.Z + ext.MaxPoint.Z) * 0.5
                        );
                    }
                    catch
                    {
                        pt = br.Position;
                    }

                    var transformed = pt.TransformBy(wcsToUcs);
                    var entry = new TagSeqEntry073
                    {
                        id = item.handle,
                        name = item.blockName,
                        oldText = item.hasTag ? item.tagValue : null,
                        x = transformed.X,
                        y = transformed.Y
                    };
                    list.Add(entry);
                }
                tr.Commit();
            }
            return list;
        }

        public static TagApply033 Apply(Database db, TagSnapshot033 snap, IList<TagSeqRow073> plan, double height, Action<string> guard)
        {
            if (snap == null || plan == null || plan.Count != snap.items.Count)
            {
                throw new InvalidOperationException("Bảng đánh số không khớp lựa chọn; bản vẽ không đổi.");
            }

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in plan)
            {
                if (row == null || row.id == null || dict.ContainsKey(row.id))
                {
                    throw new InvalidOperationException("Bảng đánh số có block trùng/thiếu; bản vẽ không đổi.");
                }
                dict[row.id] = row.newText;
            }

            var list = new List<string>();
            foreach (var item in snap.items)
            {
                string text;
                if (!dict.TryGetValue(item.handle, out text))
                {
                    throw new InvalidOperationException("Block " + item.handle + " không có trong bảng đánh số; bản vẽ không đổi.");
                }
                list.Add(text);
            }

            return TagCad033.ApplyTexts(db, snap, list, height, guard);
        }
    }
}
