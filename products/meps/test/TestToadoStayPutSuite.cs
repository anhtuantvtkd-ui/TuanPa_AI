using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Tvxd.Meps;

namespace Tvxd.Meps.Test
{
    public class TestToadoStayPutSuite
    {
        private static string GetLoadedDllSha256()
        {
            string loc = typeof(Toado100).Assembly.Location;
            using (var sha = SHA256.Create())
            {
                using (var fs = File.OpenRead(loc))
                {
                    byte[] bytes = sha.ComputeHash(fs);
                    return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        [CommandMethod("TEST_TOADO_FULL_SUITE")]
        public void Run()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            string reportPath = @"D:\TuanPa_AI\products\meps\test\toado_stayput_suite_report.txt";

            using (var sw = new StreamWriter(reportPath))
            {
                string dllHash = GetLoadedDllSha256();
                sw.WriteLine("=== TOADO COMPREHENSIVE STAY-PUT & GRIP EDIT REGRESSION SUITE ===");
                sw.WriteLine("MEASURED LOADED DLL SHA256: " + dllHash);

                try
                {
                    var blockIds = new List<ObjectId>();
                    var priorMLeaders = new HashSet<ObjectId>();

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
                            else if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                            {
                                priorMLeaders.Add(id);
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

                    // Step 1: Create
                    var res1 = Toado100.Apply(db, blockIds, settings, pickFn);
                    sw.WriteLine(string.Format("Pass 1 (Create): Created={0}, Updated={1}", res1.created, res1.updated));

                    // Identify the exact created subjects by tracking newly added ObjectIds
                    ObjectId idH = ObjectId.Null;
                    ObjectId idV = ObjectId.Null;

                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                        foreach (ObjectId id in ms)
                        {
                            if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!priorMLeaders.Contains(id))
                                {
                                    var ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);
                                    bool isVert = (ml.MText != null && Math.Abs(ml.MText.Rotation - Math.PI / 2) < 0.1) || (!ml.EnableDogleg && !ml.EnableLanding);
                                    if (!isVert && idH == ObjectId.Null) idH = id;
                                    else if (isVert && idV == ObjectId.Null) idV = id;
                                }
                            }
                        }
                        tr.Commit();
                    }

                    sw.WriteLine(string.Format("Identified subjects: idH={0}, idV={1}", idH, idV));

                    Point3d initArrowH, initLandingH, initTextH;
                    Point3d initArrowV, initLandingV, initTextV;

                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlH = (MLeader)tr.GetObject(idH, OpenMode.ForRead);
                        var mlV = (MLeader)tr.GetObject(idV, OpenMode.ForRead);

                        initArrowH = mlH.GetFirstVertex(0);
                        initLandingH = mlH.GetLastVertex(0);
                        initTextH = mlH.TextLocation;

                        initArrowV = mlV.GetFirstVertex(0);
                        initLandingV = mlV.GetLastVertex(0);
                        initTextV = mlV.TextLocation;

                        tr.Commit();
                    }

                    bool passAll = true;

                    // Test 1: 3 rounds of update with zero movement
                    for (int round = 1; round <= 3; round++)
                    {
                        Toado100.Apply(db, blockIds, settings, pickFn);
                        using (var tr = db.TransactionManager.StartTransaction())
                        {
                            var mlH = (MLeader)tr.GetObject(idH, OpenMode.ForRead);
                            var mlV = (MLeader)tr.GetObject(idV, OpenMode.ForRead);

                            double dAH = (mlH.GetFirstVertex(0) - initArrowH).Length;
                            double dLH = (mlH.GetLastVertex(0) - initLandingH).Length;
                            double dTH = (mlH.TextLocation - initTextH).Length;

                            double dAV = (mlV.GetFirstVertex(0) - initArrowV).Length;
                            double dLV = (mlV.GetLastVertex(0) - initLandingV).Length;
                            double dTV = (mlV.TextLocation - initTextV).Length;

                            sw.WriteLine(string.Format("  Round {0} H: ArrowDelta={1:F6}, LandingDelta={2:F6}, TextDelta={3:F6}", round, dAH, dLH, dTH));
                            sw.WriteLine(string.Format("  Round {0} V: ArrowDelta={1:F6}, LandingDelta={2:F6}, TextDelta={3:F6}", round, dAV, dLV, dTV));

                            if (dAH != 0.0 || dLH != 0.0 || dTH != 0.0 || dAV != 0.0 || dLV != 0.0 || dTV != 0.0) passAll = false;
                            tr.Commit();
                        }
                    }

                    // Test 2: Horizontal Leader - Arrow Grip Edit in X and Y
                    Point3d newArrowH = new Point3d(initArrowH.X + 5.0, initArrowH.Y + 3.0, 0);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlH = (MLeader)tr.GetObject(idH, OpenMode.ForWrite);
                        mlH.SetFirstVertex(0, newArrowH);
                        tr.Commit();
                    }
                    Toado100.Apply(db, blockIds, settings, pickFn);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlH = (MLeader)tr.GetObject(idH, OpenMode.ForRead);
                        double dA = (mlH.GetFirstVertex(0) - newArrowH).Length;
                        double dL = (mlH.GetLastVertex(0) - initLandingH).Length;
                        sw.WriteLine(string.Format("Test 2 (Horizontal Arrow Grip): ArrowKeptDelta={0:F6}, LandingFrozenDelta={1:F6}", dA, dL));
                        if (dA != 0.0 || dL != 0.0) passAll = false;
                        tr.Commit();
                    }

