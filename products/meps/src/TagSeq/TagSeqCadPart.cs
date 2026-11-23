// Codex integration for TASK-0018: derive ordering coordinates from geometry, not block base point.
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Tvxd.Meps
{
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
                        oldText = item.hasTag ? item.tagValue : null,
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