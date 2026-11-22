using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

public class TestTextRotation {
    [CommandMethod("TEST_TEXT_ROT")]
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
                if (map.ContainsKey("14968B")) {
                    ObjectId mlId = map["14968B"];
                    MLeader ml = (MLeader)tr.GetObject(mlId, OpenMode.ForRead);
                    ed.WriteMessage(string.Format("\nLeader for 14968B:"));
                    ed.WriteMessage(string.Format("\n  MLeader.TextHeight: {0}", ml.TextHeight));
                    if (ml.MText != null) {
                        ed.WriteMessage(string.Format("\n  MText.Rotation: {0} rad ({1:0.##} deg)", ml.MText.Rotation, ml.MText.Rotation * 180 / Math.PI));
                        ed.WriteMessage(string.Format("\n  MText.Direction: {0}", ml.MText.Direction));
                    }
                    ed.WriteMessage(string.Format("\n  TextAttachmentType: {0}", ml.TextAttachmentType));
                    ed.WriteMessage(string.Format("\n  TextAttachmentDirection: {0}", ml.TextAttachmentDirection));
                }
                tr.Commit();
            }
        } catch (System.Exception ex) {
            ed.WriteMessage("\n[EX] " + ex.ToString());
        }
    }
}
