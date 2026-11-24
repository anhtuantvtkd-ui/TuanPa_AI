using System;
using System.IO;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class TestRotationAndRefresh {
        [CommandMethod("TEST_ROT_REFRESH")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n================================================================================");
            ed.WriteMessage("\n   COMPREHENSIVE TEST: ROTATION 360 DEG + REFRESH STABILITY");
            ed.WriteMessage("\n================================================================================");

            ObjectId dotArrowId = Toado100.EnsureDotArrow(db);
            ToadoSettings s = new ToadoSettings();
            s.Height = 2.5;
            s.ArrowSize = 1.5;
            s.CircleArrow = (dotArrowId != ObjectId.Null);
            s.UnderlineBetween = true;
            s.Vn2000 = true;
            s.Decimals = 3;
            s.Layer = "TOA_DO";

            Point3d targetPt = new Point3d(406831.99, 2369223.4, 0); // Using real coordinates from DWG
            double radius = 30.0;
            double[] angles = new double[] { 0, 45, 90, 135, 180, 225, 270, 315 };

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                Toado100.EnsureRegApp(tr, db);
                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                ObjectId styleId = db.Textstyle;

                foreach (double deg in angles) {
                    double rad = deg * Math.PI / 180.0;
                    double dx = radius * Math.Cos(rad);
                    double dy = radius * Math.Sin(rad);
                    Point3d landingPt = new Point3d(targetPt.X + dx, targetPt.Y + dy, targetPt.Z);

                    ed.WriteMessage(string.Format("\n\n>>> TESTING ANGLE {0}° <<<", deg));
                    ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, "DPT", "HANDLE_" + deg, ObjectId.Null, layerId, styleId, dotArrowId);
                    MLeader ml = (MLeader)tr.GetObject(mlId, OpenMode.ForWrite);

                    Point3d v0 = ml.GetFirstVertex(0);
                    Point3d vLast = ml.GetLastVertex(0);
                    Point3d tLoc = ml.TextLocation;
                    Extents3d extBefore = ml.GeometricExtents;

                    ed.WriteMessage(string.Format("\n  [INITIAL] V0=({0:0.##},{1:0.##}), VLast=({2:0.##},{3:0.##}), TextLoc=({4:0.##},{5:0.##})",
                        v0.X, v0.Y, vLast.X, vLast.Y, tLoc.X, tLoc.Y));

                    // Now simulate REFRESH
                    Point3d refreshLandingPt;
                    try {
                        refreshLandingPt = ml.GetLastVertex(0);
                    } catch {
                        refreshLandingPt = ml.TextLocation;
                    }

                    Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, refreshLandingPt, s, "DPT", "HANDLE_" + deg, mlId, layerId, styleId, dotArrowId);
                    Point3d v0After = ml.GetFirstVertex(0);
                    Point3d vLastAfter = ml.GetLastVertex(0);
                    Point3d tLocAfter = ml.TextLocation;
                    Extents3d extAfter = ml.GeometricExtents;

                    ed.WriteMessage(string.Format("\n  [AFTER REFRESH 1] V0=({0:0.##},{1:0.##}), VLast=({2:0.##},{3:0.##}), TextLoc=({4:0.##},{5:0.##})",
                        v0After.X, v0After.Y, vLastAfter.X, vLastAfter.Y, tLocAfter.X, tLocAfter.Y));

                    double vLastShift = vLastAfter.DistanceTo(vLast);
                    double tLocShift = tLocAfter.DistanceTo(tLoc);
                    ed.WriteMessage(string.Format("\n  [DRIFT CHECK] VLast Shift={0:0.##}, TextLoc Shift={1:0.##}", vLastShift, tLocShift));

                    if (vLastShift > 0.1 || tLocShift > 0.1) {
                        ed.WriteMessage(string.Format("\n  [BUG CONFIRMED] Leader drifted after Refresh by {0:0.##} units!", Math.Max(vLastShift, tLocShift)));
                    } else {
                        ed.WriteMessage("\n  [STABLE] No drift after Refresh.");
                    }
                }
                tr.Commit();
            }
        }
    }
}
