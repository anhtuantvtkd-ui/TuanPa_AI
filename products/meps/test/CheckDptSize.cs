using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Tvxd.Meps.Test {
    public class CheckDptSize {
        [CommandMethod("CHECK_DPT_SIZE")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId id in ms) {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj is BlockReference) {
                        BlockReference br = (BlockReference)obj;
                        if (br.Name == "DPT") {
                            Extents3d ext = br.GeometricExtents;
                            double w = ext.MaxPoint.X - ext.MinPoint.X;
                            double h = ext.MaxPoint.Y - ext.MinPoint.Y;
                            ed.WriteMessage(string.Format("\n  DPT Block Ref: Pos=({0:0.##},{1:0.##}), Extents: Width={2:0.##}, Height={3:0.##}",
                                br.Position.X, br.Position.Y, w, h));
                            break;
                        }
                    }
                }
                tr.Commit();
            }
        }
    }
}