                    // Test 3: Horizontal Leader - Text/Landing Grip Edit in X and Y
                    Point3d newLandingH = new Point3d(initLandingH.X + 8.0, initLandingH.Y + 4.0, 0);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlH = (MLeader)tr.GetObject(idH, OpenMode.ForWrite);
                        mlH.SetLastVertex(0, newLandingH);
                        tr.Commit();
                    }
                    Toado100.Apply(db, blockIds, settings, pickFn);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlH = (MLeader)tr.GetObject(idH, OpenMode.ForRead);
                        double dA = (mlH.GetFirstVertex(0) - newArrowH).Length;
                        double dL = (mlH.GetLastVertex(0) - newLandingH).Length;
                        sw.WriteLine(string.Format("Test 3 (Horizontal Text/Landing Grip): ArrowFrozenDelta={0:F6}, LandingKeptDelta={1:F6}", dA, dL));
                        if (dA != 0.0 || dL != 0.0) passAll = false;
                        tr.Commit();
                    }

                    // Test 4: Threshold-Crossing Grip Edit on Horizontal Leader (dx < 0.25 dy)
                    // Move landing such that dx is small (0.5) and dy is large (20.0). Verify it DOES NOT flip to vertical!
                    Point3d thresholdLandingH = new Point3d(newArrowH.X + 0.5, newArrowH.Y + 20.0, 0);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlH = (MLeader)tr.GetObject(idH, OpenMode.ForWrite);
                        mlH.SetLastVertex(0, thresholdLandingH);
                        tr.Commit();
                    }
                    Toado100.Apply(db, blockIds, settings, pickFn);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlH = (MLeader)tr.GetObject(idH, OpenMode.ForRead);
                        bool isStillHorizontal = (mlH.MText == null || Math.Abs(mlH.MText.Rotation) < 0.01) && mlH.EnableDogleg;
                        double dL = (mlH.GetLastVertex(0) - thresholdLandingH).Length;
                        sw.WriteLine(string.Format("Test 4 (Threshold Crossing): RemainsHorizontal={0}, LandingKeptDelta={1:F6}", isStillHorizontal, dL));
                        if (!isStillHorizontal || dL != 0.0) passAll = false;
                        tr.Commit();
                    }

                    // Test 5: Vertical Leader - Text Grip Edit in X and Y
                    Point3d newTextLocV = new Point3d(initTextV.X + 7.0, initTextV.Y + 12.0, 0);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlV = (MLeader)tr.GetObject(idV, OpenMode.ForWrite);
                        mlV.TextLocation = newTextLocV;
                        tr.Commit();
                    }
                    Toado100.Apply(db, blockIds, settings, pickFn);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlV = (MLeader)tr.GetObject(idV, OpenMode.ForRead);
                        double dA = (mlV.GetFirstVertex(0) - initArrowV).Length;
                        double dT = (mlV.TextLocation - newTextLocV).Length;
                        sw.WriteLine(string.Format("Test 5 (Vertical Text Grip X & Y): ArrowFrozenDelta={0:F6}, TextKeptDelta={1:F6}", dA, dT));
                        if (dA != 0.0 || dT != 0.0) passAll = false;
                        tr.Commit();
                    }

                    // Test 6: Vertical Leader - Arrow Grip Edit in X and Y
                    Point3d newArrowV = new Point3d(initArrowV.X + 4.0, initArrowV.Y + 2.0, 0);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlV = (MLeader)tr.GetObject(idV, OpenMode.ForWrite);
                        mlV.SetFirstVertex(0, newArrowV);
                        tr.Commit();
                    }
                    Toado100.Apply(db, blockIds, settings, pickFn);
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var mlV = (MLeader)tr.GetObject(idV, OpenMode.ForRead);
                        double dA = (mlV.GetFirstVertex(0) - newArrowV).Length;
                        double dT = (mlV.TextLocation - newTextLocV).Length;
                        sw.WriteLine(string.Format("Test 6 (Vertical Arrow Grip X & Y): ArrowKeptDelta={0:F6}, TextFrozenDelta={1:F6}", dA, dT));
                        if (dA != 0.0 || dT != 0.0) passAll = false;
                        tr.Commit();
                    }

                    if (passAll)
                    {
                        sw.WriteLine("STRICT PASS: Delta = 0.000000 on all rounds, grip edits, and threshold crossing!");
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
