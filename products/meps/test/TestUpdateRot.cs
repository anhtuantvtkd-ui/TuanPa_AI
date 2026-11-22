using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class TestUpdateRot {
        [CommandMethod("TEST_UPDATE_ROT")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            try {
                ToadoSettings s = Toado100.Load();
                ObjectId dotArrowId = Toado100.EnsureDotArrow(db);

                using (Transaction tr = db.TransactionManager.StartTransaction()) {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    Toado100.EnsureRegApp(tr, db);
                    ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                    ObjectId styleId = db.Textstyle;

                    Point3d p0 = new Point3d(400000, 2000000, 0);
                    // Step 1: Create a 90 deg Vertical MLeader
                    Point3d pVert = new Point3d(400000, 2000030, 0);
                    ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, p0, pVert, s, "TEST", "1234", ObjectId.Null, layerId, styleId, dotArrowId);
                    MLeader ml1 = tr.GetObject(mlId, OpenMode.ForRead) as MLeader;
                    ed.WriteMessage(string.Format("\n[STEP 1] Created Vertical: Rot={0} deg", ml1.MText.Rotation * 180 / Math.PI));

                    // Step 2: Now update it with 45 deg landingPt (dx=15, dy=15)
                    Point3d p45 = new Point3d(400015, 2000015, 0);
                    Toado100.CreateOrUpdateLeader(db, tr, ms, p0, p45, s, "TEST", "1234", mlId, layerId, styleId, dotArrowId);
                    MLeader ml2 = tr.GetObject(mlId, OpenMode.ForRead) as MLeader;
                    ed.WriteMessage(string.Format("\n[STEP 2] After 45 deg update: Rot={0} deg, Vertices V0=({1},{2}), V1=({3},{4}), TextLoc=({5},{6})",
                        ml2.MText.Rotation * 180 / Math.PI,
                        ml2.GetFirstVertex(0).X, ml2.GetFirstVertex(0).Y,
                        ml2.GetLastVertex(0).X, ml2.GetLastVertex(0).Y,
                        ml2.TextLocation.X, ml2.TextLocation.Y));

                    tr.Commit();
                }
            } catch (System.Exception ex) {
                ed.WriteMessage("\n[EX] " + ex.ToString());
            }
        }
    }
}
