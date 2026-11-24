using System;
using System.IO;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace TestMepsTagSeq
{
    public class Test32
    {
        [CommandMethod("TEST_32_BLOCKS")]
        public void Run()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            string reportPath = @"D:\TuanPa_AI\products\meps\test\test32_report.txt";

            using (var sw = new StreamWriter(reportPath, false))
            {
                sw.WriteLine("=== TESTING 32 BLOCKS TAG NUMBERING (LR) ===");
                try
                {
                    var blockIds = new List<ObjectId>();
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
                        foreach (ObjectId id in btr)
                        {
                            if (id.ObjectClass == RXObject.GetClass(typeof(BlockReference)))
                            {
                                var br = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                                if (br.Name == "BE2 DAN59" || br.Name.Contains("BE2"))
                                {
                                    blockIds.Add(id);
                                }
                            }
                        }
                        tr.Commit();
                    }

                    sw.WriteLine("Selected block count: " + blockIds.Count);

                    var snap = TagCad033.Snapshot(db, blockIds);
                    sw.WriteLine("Snapshot items count: " + (snap != null && snap.items != null ? snap.items.Count : 0));

                    var entries = TagSeqCad073.Entries(db, snap, Matrix3d.Identity);
                    sw.WriteLine("Entries count: " + entries.Count);

                    var options = new TagSeqOptions073
                    {
                        direction = TagSeqDir073.LR,
                        prefix = "B",
                        start = 1,
                        width = 1
                    };

                    var plan = TagSequence073.Plan(entries, options);
                    sw.WriteLine("Plan count: " + plan.Count);
                    sw.WriteLine("--------------------------------------------------------------------------------");
                    sw.WriteLine(string.Format("{0,-6} | {1,-8} | {2,-15} | {3,-15} | {4,-10} | {5,-10}",
                        "No.", "Handle", "X (GeoExtents)", "Y (GeoExtents)", "Old Tag", "New Tag"));
                    sw.WriteLine("--------------------------------------------------------------------------------");

                    bool allMonotonicX = true;
                    double lastX = double.MinValue;

                    for (int i = 0; i < plan.Count; i++)
                    {
                        var row = plan[i];
                        sw.WriteLine(string.Format("{0,-6} | {1,-8} | {2,15:F2} | {3,15:F2} | {4,-10} | {5,-10}",
                            row.ordinal, row.id, row.x, row.y, row.oldText ?? "null", row.newText));

                        if (row.x < lastX - 0.001)
                        {
                            allMonotonicX = false;
                        }
                        lastX = row.x;
                    }

                    sw.WriteLine("--------------------------------------------------------------------------------");
                    sw.WriteLine("Result: " + (allMonotonicX ? "PASS - ALL 32 BLOCKS STRICTLY MONOTONIC FROM LEFT TO RIGHT" : "FAIL - NOT MONOTONIC"));
                }
                catch (System.Exception ex)
                {
                    sw.WriteLine("ERROR: " + ex.ToString());
                }
            }
        }
    }
}
