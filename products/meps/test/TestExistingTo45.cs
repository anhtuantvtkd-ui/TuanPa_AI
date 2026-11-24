using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

public class TestExistingTo45 {
    [CommandMethod("TEST_EXISTING_TO_45")]
    public static void Run() {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        try {
            ToadoSettings s = Toado100.Load();
            s.PickEach = false;
            ObjectId dotArrowId = Toado100.EnsureDotArrow(db);

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                Toado100.EnsureRegApp(tr, db);
                ObjectId styleId = Toado100.EnsureTextStyle(tr, db, s.Style);
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var map = Toado100.ScanExistingLeaders(tr, ms);
                ed.WriteMessage(string.Format("\nTotal existing leaders in map: {0}", map.Count));

                // Find block #1 (14968B) which has leader #1 (Handle 15653E)
                // In Mat bang cap dien trung the_TOADO_TEST.dwg, leader #1 is vertical (90 deg)
                if (map.ContainsKey("14968B")) {
                    ObjectId mlId = map["14968B"];
                    MLeader mlBefore = (MLeader)tr.GetObject(mlId, OpenMode.ForRead);
                    Point3d v0B = mlBefore.GetFirstVertex(0);
                    Point3d v1B = mlBefore.GetLastVertex(0);
                    ed.WriteMessage(string.Format("\nBEFORE update: V0={0}, V1={1}, dx={2:0.##}, dy={3:0.##}",
                        v0B, v1B, v1B.X - v0B.X, v1B.Y - v0B.Y));

                    // Now simulate batch 45 scan:
                    Point3d targetPt = v0B;
                    Point3d landingPt = new Point3d(targetPt.X + s.Distance, targetPt.Y + s.Distance, targetPt.Z);
                    Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, "DPT", "14968B", mlId, layerId, styleId, dotArrowId);

                    MLeader mlAfter = (MLeader)tr.GetObject(mlId, OpenMode.ForRead);
                    Point3d v0A = mlAfter.GetFirstVertex(0);
                    Point3d v1A = mlAfter.GetLastVertex(0);
                    ed.WriteMessage(string.Format("\nAFTER update: V0={0}, V1={1}, dx={2:0.##}, dy={3:0.##}",
                        v0A, v1A, v1A.X - v0A.X, v1A.Y - v0A.Y));
                } else {
                    ed.WriteMessage("\nBlock 14968B not found in map.");
                }
                tr.Commit();
            }
        } catch (System.Exception ex) {
            ed.WriteMessage("\n[EX] " + ex.ToString());
        }
    }
}
