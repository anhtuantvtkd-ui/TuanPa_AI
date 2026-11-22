using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

public class TestBatch45 {
    [CommandMethod("TEST_BATCH_45")]
    public static void Run() {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        try {
            ToadoSettings s = Toado100.Load();
            s.PickEach = false; // Batch 45 deg
            ObjectId dotArrowId = Toado100.EnsureDotArrow(db);

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                Toado100.EnsureRegApp(tr, db);
                ObjectId styleId = Toado100.EnsureTextStyle(tr, db, s.Style);
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var map = Toado100.ScanExistingLeaders(tr, ms);
                ed.WriteMessage(string.Format("\nSettings: Height={0}, Distance={1}, PickEach={2}", s.Height, s.Distance, s.PickEach));

                int count = 0;
                foreach (ObjectId id in ms) {
                    if (id.ObjectClass.DxfName.Equals("INSERT", StringComparison.OrdinalIgnoreCase)) {
                        var br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                        if (br == null) continue;
                        string blockName = Toado100.NameOf(tr, br);
                        if (blockName.IndexOf("khung", StringComparison.OrdinalIgnoreCase) >= 0) continue;

                        count++;
                        Point3d targetPt = Toado100.Tip(br, s.Center);
                        string handleStr = br.Handle.ToString();

                        Point3d landingPt = new Point3d(targetPt.X + s.Distance, targetPt.Y + s.Distance, targetPt.Z);
                        ObjectId existingId = map.ContainsKey(handleStr) ? map[handleStr] : ObjectId.Null;
                        ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, blockName, handleStr, existingId, layerId, styleId, dotArrowId);

                        var ml = tr.GetObject(mlId, OpenMode.ForRead) as MLeader;
                        Point3d v0 = ml.GetFirstVertex(0);
                        Point3d v1 = ml.GetLastVertex(0);
                        double dx = v1.X - v0.X;
                        double dy = v1.Y - v0.Y;
                        double angleDeg = Math.Atan2(dy, dx) * 180.0 / Math.PI;
                        ed.WriteMessage(string.Format("\nBlock #{0} [{1}]: V0=({2:0.##},{3:0.##}), V1=({4:0.##},{5:0.##}), Angle={6:0.##} deg, Dogleg={7}, Landing={8}",
                            count, blockName, v0.X, v0.Y, v1.X, v1.Y, angleDeg, ml.EnableDogleg, ml.EnableLanding));
                    }
                }
                tr.Commit();
                ed.WriteMessage(string.Format("\nProcessed {0} blocks.", count));
            }
        } catch (System.Exception ex) {
            ed.WriteMessage("\n[EX] " + ex.ToString());
        }
    }
}
