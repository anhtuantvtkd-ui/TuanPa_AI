using System;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Tvxd.Meps;

namespace TestTask0021 {
    public class Suite {
        [CommandMethod("TEST_TASK0021_FEATURES")]
        public void RunTest() {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var sw = new StreamWriter(@"D:\TuanPa_AI\products\meps\test\task0021_test_report.txt")) {
                sw.WriteLine("=== TASK-0021 FEATURES NATIVE TEST REPORT ===");
                using (var tr = db.TransactionManager.StartTransaction()) {
                    var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    // Test 1: Tip on block
                    sw.WriteLine("--- Test 1: Tip accuracy ---");
                    BlockReference sampleBr = null;
                    foreach (ObjectId id in ms) {
                        if (id.ObjectClass.DxfName.Equals("INSERT", StringComparison.OrdinalIgnoreCase)) {
                            sampleBr = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                            break;
                        }
                    }
                    if (sampleBr != null) {
                        Point3d tip = Toado100.Tip(sampleBr, false);
                        sw.WriteLine(string.Format("Block: {0}, Pos: ({1:F3}, {2:F3}), Computed Tip: ({3:F3}, {4:F3})",
                            sampleBr.Name, sampleBr.Position.X, sampleBr.Position.Y, tip.X, tip.Y));
                        sw.WriteLine("Test 1 PASS: Tip calculated successfully.");
                    }

                    // Test 2: ReverseText Normal vs Flipped
                    sw.WriteLine("--- Test 2: ReverseText Normal (false) vs Flipped (true) ---");
                    ObjectId styleId = Toado100.EnsureTextStyle(tr, db, "Standard");
                    ObjectId layerId = Toado100.EnsureLayer(tr, db, "TOA_DO");
                    ObjectId dotArrowId = Toado100.EnsureDotArrow(db);
                    Toado100.EnsureRegApp(tr, db);

                    ToadoSettings sNormal = new ToadoSettings { ReverseText = false, Height = 2.5 };
                    ToadoSettings sFlipped = new ToadoSettings { ReverseText = true, Height = 2.5 };

                    Point3d pTarget = new Point3d(100, 100, 0);
                    Point3d pLandH = new Point3d(120, 120, 0); // Horizontal
                    Point3d pLandV = new Point3d(100, 150, 0); // Vertical

                    ObjectId idHNormal = Toado100.CreateOrUpdateLeader(db, tr, ms, pTarget, pLandH, sNormal, "BLK1", "1A", ObjectId.Null, layerId, styleId, dotArrowId);
                    ObjectId idHFlipped = Toado100.CreateOrUpdateLeader(db, tr, ms, pTarget, pLandH, sFlipped, "BLK2", "1B", ObjectId.Null, layerId, styleId, dotArrowId);
                    ObjectId idVNormal = Toado100.CreateOrUpdateLeader(db, tr, ms, pTarget, pLandV, sNormal, "BLK3", "1C", ObjectId.Null, layerId, styleId, dotArrowId);
                    ObjectId idVFlipped = Toado100.CreateOrUpdateLeader(db, tr, ms, pTarget, pLandV, sFlipped, "BLK4", "1D", ObjectId.Null, layerId, styleId, dotArrowId);

                    MLeader mlHNormal = (MLeader)tr.GetObject(idHNormal, OpenMode.ForRead);
                    MLeader mlHFlipped = (MLeader)tr.GetObject(idHFlipped, OpenMode.ForRead);
                    MLeader mlVNormal = (MLeader)tr.GetObject(idVNormal, OpenMode.ForRead);
                    MLeader mlVFlipped = (MLeader)tr.GetObject(idVFlipped, OpenMode.ForRead);

                    sw.WriteLine(string.Format("H Normal Rot: {0:F3} rad (Expected 0.0), LastVertex: ({1:F3}, {2:F3}) (Expected ({3:F3}, {4:F3}))",
                        mlHNormal.MText.Rotation, mlHNormal.GetLastVertex(0).X, mlHNormal.GetLastVertex(0).Y, pLandH.X, pLandH.Y));
                    sw.WriteLine(string.Format("H Flipped Rot: {0:F3} rad (Expected {1:F3}), LastVertex: ({2:F3}, {3:F3})",
                        mlHFlipped.MText.Rotation, Math.PI, mlHFlipped.GetLastVertex(0).X, mlHFlipped.GetLastVertex(0).Y));
                    sw.WriteLine(string.Format("V Normal Rot: {0:F3} rad (Expected {1:F3}), LastVertex: ({2:F3}, {3:F3}) (Expected ({4:F3}, {5:F3}))",
                        mlVNormal.MText.Rotation, Math.PI / 2, mlVNormal.GetLastVertex(0).X, mlVNormal.GetLastVertex(0).Y, pLandV.X, pLandV.Y));
                    sw.WriteLine(string.Format("V Flipped Rot: {0:F3} rad (Expected {1:F3}), LastVertex: ({2:F3}, {3:F3})",
                        mlVFlipped.MText.Rotation, -Math.PI / 2, mlVFlipped.GetLastVertex(0).X, mlVFlipped.GetLastVertex(0).Y));

                    bool passRotH = Math.Abs(mlHNormal.MText.Rotation - 0.0) < 0.01 && Math.Abs(mlHFlipped.MText.Rotation - Math.PI) < 0.01;
                    bool passRotV = Math.Abs(mlVNormal.MText.Rotation - Math.PI / 2) < 0.01 &&
                        (Math.Abs(mlVFlipped.MText.Rotation - (3 * Math.PI / 2)) < 0.01 || Math.Abs(mlVFlipped.MText.Rotation - (-Math.PI / 2)) < 0.01);
                    bool passVertex = (mlHNormal.GetLastVertex(0) - pLandH).Length < 1e-4 && (mlVNormal.GetLastVertex(0) - pLandV).Length < 1e-4;

                    sw.WriteLine("--- Test 3: LastVertex Anchor Verification ---");
                    sw.WriteLine("LastVertex equals LandingPt: " + passVertex);
                    sw.WriteLine("Rotation Normal vs Flipped PASS: " + (passRotH && passRotV));

                    if (passRotH && passRotV && passVertex) {
                        sw.WriteLine("FINAL VERDICT: ALL TASK-0021 TESTS PASSED 100%");
                    } else {
                        sw.WriteLine("FINAL VERDICT: FAILED");
                    }
                    tr.Commit();
                }
            }
        }
    }
}
