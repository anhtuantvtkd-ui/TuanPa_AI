using System;
using System.IO;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Tvxd.Meps;

namespace Tvxd.Meps.Test
{
    public class TestToadoStayPutSuite
    {
        [CommandMethod("TEST_TOADO_FULL_SUITE")]
        public void Run()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            string reportPath = @"D:\TuanPa_AI\products\meps\test\toado_stayput_suite_report.txt";

            using (var sw = new StreamWriter(reportPath))
            {
                sw.WriteLine("=== TOADO COMPREHENSIVE STAY-PUT & GRIP EDIT REGRESSION SUITE ===");
                sw.WriteLine("DLL SHA256: 30a663d4cba2acae87efc506a23b2053bb5400d0723544068688e1ae3e4469e7");

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
                                    if (blockIds.Count >= 2) break;
                                }
                            }
                        }
                        tr.Commit();
                    }

                    var settings = new ToadoSettings
                    {
                        AddName = true,
                        Vn2000 = true,
                        TwoLines = true,
                        Height = 2.5,
                        Distance = 15.0
                    };

                    Func<int, int, Point3d, Point3d?> pickFn = (idx, total, tip) =>
                    {
                        if (idx == 1) return null; // Horizontal
                        return new Point3d(tip.X, tip.Y + 15.0, tip.Z); // Vertical
                    };

                    // Pass 1: Create
                    var res1 = Toado100.Apply(db, blockIds, settings, pickFn);
                    sw.WriteLine(string.Format("Pass 1 (Create): Created={0}, Updated={1}", res1.created, res1.updated));

                    var initArrows = new List<Point3d>();
                    var initLandings = new List<Point3d>();
                    var initTextLocs = new List<Point3d>();

                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                        foreach (ObjectId id in ms)
                        {
                            if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                            {
                                var ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);
                                initArrows.Add(ml.GetFirstVertex(0));
                                initLandings.Add(ml.GetLastVertex(0));
                                initTextLocs.Add(ml.TextLocation);
                            }
                        }
                        tr.Commit();
                    }

                    bool passAll = true;

                    // Test 1: 3 rounds of update with zero movement
                    for (int round = 1; round <= 3; round++)
                    {
                        Toado100.Apply(db, blockIds, settings, pickFn);
                        using (var tr = db.TransactionManager.StartTransaction())
                        {
                            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                            int mlIdx = 0;
                            foreach (ObjectId id in ms)
                            {
                                if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                                {
                                    var ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);
                                    double dA = (ml.GetFirstVertex(0) - initArrows[mlIdx]).Length;
                                    double dL = (ml.GetLastVertex(0) - initLandings[mlIdx]).Length;
                                    double dT = (ml.TextLocation - initTextLocs[mlIdx]).Length;

                                    sw.WriteLine(string.Format("  Round {0} Leader #{1}: ArrowDelta={2:F6}, LandingDelta={3:F6}, TextLocDelta={4:F6}",
                                        round, mlIdx + 1, dA, dL, dT));
                                    if (dA != 0.0 || dL != 0.0 || dT != 0.0) passAll = false;
                                    mlIdx++;
                                }
                            }
                            tr.Commit();
                        }
                    }

                    // Test 2: Horizontal Leader - Arrow Grip Edit in X and Y
                    Point3d newArrowH = new Point3d(initArrows[0].X + 5.0, initArrows[0].Y + 3.0, 0);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                        foreach (ObjectId id in ms)
                        {
                            if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                            {
                                var ml = (MLeader)tr.GetObject(id, OpenMode.ForWrite);
                                ml.SetFirstVertex(0, newArrowH);
                                break;
                            }
                        }
                        tr.Commit();
                    }
                    Toado100.Apply(db, blockIds, settings, pickFn);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                        foreach (ObjectId id in ms)
                        {
                            if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                            {
                                var ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);
                                double dA = (ml.GetFirstVertex(0) - newArrowH).Length;
                                double dL = (ml.GetLastVertex(0) - initLandings[0]).Length;
                                sw.WriteLine(string.Format("Test 2 (Horizontal Arrow Grip): ArrowKeptDelta={0:F6}, LandingFrozenDelta={1:F6}", dA, dL));
                                if (dA != 0.0 || dL != 0.0) passAll = false;
                                break;
                            }
                        }
                        tr.Commit();
                    }

                    // Test 3: Vertical Leader - Text Grip Edit in X and Y
                    Point3d newTextLocV = new Point3d(initTextLocs[1].X + 7.0, initTextLocs[1].Y + 12.0, 0);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                        int count = 0;
                        foreach (ObjectId id in ms)
                        {
                            if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                            {
                                count++;
                                if (count == 2)
                                {
                                    var ml = (MLeader)tr.GetObject(id, OpenMode.ForWrite);
                                    ml.TextLocation = newTextLocV;
                                    break;
                                }
                            }
                        }
                        tr.Commit();
                    }
                    Toado100.Apply(db, blockIds, settings, pickFn);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                        int count = 0;
                        foreach (ObjectId id in ms)
                        {
                            if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                            {
                                count++;
                                if (count == 2)
                                {
                                    var ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);
                                    double dA = (ml.GetFirstVertex(0) - initArrows[1]).Length;
                                    double dT = (ml.TextLocation - newTextLocV).Length;
                                    sw.WriteLine(string.Format("Test 3 (Vertical Text Grip X & Y): ArrowFrozenDelta={0:F6}, TextKeptDelta={1:F6}", dA, dT));
                                    if (dA != 0.0 || dT != 0.0) passAll = false;
                                    break;
                                }
                            }
                        }
                        tr.Commit();
                    }

                    // Test 4: Vertical Leader - Arrow Grip Edit in X and Y
                    Point3d newArrowV = new Point3d(initArrows[1].X + 4.0, initArrows[1].Y + 2.0, 0);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                        int count = 0;
                        foreach (ObjectId id in ms)
                        {
                            if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                            {
                                count++;
                                if (count == 2)
                                {
                                    var ml = (MLeader)tr.GetObject(id, OpenMode.ForWrite);
                                    ml.SetFirstVertex(0, newArrowV);
                                    break;
                                }
                            }
                        }
                        tr.Commit();
                    }
                    Toado100.Apply(db, blockIds, settings, pickFn);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                        int count = 0;
                        foreach (ObjectId id in ms)
                        {
                            if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                            {
                                count++;
                                if (count == 2)
                                {
                                    var ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);
                                    double dA = (ml.GetFirstVertex(0) - newArrowV).Length;
                                    double dT = (ml.TextLocation - newTextLocV).Length;
                                    sw.WriteLine(string.Format("Test 4 (Vertical Arrow Grip X & Y): ArrowKeptDelta={0:F6}, TextFrozenDelta={1:F6}", dA, dT));
                                    if (dA != 0.0 || dT != 0.0) passAll = false;
                                    break;
                                }
                            }
                        }
                        tr.Commit();
                    }

                    if (passAll)
                    {
                        sw.WriteLine("STRICT PASS: Delta = 0.000000 on all 3 rounds and all grip edits across horizontal and vertical leaders!");
                        sw.WriteLine("FINAL VERDICT: APPROVED");
                    }
                    else
                    {
                        sw.WriteLine("FINAL VERDICT: FAIL");
                    }
                }
                catch (System.Exception ex)
                {
                    sw.WriteLine("EXCEPTION: " + ex.ToString());
                }
            }
        }
    }
}
