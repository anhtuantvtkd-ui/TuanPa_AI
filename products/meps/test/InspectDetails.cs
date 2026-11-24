using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class InspectDetails {
        [CommandMethod("INSPECT_DETAILS")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                ed.WriteMessage("\n--- ALL BLOCKS IN MODELSPACE ---");
                foreach (ObjectId id in ms) {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj is BlockReference) {
                        BlockReference br = (BlockReference)obj;
                        string bName = Toado100.NameOf(tr, br);
                        ed.WriteMessage(string.Format("\n  Block '{0}': Pos=({1:0.##}, {2:0.##}, {3:0.##}), Rot={4:0.##} rad, Layer={5}",
                            bName, br.Position.X, br.Position.Y, br.Position.Z, br.Rotation, br.Layer));
                        if (br.AttributeCollection.Count > 0) {
                            foreach (ObjectId attId in br.AttributeCollection) {
                                AttributeReference att = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                                ed.WriteMessage(string.Format(" [Att: {0}='{1}']", att.Tag, att.TextString));
                            }
                        }
                    } else if (obj is DBText) {
                        DBText dt = (DBText)obj;
                        ed.WriteMessage(string.Format("\n  DBText: '{0}', Pos=({1:0.##}, {2:0.##}), Height={3:0.##}, Rot={4:0.##}, Layer={5}",
                            dt.TextString, dt.Position.X, dt.Position.Y, dt.Height, dt.Rotation * 180 / Math.PI, dt.Layer));
                    } else if (obj is MText) {
                        MText mt = (MText)obj;
                        ed.WriteMessage(string.Format("\n  MText: '{0}', Loc=({1:0.##}, {2:0.##}), Height={3:0.##}, Rot={4:0.##}, Layer={5}",
                            mt.Contents, mt.Location.X, mt.Location.Y, mt.TextHeight, mt.Rotation * 180 / Math.PI, mt.Layer));
                    }
                }
                tr.Commit();
            }
        }
    }
}
