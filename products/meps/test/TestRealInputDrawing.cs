using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class TestRealInputDrawing {
        [CommandMethod("TEST_REAL_INPUT")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n=======================================================");
            ed.WriteMessage("\n   TESTING T-TOADO ON REAL DRAWING: " + doc.Name);
            ed.WriteMessage("\n=======================================================");

            ObjectId dotArrowId = Toado100.EnsureDotArrow(db);

            ToadoSettings s = new ToadoSettings();
            s.Height = 2.5;
            s.ArrowSize = 1.5;
            s.CircleArrow = (dotArrowId != ObjectId.Null);
            s.UnderlineBetween = true;
            s.Vn2000 = true; // In VN-2000, X is North (CAD Y), Y is East (CAD X)
            s.Decimals = 3;

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                Toado100.EnsureRegApp(tr, db);
                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                ObjectId styleId = db.Textstyle;

                int count = 0;
                foreach (ObjectId id in ms) {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj is BlockReference) {
                        BlockReference br = (BlockReference)obj;
                        if (br.Name == "DPT") {
                            count++;
                            Point3d targetPt = br.Position;
                            string bName = Toado100.NameOf(tr, br);
                            string handle = br.Handle.ToString();

                            // Test case: Block 1 -> Vertical Up label (like in media_1795321591323_25784349.png)
                            if (count == 1) {
                                Point3d landingPt = new Point3d(targetPt.X, targetPt.Y + 25.0, targetPt.Z);
                                ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, bName, handle, ObjectId.Null, layerId, styleId, dotArrowId);
                                ed.WriteMessage(string.Format("\n  [Block #1 DPT] Created VERTICAL label: Target=({0:0.##},{1:0.##}), Landing=({2:0.##},{3:0.##}), Id={4}",
                                    targetPt.X, targetPt.Y, landingPt.X, landingPt.Y, mlId));

                                // Verify Explode geometry
                                MLeader ml = (MLeader)tr.GetObject(mlId, OpenMode.ForRead);
                                DBObjectCollection col = new DBObjectCollection();
                                ml.Explode(col);
                                foreach (DBObject o in col) {
                                    if (o is MText) {
                                        MText mt = (MText)o;
                                        DBObjectCollection parts = new DBObjectCollection();
                                        mt.Explode(parts);
                                        ed.WriteMessage(string.Format("\n    MText Contents: '{0}', Rot={1:0.#} deg", mt.Contents.Replace("\n", " "), mt.Rotation * 180 / Math.PI));
                                        foreach (DBObject p in parts) {
                                            if (p is DBText) {
                                                DBText dt = (DBText)p;
                                                ed.WriteMessage(string.Format("\n      DBText: '{0}', Bounds=[({1:0.##},{2:0.##}) to ({3:0.##},{4:0.##})]",
                                                    dt.TextString, dt.GeometricExtents.MinPoint.X, dt.GeometricExtents.MinPoint.Y, dt.GeometricExtents.MaxPoint.X, dt.GeometricExtents.MaxPoint.Y));
                                            }
                                        }
                                    }
                                }
                            }
                            // Test case: Block 2 -> Horizontal / Slanted label (Q1)
                            else if (count == 2) {
                                Point3d landingPt = new Point3d(targetPt.X + 20.0, targetPt.Y + 15.0, targetPt.Z);
                                ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, bName, handle, ObjectId.Null, layerId, styleId, dotArrowId);
                                ed.WriteMessage(string.Format("\n  [Block #2 DPT] Created SLANTED label: Target=({0:0.##},{1:0.##}), Landing=({2:0.##},{3:0.##}), Id={4}",
                                    targetPt.X, targetPt.Y, landingPt.X, landingPt.Y, mlId));
                            }
                        }
                    }
                }
                tr.Commit();
                ed.WriteMessage(string.Format("\n\n>>> Tested on {0} DPT blocks in real DWG successfully! <<<", count));
            }
        }
    }
}
