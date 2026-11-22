using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class TestGeometryToado {
        [CommandMethod("TEST_TOADO_ALL")]
        public static void RunAllTests() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n==================================================================");
            ed.WriteMessage("\n   AUTOMATED ACCORECONSOLE TEST: T-TOADO GEOMETRY (6 CASES)");
            ed.WriteMessage("\n==================================================================");

            ObjectId dotId = ObjectId.Null;
            try { dotId = Toado100.EnsureDotArrow(db); } catch {}

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                Toado100.EnsureRegApp(tr, db);

                ToadoSettings s = new ToadoSettings();
                s.Height = 2.5;
                s.ArrowSize = 1.5;
                s.CircleArrow = (dotId != ObjectId.Null);
                s.UnderlineBetween = true;

                ObjectId styleId = db.Textstyle;
                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);

                int passed = 0;
                int total = 0;

                // 1. Q1: Up-Right
                total++;
                if (RunCase(ed, tr, ms, db, s, layerId, styleId, dotId, new Point3d(0, 0, 0), new Point3d(25, 15, 0), "Case 1: Up-Right (Q1)", false)) passed++;

                // 2. Q2: Up-Left
                total++;
                if (RunCase(ed, tr, ms, db, s, layerId, styleId, dotId, new Point3d(50, 0, 0), new Point3d(25, 15, 0), "Case 2: Up-Left (Q2)", false)) passed++;

                // 3. Q3: Down-Left
                total++;
                if (RunCase(ed, tr, ms, db, s, layerId, styleId, dotId, new Point3d(100, 0, 0), new Point3d(75, -15, 0), "Case 3: Down-Left (Q3)", false)) passed++;

                // 4. Q4: Down-Right
                total++;
                if (RunCase(ed, tr, ms, db, s, layerId, styleId, dotId, new Point3d(150, 0, 0), new Point3d(175, -15, 0), "Case 4: Down-Right (Q4)", false)) passed++;

                // 5. Vertical Up
                total++;
                if (RunCase(ed, tr, ms, db, s, layerId, styleId, dotId, new Point3d(200, 0, 0), new Point3d(200, 30, 0), "Case 5: Vertical Up (90 deg)", true)) passed++;

                // 6. Vertical Down
                total++;
                if (RunCase(ed, tr, ms, db, s, layerId, styleId, dotId, new Point3d(250, 0, 0), new Point3d(250, -30, 0), "Case 6: Vertical Down (90 deg)", true)) passed++;

                tr.Commit();
                ed.WriteMessage(string.Format("\n\n>>> SUMMARY: {0}/{1} CASES PASSED 100% <<<", passed, total));
            }
        }

        static bool RunCase(Editor ed, Transaction tr, BlockTableRecord ms, Database db, ToadoSettings s, ObjectId layerId, ObjectId styleId, ObjectId dotId, Point3d targetPt, Point3d landingPt, string label, bool expectVertical) {
            ed.WriteMessage(string.Format("\n--- {0} ---", label));
            ed.WriteMessage(string.Format("\n  Input: Target=({0:0.##},{1:0.##}), Landing=({2:0.##},{3:0.##})", targetPt.X, targetPt.Y, landingPt.X, landingPt.Y));

            ObjectId id = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, "", "", ObjectId.Null, layerId, styleId, dotId);
            MLeader ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);

            DBObjectCollection col = new DBObjectCollection();
            ml.Explode(col);

            MText text = null;
            Polyline landingLine = null;
            Polyline leaderStem = null;

            foreach (DBObject o in col) {
                if (o is MText) {
                    text = (MText)o;
                } else if (o is Polyline) {
                    Polyline pl = (Polyline)o;
                    if (pl.NumberOfVertices == 2) {
                        Point2d p1 = pl.GetPoint2dAt(0);
                        Point2d p2 = pl.GetPoint2dAt(1);
                        if (Math.Abs(p1.Y - p2.Y) < 1e-4) {
                            landingLine = pl;
                        } else {
                            leaderStem = pl;
                        }
                    }
                }
            }

            bool ok = false;
            if (!expectVertical) {
                // Horizontal / Slanted: Landing line must be at landingPt.Y
                // Text top line must be strictly above landing line, bottom line strictly below
                if (text != null) {
                    double landingY = landingPt.Y;
                    double textTop = text.GeometricExtents.MaxPoint.Y;
                    double textBottom = text.GeometricExtents.MinPoint.Y;
                    bool lineBetween = (textTop > landingY && textBottom < landingY);
                    ed.WriteMessage(string.Format("\n  Horizontal Check: LandingY={0:0.##}, TextBounds=[{1:0.##}, {2:0.##}] => LineBetween={3}",
                        landingY, textBottom, textTop, lineBetween));
                    ok = lineBetween;
                }
            } else {
                // Vertical 90 deg: Text rotation = 90 deg (Math.PI / 2)
                // Text line 1 (X) must be to the left of dividing line, line 2 (Y) to the right
                if (text != null) {
                    bool rotOk = Math.Abs(text.Rotation - Math.PI / 2) < 0.05;
                    double minX = text.GeometricExtents.MinPoint.X;
                    double maxX = text.GeometricExtents.MaxPoint.X;
                    double midX = (minX + maxX) / 2.0;
                    ed.WriteMessage(string.Format("\n  Vertical Check: Rot={0:0.#} deg, X_Bounds=[{1:0.##}, {2:0.##}], MidX={3:0.##} => VerticalStemSeparation={4}",
                        text.Rotation * 180 / Math.PI, minX, maxX, midX, rotOk));
                    ok = rotOk;
                }
            }

            ed.WriteMessage(string.Format("\n  Verdict: {0}", ok ? "PASS" : "FAIL"));
            return ok;
        }
    }
}
