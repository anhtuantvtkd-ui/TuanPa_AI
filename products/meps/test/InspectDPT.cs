using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Tvxd.Meps.Test {
    public class InspectDPT {
        [CommandMethod("INSPECT_DPT")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (bt.Has("DPT")) {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt["DPT"], OpenMode.ForRead);
                    ed.WriteMessage("\n--- BLOCK DEF 'DPT' ENTITIES ---");
                    foreach (ObjectId id in btr) {
                        DBObject ent = tr.GetObject(id, OpenMode.ForRead);
                        ed.WriteMessage(string.Format("\n  Entity: {0}, Color={1}, Layer={2}",
                            ent.GetType().Name, ((Entity)ent).Color, ((Entity)ent).Layer));
                        if (ent is Hatch) {
                            Hatch h = (Hatch)ent;
                            ed.WriteMessage(string.Format(" (HatchPattern={0})", h.PatternName));
                        }
                    }
                }
                tr.Commit();
            }
        }
    }
}
