using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class CheckBlockRot {
        [CommandMethod("CHECK_BLOCK_ROT")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                int count = 0;
                foreach (ObjectId id in ms) {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj is BlockReference) {
                        BlockReference br = (BlockReference)obj;
                        if (br.Name == "DPT") {
                            count++;
                            ed.WriteMessage(string.Format("\n[DPT #{0}] Pos=({1:0.##},{2:0.##}), Rot={3:0.##} rad ({4:0.#} deg)",
                                count, br.Position.X, br.Position.Y, br.Rotation, br.Rotation * 180 / Math.PI));
                        }
                    }
                }
                tr.Commit();
            }
        }
    }
}
