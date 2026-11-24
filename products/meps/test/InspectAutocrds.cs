using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Tvxd.Meps.Test {
    public class InspectAutocrds {
        [CommandMethod("INSPECT_AUTOCRDS")]
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
                        if (br.Name.StartsWith("AUTOCRD", StringComparison.OrdinalIgnoreCase)) {
                            ed.WriteMessage(string.Format("\n  AUTOCRD Ref: Name='{0}', Pos=({1:0.##},{2:0.##}), Scale=({3:0.##},{4:0.##},{5:0.##}), Rot={6:0.#} deg",
                                br.Name, br.Position.X, br.Position.Y, br.ScaleFactors.X, br.ScaleFactors.Y, br.ScaleFactors.Z, br.Rotation * 180 / Math.PI));
                            foreach (ObjectId aId in br.AttributeCollection) {
                                AttributeReference att = (AttributeReference)tr.GetObject(aId, OpenMode.ForRead);
                                ed.WriteMessage(string.Format("\n    Att '{0}': Text='{1}', Pos=({2:0.##},{3:0.##}), Rot={4:0.#} deg, Height={5:0.##}",
                                    att.Tag, att.TextString, att.Position.X, att.Position.Y, att.Rotation * 180 / Math.PI, att.Height));
                            }
                        }
                    }
                }
                tr.Commit();
            }
        }
    }
}
