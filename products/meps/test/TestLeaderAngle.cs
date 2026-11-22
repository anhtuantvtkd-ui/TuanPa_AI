using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

public class TestLeaderAngle {
    [CommandMethod("TEST_LEADER_ANGLE")]
    public static void Run() {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        try {
            ToadoSettings s = Toado100.Load();
            ObjectId dotArrowId = Toado100.EnsureDotArrow(db);

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                Toado100.EnsureRegApp(tr, db);
                ObjectId styleId = Toado100.EnsureTextStyle(tr, db, s.Style);
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                Point3d targetPt = new Point3d(1000, 1000, 0);
                // Distance = 15
                Point3d landingPt = new Point3d(targetPt.X + s.Distance, targetPt.Y + s.Distance, 0);

                ed.WriteMessage(string.Format("\nCreating leader with target={0}, landing={1}, Height={2}, Distance={3}",
                    targetPt, landingPt, s.Height, s.Distance));

                ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, "TEST", "1234", ObjectId.Null, layerId, styleId, dotArrowId);
                tr.Commit();

                using (Transaction tr2 = db.TransactionManager.StartTransaction()) {
                    var ml = tr2.GetObject(mlId, OpenMode.ForRead) as MLeader;
                    Point3d v0 = ml.GetFirstVertex(0);
                    Point3d v1 = ml.GetLastVertex(0);
                    ed.WriteMessage(string.Format("\nAfter commit: V0={0}, V1={1}, TextLoc={2}", v0, v1, ml.TextLocation));
                    double dx = v1.X - v0.X;
                    double dy = v1.Y - v0.Y;
                    double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
                    ed.WriteMessage(string.Format("\nAngle={0:0.##} deg, dx={1:0.##}, dy={2:0.##}", angle, dx, dy));
                    tr2.Commit();
                }
            }
        } catch (System.Exception ex) {
            ed.WriteMessage("\n[EX] " + ex.ToString());
        }
    }
}
