using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class CheckExplodeLeader {
        [CommandMethod("CHECK_EXPLODE_LEADER")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            try {
                ToadoSettings s = Toado100.Load();
                ObjectId dotArrowId = Toado100.EnsureDotArrow(db);

                using (Transaction tr = db.TransactionManager.StartTransaction()) {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    Toado100.EnsureRegApp(tr, db);
                    ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                    ObjectId styleId = db.Textstyle;

                    Point3d p0 = new Point3d(400000, 2000000, 0);
                    Point3d pLanding = new Point3d(p0.X + s.Distance, p0.Y + s.Distance, 0);

                    ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, p0, pLanding, s, "DPT", "111", ObjectId.Null, layerId, styleId, dotArrowId);
                    MLeader ml = tr.GetObject(mlId, OpenMode.ForRead) as MLeader;

                    DBObjectCollection ents = new DBObjectCollection();
                    ml.Explode(ents);

                    ed.WriteMessage(string.Format("\n[EXPLODE] Count={0}", ents.Count));
                    foreach (DBObject ent in ents) {
                        if (ent is Polyline) {
                            Polyline pl = (Polyline)ent;
                            ed.WriteMessage(string.Format("\n  Polyline ({0} verts):", pl.NumberOfVertices));
                            for (int i = 0; i < pl.NumberOfVertices; i++) {
                                Point2d pt = pl.GetPoint2dAt(i);
                                ed.WriteMessage(string.Format(" ({0:0.##},{1:0.##})", pt.X, pt.Y));
                            }
                        } else if (ent is Line) {
                            Line l = (Line)ent;
                            double angle = Math.Atan2(l.EndPoint.Y - l.StartPoint.Y, l.EndPoint.X - l.StartPoint.X) * 180 / Math.PI;
                            ed.WriteMessage(string.Format("\n  Line: Start=({0:0.##},{1:0.##}) -> End=({2:0.##},{3:0.##}), Angle={4:0.##} deg, Len={5:0.##}",
                                l.StartPoint.X, l.StartPoint.Y, l.EndPoint.X, l.EndPoint.Y, angle, l.Length));
                        } else if (ent is MText) {
                            MText mt = (MText)ent;
                            ed.WriteMessage(string.Format("\n  MText: Loc=({0:0.##},{1:0.##}), Rot={2:0.##} deg, Contents='{3}'",
                                mt.Location.X, mt.Location.Y, mt.Rotation * 180 / Math.PI, mt.Contents.Replace("\n", " ")));
                        } else {
                            ed.WriteMessage(string.Format("\n  Entity: {0}", ent.GetType().Name));
                        }
                    }
                    tr.Commit();
                }
            } catch (System.Exception ex) {
                ed.WriteMessage("\n[EX] " + ex.ToString());
            }
        }
    }
}
