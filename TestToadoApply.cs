using System;
using System.IO;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Tvxd.Meps;

namespace TestToadoApplyNative
{
    public class TestApply
    {
        [CommandMethod("TEST_APPLY_TOADO")]
        public void Run()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var sw = new StreamWriter(@"D:\TuanPa_AI\toado_apply_result.txt"))
            {
                sw.WriteLine("=== TESTING T-TOADO APPLY ON BE2 DAN59 ===");
                try
                {
                    var blockIds = new List<ObjectId>();
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                        foreach (ObjectId id in ms)
                        {
                            if (id.ObjectClass.DxfName.Equals("INSERT", StringComparison.OrdinalIgnoreCase))
                            {
                                var br = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                                if (br.Name == "BE2 DAN59")
                                {
                                    blockIds.Add(id);
                                    if (blockIds.Count >= 3) break; // test first 3 blocks
                                }
                            }
                        }
                        tr.Commit();
                    }

                    sw.WriteLine("Selected block count: " + blockIds.Count);

                    var settings = new ToadoSettings
                    {
                        AddName = true,
                        Vn2000 = true,
                        TwoLines = true,
                        Height = 2.5,
                        Distance = 15.0
                    };

                    var res = Toado100.Apply(db, blockIds, settings, null);
                    sw.WriteLine(string.Format("Apply Result: Created={0}, Updated={1}, Skipped={2}", res.created, res.updated, res.skipped));

                    // Now inspect created MLeaders in ModelSpace
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                        int mlCount = 0;
                        foreach (ObjectId id in ms)
                        {
                            if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                            {
                                mlCount++;
                                var ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);
                                Point3d arrowPt = ml.GetFirstVertex(0);
                                Point3d landingPt = ml.GetLastVertex(0);
                                string content = ml.MText != null ? ml.MText.Contents.Replace("\n", "\\P") : "";
                                sw.WriteLine(string.Format("  MLeader #{0}: Arrow={1}, Landing={2}, Text='{3}'",
                                    mlCount, arrowPt, landingPt, content));
                            }
                        }
                        sw.WriteLine("Total MLeaders in ModelSpace: " + mlCount);
                        tr.Commit();
                    }
                    sw.WriteLine("TEST STATUS: PASS");
                }
                catch (System.Exception ex)
                {
                    sw.WriteLine("ERROR: " + ex.ToString());
                }
            }
        }
    }
}
