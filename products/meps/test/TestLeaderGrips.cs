using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

public class TestLeaderGrips {
    [CommandMethod("TEST_LEADER_GRIPS")]
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

                // Option 1: EnableDogleg = false, gap = Height * 0.1
                ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, "DPT", "H1", ObjectId.Null, layerId, styleId, dotArrowId);
                MLeader ml = (MLeader)tr.GetObject(mlId, OpenMode.ForWrite);

                ed.WriteMessage(string.Format("\nCurrent Leader: EnableDogleg={0}, DoglegLength={1}, LandingGap={2}",
                    ml.EnableDogleg, ml.DoglegLength, ml.LandingGap));
                ed.WriteMessage(string.Format("\nV0={0}, V1={1}, TextLoc={2}",
                    ml.GetFirstVertex(0), ml.GetLastVertex(0), ml.TextLocation));

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
