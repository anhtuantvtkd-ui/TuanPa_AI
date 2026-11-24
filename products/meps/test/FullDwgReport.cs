using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Tvxd.Meps.Test {
    public class FullDwgReport {
        [CommandMethod("FULL_DWG_REPORT")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n=======================================================");
            ed.WriteMessage("\n   FULL REPORT ON INPUT DWG: " + doc.Name);
            ed.WriteMessage("\n=======================================================");

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                // 1. Drawing limits and extents
                ed.WriteMessage(string.Format("\nExtents: Min=({0:0.##},{1:0.##}), Max=({2:0.##},{3:0.##})",
                    db.Extmin.X, db.Extmin.Y, db.Extmax.X, db.Extmax.Y));

                // 2. Layers
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                ed.WriteMessage("\nLayers in drawing:");
                foreach (ObjectId lId in lt) {
                    LayerTableRecord ltr = (LayerTableRecord)tr.GetObject(lId, OpenMode.ForRead);
                    ed.WriteMessage(string.Format(" [{0}]", ltr.Name));
                }

                // 3. Block Table
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                int dptCount = 0;
                int autocrdsCount = 0;
                int otherBlocks = 0;

                ed.WriteMessage("\n\nAll Blocks placed in ModelSpace:");
                foreach (ObjectId id in ms) {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj is BlockReference) {
                        BlockReference br = (BlockReference)obj;
                        string name = br.Name;
                        if (br.IsDynamicBlock) {
                            var btr = (BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord, OpenMode.ForRead);
                            name = btr.Name;
                        }
                        if (name == "DPT") {
                            dptCount++;
                            ed.WriteMessage(string.Format("\n  DPT #{0}: Pos=({1:0.##},{2:0.##}), Layer={3}", dptCount, br.Position.X, br.Position.Y, br.Layer));
                        } else if (name.StartsWith("AUTOCRD", StringComparison.OrdinalIgnoreCase)) {
                            autocrdsCount++;
                            ed.WriteMessage(string.Format("\n  AUTOCRDS #{0}: Name='{1}', Pos=({2:0.##},{3:0.##}), Layer={4}", autocrdsCount, name, br.Position.X, br.Position.Y, br.Layer));
                            foreach (ObjectId aId in br.AttributeCollection) {
                                AttributeReference att = (AttributeReference)tr.GetObject(aId, OpenMode.ForRead);
                                ed.WriteMessage(string.Format(" [{0}='{1}']", att.Tag, att.TextString));
                            }
                        } else {
                            otherBlocks++;
                            ed.WriteMessage(string.Format("\n  Block '{0}': Pos=({1:0.##},{2:0.##}), Layer={3}", name, br.Position.X, br.Position.Y, br.Layer));
                        }
                    }
                }

                ed.WriteMessage(string.Format("\n\nSummary: DPT={0}, AUTOCRDS={1}, Others={2}", dptCount, autocrdsCount, otherBlocks));
                tr.Commit();
            }
        }
    }
}
