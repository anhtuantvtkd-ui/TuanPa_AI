using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Tvxd.Meps.Test {
    public class InspectAutocrds1 {
        [CommandMethod("INSPECT_AUTOCRDS1")]
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
                            ed.WriteMessage(string.Format("\n[AUTOCRDS REF] Name='{0}', Pos=({1:0.##},{2:0.##}), Rot={3:0.##} rad ({4:0.#} deg)",
                                br.Name, br.Position.X, br.Position.Y, br.Rotation, br.Rotation * 180 / Math.PI));
                            foreach (ObjectId aId in br.AttributeCollection) {
                                AttributeReference att = (AttributeReference)tr.GetObject(aId, OpenMode.ForRead);
                                ed.WriteMessage(string.Format("\n  Att: Tag='{0}', Text='{1}', Pos=({2:0.##},{3:0.##}), Rot={4:0.#} deg, Height={5}",
                                    att.Tag, att.TextString, att.Position.X, att.Position.Y, att.Rotation * 180 / Math.PI, att.Height));
                            }
                        }
                    }
                }

                if (bt.Has("AUTOCRDS1")) {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt["AUTOCRDS1"], OpenMode.ForRead);
                    ed.WriteMessage("\n\n[AUTOCRDS1 DEF ENTITIES]:");
                    foreach (ObjectId id in btr) {
                        DBObject ent = tr.GetObject(id, OpenMode.ForRead);
                        if (ent is Line) {
                            Line l = (Line)ent;
                            double angle = Math.Atan2(l.EndPoint.Y - l.StartPoint.Y, l.EndPoint.X - l.StartPoint.X) * 180 / Math.PI;
                            ed.WriteMessage(string.Format("\n  Line: Start=({0:0.##},{1:0.##}) -> End=({2:0.##},{3:0.##}), Angle={4:0.#} deg, Len={5:0.##}",
                                l.StartPoint.X, l.StartPoint.Y, l.EndPoint.X, l.EndPoint.Y, angle, l.Length));
                        } else if (ent is AttributeDefinition) {
                            AttributeDefinition att = (AttributeDefinition)ent;
                            ed.WriteMessage(string.Format("\n  AttDef: Tag='{0}', Prompt='{1}', Default='{2}', Pos=({3:0.##},{4:0.##}), Rot={5:0.#} deg, Height={6}",
                                att.Tag, att.Prompt, att.TextString, att.Position.X, att.Position.Y, att.Rotation * 180 / Math.PI, att.Height));
                        } else if (ent is Circle) {
                            Circle c = (Circle)ent;
                            ed.WriteMessage(string.Format("\n  Circle: Center=({0:0.##},{1:0.##}), Radius={2:0.##}", c.Center.X, c.Center.Y, c.Radius));
                        } else {
                            ed.WriteMessage(string.Format("\n  Entity: {0}", ent.GetType().Name));
                        }
                    }
                }
                tr.Commit();
            }
        }
    }
}
