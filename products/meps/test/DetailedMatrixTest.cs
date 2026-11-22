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
    public class DetailedMatrixTest {
        [CommandMethod("DETAILED_MATRIX_TEST")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n================================================================================");
            ed.WriteMessage("\n   COMPREHENSIVE CAD MATRIX TEST ON REAL DWG: " + Path.GetFileName(doc.Name));
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

            // Real target coordinate from DWG (DPT block)
            Point3d targetPt = new Point3d(406831.99, 2369223.4, 0);
            double radius = 30.0;
            double[] angles = new double[] { 0, 15, 45, 75, 90, 105, 135, 165, 180, 195, 225, 255, 270, 285, 315, 345 };

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                Toado100.EnsureRegApp(tr, db);
                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                ObjectId styleId = db.Textstyle;

                int testIdx = 0;
                foreach (double deg in angles) {
                    testIdx++;
                    double rad = deg * Math.PI / 180.0;
                    double dx = radius * Math.Cos(rad);
                    double dy = radius * Math.Sin(rad);
                    Point3d landingPt = new Point3d(targetPt.X + dx, targetPt.Y + dy, targetPt.Z);
                    bool isVertical = Math.Abs(dx) < 0.25 * Math.Abs(dy) && Math.Abs(dy) > 1.0;

                    ed.WriteMessage(string.Format("\n\n----------------------------------------------------------------"));
                    ed.WriteMessage(string.Format("\n[CASE {0:D2}] ANGLE = {1:0}° | Type = {2} | dx={3:0.##}, dy={4:0.##}",
                        testIdx, deg, isVertical ? "VERTICAL" : "HORIZONTAL/SLANTED", dx, dy));

                    ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, "DPT", "HANDLE_" + deg, ObjectId.Null, layerId, styleId, dotArrowId);
                    MLeader ml = (MLeader)tr.GetObject(mlId, OpenMode.ForWrite);

                    // 1. Target Attachment Check
                    Point3d v0 = ml.GetFirstVertex(0);
                    double arrowDist = v0.DistanceTo(targetPt);
                    bool arrowOk = arrowDist < 0.001;
                    ObjectId arrowSym = ml.ArrowSymbolId;
                    bool arrowSymOk = (s.CircleArrow && arrowSym != ObjectId.Null);

                    // 2. Explode geometry to check text and lines
                    DBObjectCollection col = new DBObjectCollection();
                    try { ml.Explode(col); } catch {}
                    MText mtext = null;
                    List<Polyline> plines = new List<Polyline>();
                    List<Line> lines = new List<Line>();
                    foreach (DBObject o in col) {
                        if (o is MText) mtext = (MText)o;
                        else if (o is Polyline) plines.Add((Polyline)o);
                        else if (o is Line) lines.Add((Line)o);
                    }

                    // 3. Text & Line Alignment Check
                    bool geomOk = false;
                    string geomNote = "";
                    if (mtext != null) {
                        if (isVertical) {
                            DBObjectCollection parts = new DBObjectCollection();
                            try { mtext.Explode(parts); } catch {}
                            DBText dtX = null, dtY = null;
                            foreach (DBObject p in parts) {
                                if (p is DBText) {
                                    DBText dt = (DBText)p;
                                    if (dt.TextString.StartsWith("X=")) dtX = dt;
                                    else if (dt.TextString.StartsWith("Y=")) dtY = dt;
                                }
                            }
                            if (dtX != null && dtY != null) {
                                double xMaxX = dtX.GeometricExtents.MaxPoint.X;
                                double xMinY = dtY.GeometricExtents.MinPoint.X;
                                double stemX = targetPt.X;
                                bool xLeft = (xMaxX < stemX);
                                bool yRight = (xMinY > stemX);
                                geomOk = xLeft && yRight;
                                geomNote = string.Format("StemX={0:0.##}, LineX_Max={1:0.##} (gap={2:0.##}), LineY_Min={3:0.##} (gap={4:0.##}) => X_Left={5}, Y_Right={6}",
                                    stemX, xMaxX, stemX - xMaxX, xMinY, xMinY - stemX, xLeft, yRight);
                            } else {
                                geomNote = "Could not explode MText into DBText lines";
                            }
                        } else {
                            double landY = landingPt.Y;
                            double tTop = mtext.GeometricExtents.MaxPoint.Y;
                            double tBot = mtext.GeometricExtents.MinPoint.Y;
                            geomOk = (tTop > landY && tBot < landY);
                            geomNote = string.Format("LandingY={0:0.##}, TextBounds=[{1:0.##}, {2:0.##}] => LineBetween={3}",
                                landY, tBot, tTop, geomOk);
                        }
                    }

                    // 4. Refresh Stability Check
                    Point3d vLastBefore = ml.GetLastVertex(0);
                    Point3d tLocBefore = ml.TextLocation;

                    Point3d refreshLandingPt;
                    try {
                        refreshLandingPt = ml.GetLastVertex(0);
                    } catch {
                        refreshLandingPt = ml.TextLocation;
                    }
                    Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, refreshLandingPt, s, "DPT", "HANDLE_" + deg, mlId, layerId, styleId, dotArrowId);

                    Point3d vLastAfter = ml.GetLastVertex(0);
                    Point3d tLocAfter = ml.TextLocation;
                    double drift = Math.Max(vLastAfter.DistanceTo(vLastBefore), tLocAfter.DistanceTo(tLocBefore));
                    bool refreshOk = (drift < 0.05);

                    // Print Evaluation
                    ed.WriteMessage(string.Format("\n  Arrow Attachment: dist={0:0.###} => {1} (Symbol={2})", arrowDist, arrowOk ? "PASS" : "FAIL", arrowSymOk ? "DOT" : "NONE"));
                    ed.WriteMessage(string.Format("\n  Geometry Layout : {0} => {1}", geomNote, geomOk ? "PASS" : "FAIL"));
                    ed.WriteMessage(string.Format("\n  Refresh Drift   : drift={0:0.##} => {1}", drift, refreshOk ? "STABLE" : "FAIL (DRIFTED)"));

                    string overall = (arrowOk && arrowSymOk && geomOk && refreshOk) ? "PASS" : "FAIL";
                    ed.WriteMessage(string.Format("\n  ==> VERDICT: {0}", overall));
                }
                tr.Commit();
            }
        }
    }
}
