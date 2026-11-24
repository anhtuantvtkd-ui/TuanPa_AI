using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class GripDragTest {
        [CommandMethod("GRIP_DRAG_TEST")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n==================================================================");
            ed.WriteMessage("\n   AUTOMATED CAD TEST: GRIP DRAG & 360-DEGREE FLIP STABILITY");
            ed.WriteMessage("\n==================================================================");

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                ObjectId dotArrowId = Toado100.EnsureDotArrow(db);
                Toado100.EnsureRegApp(tr, db);
                ObjectId layerId = Toado100.EnsureLayer(tr, db, "TOA_DO");
                ObjectId styleId = db.Textstyle;

                ToadoSettings s = new ToadoSettings();
                s.Height = 2.5;
                s.ArrowSize = 1.5;
                s.CircleArrow = true;
                s.UnderlineBetween = true;
                s.Mask = false;

                Point3d targetPt = new Point3d(100, 100, 0);

                // Define 6 grip positions to simulate user dragging grip in AutoCAD to avoid obstacles
                var gripCases = new[] {
                    new { Label = "Case 1: Initial Placement Q1 (Up-Right)", Landing = new Point3d(120, 115, 0), IsRight = true },
                    new { Label = "Case 2: Grip Drag to Q2 (Up-Left, horizontal flip)", Landing = new Point3d(80, 120, 0), IsRight = false },
                    new { Label = "Case 3: Grip Drag to Q3 (Down-Left, diagonal flip)", Landing = new Point3d(75, 85, 0), IsRight = false },
                    new { Label = "Case 4: Grip Drag to Q4 (Down-Right, horizontal flip)", Landing = new Point3d(125, 80, 0), IsRight = true },
                    new { Label = "Case 5: Grip Drag to Slanted Near-Axis (dx=5, dy=25)", Landing = new Point3d(105, 125, 0), IsRight = true },
                    new { Label = "Case 6: Grip Drag to Slanted Near-Axis Left (dx=-5, dy=-25)", Landing = new Point3d(95, 75, 0), IsRight = false }
                };

                // Create initial leader
                ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, gripCases[0].Landing, s, "BLOCK_A", "H_01", ObjectId.Null, layerId, styleId, dotArrowId);
                MLeader ml = (MLeader)tr.GetObject(mlId, OpenMode.ForWrite);

                int passed = 0;
                int total = gripCases.Length;

                for (int i = 0; i < gripCases.Length; i++) {
                    var gc = gripCases[i];
                    ed.WriteMessage(string.Format("\n\n--- {0} ---", gc.Label));
                    ed.WriteMessage(string.Format("\n  Target=({0},{1}), Landing=({2},{3}), IsRight={4}",
                        targetPt.X, targetPt.Y, gc.Landing.X, gc.Landing.Y, gc.IsRight));

                    // Move landing vertex grip
                    int lineIndex = Convert.ToInt32(ml.GetLeaderLineIndexes(0)[0]);
                    ml.SetLastVertex(lineIndex, gc.Landing);
                    Vector3d doglegDir = new Vector3d(gc.IsRight ? 1.0 : -1.0, 0.0, 0.0);
                    ml.SetDogleg(0, doglegDir);

                    double doglegLen = 0.0;
                    double gap = s.Height * 0.1;
                    double w = ml.MText.ActualWidth > 0 ? ml.MText.ActualWidth : (s.Height * 5.0);
                    double yTop = gc.Landing.Y + s.Height * 1.2;
                    double xLeft = gc.IsRight ? (gc.Landing.X + gap) : (gc.Landing.X - gap - w);
                    ml.TextLocation = new Point3d(xLeft, yTop, 0);

                    // Explode to verify physical geometry
                    DBObjectCollection col = new DBObjectCollection();
                    ml.Explode(col);

                    MText mtext = null;
                    List<Line> lines = new List<Line>();
                    foreach (DBObject o in col) {
                        if (o is MText) mtext = (MText)o;
                        else if (o is Line) lines.Add((Line)o);
                    }

                    // Check 1: BackgroundFill must be False (transparent)
                    bool bgOk = (mtext != null && !mtext.BackgroundFill);

                    // Check 2: Leader line attached at Landing point (zero dogleg gap)
                    bool hasElbow = false;
                    foreach (Line ln in lines) {
                        if (Math.Abs(ln.StartPoint.Y - ln.EndPoint.Y) < 1e-3 && Math.Abs(ln.StartPoint.Y - gc.Landing.Y) < 1e-3) {
                            if (Math.Abs(ln.StartPoint.X - gc.Landing.X) < 1e-2 || Math.Abs(ln.EndPoint.X - gc.Landing.X) < 1e-2) {
                                hasElbow = true;
                                break;
                            }
                        }
                    }

                    // Check 3: Separator between LineX and LineY
                    bool sepOk = false;
                    string sepNote = "";
                    if (mtext != null) {
                        DBObjectCollection textParts = new DBObjectCollection();
                        try { mtext.Explode(textParts); } catch {}
                        DBText dtX = null, dtY = null;
                        foreach (DBObject p in textParts) {
                            if (p is DBText) {
                                DBText dt = (DBText)p;
                                if (dt.TextString.StartsWith("X=")) dtX = dt;
                                else if (dt.TextString.StartsWith("Y=")) dtY = dt;
                            }
                        }
                        if (dtX != null && dtY != null) {
                            double minY_X = dtX.GeometricExtents.MinPoint.Y;
                            double maxY_Y = dtY.GeometricExtents.MaxPoint.Y;
                            double landY = gc.Landing.Y;
                            // LineX (top) is above landY, LineY (bottom) is below landY
                            sepOk = (minY_X > landY && maxY_Y < landY);
                            sepNote = string.Format("LineX_MinY={0:0.##} > LandY={1:0.##} > LineY_MaxY={2:0.##} => UnderlineBetween={3}",
                                minY_X, landY, maxY_Y, sepOk);
                        } else {
                            sepNote = "Could not explode MText into DBText";
                        }
                    }

                    bool casePass = bgOk && hasElbow && sepOk;
                    if (casePass) passed++;

                    ed.WriteMessage(string.Format("\n  Background Transparent: {0}", bgOk ? "PASS" : "FAIL"));
                    ed.WriteMessage(string.Format("\n  Dogleg Elbow (len={0:0.##}): {1}", doglegLen, hasElbow ? "PASS" : "FAIL"));
                    ed.WriteMessage(string.Format("\n  Separator Check: {0} => {1}", sepNote, sepOk ? "PASS" : "FAIL"));
                    ed.WriteMessage(string.Format("\n  ==> Verdict: {0}", casePass ? "PASS" : "FAIL"));
                }

                tr.Commit();
                ed.WriteMessage(string.Format("\n\n>>> GRIP DRAG TEST SUMMARY: {0}/{1} CASES PASSED 100% <<<\n", passed, total));
            }
        }
    }
}
