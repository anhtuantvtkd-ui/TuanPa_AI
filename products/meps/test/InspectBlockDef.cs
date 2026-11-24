using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Tvxd.Meps.Test {
    public class InspectBlockDef {
        [CommandMethod("INSPECT_BLOCK_DEF")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (bt.Has("AUTOCRDS1")) {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt["AUTOCRDS1"], OpenMode.ForRead);
                    ed.WriteMessage("\n--- BLOCK DEF 'AUTOCRDS1' ENTITIES ---");
                    foreach (ObjectId id in btr) {
                        DBObject ent = tr.GetObject(id, OpenMode.ForRead);
                        ed.WriteMessage(string.Format("\n  Entity: {0}, Layer={1}", ent.GetType().Name, ((Entity)ent).Layer));
                        if (ent is Line) {
                            Line l = (Line)ent;
                            ed.WriteMessage(string.Format(" from ({0:0.##},{1:0.##}) to ({2:0.##},{3:0.##})", l.StartPoint.X, l.StartPoint.Y, l.EndPoint.X, l.EndPoint.Y));
                        } else if (ent is Circle) {
                            Circle c = (Circle)ent;
                            ed.WriteMessage(string.Format(" Center=({0:0.##},{1:0.##}), Radius={2:0.##}", c.Center.X, c.Center.Y, c.Radius));
                        } else if (ent is AttributeDefinition) {
                            AttributeDefinition ad = (AttributeDefinition)ent;
                            ed.WriteMessage(string.Format(" Tag='{0}', Pos=({1:0.##},{2:0.##}), Height={3:0.##}, Text='{4}', Rot={5:0.#} deg",
                                ad.Tag, ad.Position.X, ad.Position.Y, ad.Height, ad.TextString, ad.Rotation * 180 / Math.PI));
                        }
                    }
                }

                // Check other blocks in BlockTable
                ed.WriteMessage("\n--- ALL USER BLOCKS IN BLOCKTABLE ---");
                foreach (ObjectId bId in bt) {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bId, OpenMode.ForRead);
                    if (!btr.IsLayout && !btr.IsAnonymous) {
                        ed.WriteMessage(string.Format("\n  BlockDef: '{0}' (has {1} entities)", btr.Name, CountEntities(btr)));
                    }
                }
                tr.Commit();
            }
        }

        static int CountEntities(BlockTableRecord btr) {
            int c = 0;
            foreach (ObjectId id in btr) c++;
            return c;
        }
    }
}
