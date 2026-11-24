using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

public class TestLeaderGrips2 {
    [CommandMethod("TEST_LEADER_GRIPS_2")]
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

                Point3d targetPt = new Point3d(100, 100, 0);
                Point3d landingPt = new Point3d(125, 125, 0);

                // Test A: DoglegLength = 0 on MLeader
                ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, "DPT", "H1", ObjectId.Null, layerId, styleId, dotArrowId);
                MLeader ml = (MLeader)tr.GetObject(mlId, OpenMode.ForWrite);

                double gap = s.Height * 0.1;
                double w = ml.MText.ActualWidth > 0 ? ml.MText.ActualWidth : (s.Height * 5.0);
                double yTop = landingPt.Y + s.Height * 1.2;

                try {
                    ml.DoglegLength = 0.0;
                    ed.WriteMessage("\nml.DoglegLength = 0.0 SUCCEEDED!");
                } catch (System.Exception ex) {
                    ed.WriteMessage("\nml.DoglegLength = 0.0 FAILED: " + ex.Message);
                }

                ml.TextLocation = new Point3d(landingPt.X + gap, yTop, 0);

                DBObjectCollection ents = new DBObjectCollection();
                ml.Explode(ents);
                ed.WriteMessage(string.Format("\nExplode Count={0}", ents.Count));
                foreach (DBObject ent in ents) {
                    if (ent is Line) {
                        Line l = (Line)ent;
                        ed.WriteMessage(string.Format("\n  Line: ({0:0.##},{1:0.##}) -> ({2:0.##},{3:0.##}), Len={4:0.##}",
                            l.StartPoint.X, l.StartPoint.Y, l.EndPoint.X, l.EndPoint.Y, l.Length));
                    } else if (ent is Polyline) {
                        Polyline pl = (Polyline)ent;
                        ed.WriteMessage(string.Format("\n  Polyline ({0} verts): ({1:0.##},{2:0.##}) -> ({3:0.##},{4:0.##})",
                            pl.NumberOfVertices, pl.GetPoint2dAt(0).X, pl.GetPoint2dAt(0).Y,
                            pl.GetPoint2dAt(pl.NumberOfVertices-1).X, pl.GetPoint2dAt(pl.NumberOfVertices-1).Y));
                    } else if (ent is MText) {
                        MText t = (MText)ent;
                        ed.WriteMessage(string.Format("\n  MText: Loc=({0:0.##},{1:0.##})", t.Location.X, t.Location.Y));
                    }
                }
                tr.Commit();
            }
        } catch (System.Exception ex) {
            ed.WriteMessage("\n[EX] " + ex.ToString());
        }
    }
}
