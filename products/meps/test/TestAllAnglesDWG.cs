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
    public class TestAllAnglesDWG {
        [CommandMethod("TEST_ALL_ANGLES")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n================================================================================");
            ed.WriteMessage("\n   SYSTEMATIC 360-DEGREE ROTATION TEST ON REAL DWG: " + doc.Name);
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

            // Find a DPT block in the drawing
            Point3d targetPt = Point3d.Origin;
            string blockName = "DPT";
            string blockHandle = "";

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId id in ms) {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj is BlockReference) {
                        BlockReference br = (BlockReference)obj;
                        if (br.Name == "DPT") {
                            targetPt = br.Position;
                            blockName = Toado100.NameOf(tr, br);
                            blockHandle = br.Handle.ToString();
                            break;
                        }
                    }
                }
                tr.Commit();
            }

            ed.WriteMessage(string.Format("\nBase Block: '{0}' at ({1:0.##},{2:0.##})\n", blockName, targetPt.X, targetPt.Y));

            // Angles to test (degrees): 0, 15, 45, 75, 90, 105, 135, 165, 180, 195, 225, 255, 270, 285, 315, 345
            double[] angles = new double[] { 0, 15, 45, 75, 90, 105, 135, 165, 180, 195, 225, 255, 270, 285, 315, 345 };
            double radius = 30.0;

            int totalTests = 0;
            int passedTests = 0;
            int failedTests = 0;

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                Toado100.EnsureRegApp(tr, db);
                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                ObjectId styleId = db.Textstyle;

                foreach (double deg in angles) {
                    totalTests++;
                    double rad = deg * Math.PI / 180.0;
                    double dx = radius * Math.Cos(rad);
                    double dy = radius * Math.Sin(rad);
                    Point3d landingPt = new Point3d(targetPt.X + dx, targetPt.Y + dy, targetPt.Z);

                    bool isVertical = Math.Abs(dx) < 0.25 * Math.Abs(dy) && Math.Abs(dy) > 1.0;

                    ed.WriteMessage(string.Format("\n------------------------------------------------------------"));
                    ed.WriteMessage(string.Format("\nANGLE: {0:0}° (rad={1:0.###}) | dx={2:0.##}, dy={3:0.##} | isVertical={4}",
                        deg, rad, dx, dy, isVertical));

                    ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, blockName, blockHandle, ObjectId.Null, layerId, styleId, dotArrowId);
                    MLeader ml = (MLeader)tr.GetObject(mlId, OpenMode.ForRead);

                    // Check leader lines count and vertices
                    int ldrCount = ml.LeaderCount;
                    int lineCount = ml.LeaderLineCount;

                    // Explode to examine actual AutoCAD rendered geometry
                    DBObjectCollection col = new DBObjectCollection();
                    try { ml.Explode(col); } catch (System.Exception ex) {
                        ed.WriteMessage("\n  [ERROR] Explode failed: " + ex.Message);
                    }

                    MText mtext = null;
                    List<Polyline> polylines = new List<Polyline>();
                    List<Line> lines = new List<Line>();

                    foreach (DBObject o in col) {
                        if (o is MText) mtext = (MText)o;
                        else if (o is Polyline) polylines.Add((Polyline)o);
                        else if (o is Line) lines.Add((Line)o);
                    }

                    // Evaluation
                    bool pass = true;
                    string failReason = "";

                    if (mtext == null) {
                        pass = false;
                        failReason = "No MText found after Explode";
                    } else if (isVertical) {
                        // VERTICAL EVALUATION:
                        // 1. Text rotation must be 90 deg (PI/2)
                        // 2. Line 1 (X) must be on LEFT of stem (maxX < stemX)
                        // 3. Line 2 (Y) must be on RIGHT of stem (minX > stemX)
                        // 4. Stem runs between them
                        bool rotOk = Math.Abs(mtext.Rotation - Math.PI / 2) < 0.05;
                        double stemX = targetPt.X;

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

                        if (!rotOk) {
                            pass = false;
                            failReason = string.Format("Text rotation {0:0.#}° != 90°", mtext.Rotation * 180 / Math.PI);
                        } else if (dtX == null || dtY == null) {
                            pass = false;
                            failReason = "Could not find DBText X and Y";
                        } else {
                            double xMaxOfX = dtX.GeometricExtents.MaxPoint.X;
                            double xMinOfY = dtY.GeometricExtents.MinPoint.X;
                            bool xLeft = xMaxOfX < stemX;
                            bool yRight = xMinOfY > stemX;
                            double leftMargin = stemX - xMaxOfX;
                            double rightMargin = xMinOfY - stemX;

                            ed.WriteMessage(string.Format("\n  Vertical Stem: StemX={0:0.##}, LineX_Max={1:0.##} (Margin={2:0.##}), LineY_Min={3:0.##} (Margin={4:0.##})",
                                stemX, xMaxOfX, leftMargin, xMinOfY, rightMargin));

                            if (!xLeft || !yRight) {
                                pass = false;
                                failReason = string.Format("Stem not between X and Y: LineX_Left={0}, LineY_Right={1}", xLeft, yRight);
                            }
                        }
                    } else {
                        // HORIZONTAL / SLANTED EVALUATION:
                        // 1. Landing line Y must divide Line 1 (top) and Line 2 (bottom)
                        // 2. Top of MText > landingPt.Y and Bottom of MText < landingPt.Y
                        double landY = landingPt.Y;
                        double textTop = mtext.GeometricExtents.MaxPoint.Y;
                        double textBottom = mtext.GeometricExtents.MinPoint.Y;
                        bool lineBetween = (textTop > landY && textBottom < landY);

                        ed.WriteMessage(string.Format("\n  Horizontal Check: LandingY={0:0.##}, TextBounds=[{1:0.##}, {2:0.##}] => LineBetween={3}",
                            landY, textBottom, textTop, lineBetween));

                        if (!lineBetween) {
                            pass = false;
                            failReason = string.Format("Landing line Y={0:0.##} does not divide text [{1:0.##}, {2:0.##}]", landY, textBottom, textTop);
                        }
                    }

                    if (pass) {
                        passedTests++;
                        ed.WriteMessage(string.Format("\n  [VERDICT] PASS"));
                    } else {
                        failedTests++;
                        ed.WriteMessage(string.Format("\n  [VERDICT] FAIL: {0}", failReason));
                        ed.WriteMessage(string.Format("\n    Expected: {0}", isVertical ? "Vertical stem running between Line 1 (X) on left and Line 2 (Y) on right" : "Landing line strictly separating Line 1 and Line 2"));
                        ed.WriteMessage(string.Format("\n    Actual: {0}", failReason));
                    }
                }

                tr.Commit();
                ed.WriteMessage(string.Format("\n\n================================================================================"));
                ed.WriteMessage(string.Format("\n   SUMMARY: Total={0}, PASSED={1}, FAILED={2}", totalTests, passedTests, failedTests));
                ed.WriteMessage(string.Format("\n================================================================================"));
            }
        }
    }
}
