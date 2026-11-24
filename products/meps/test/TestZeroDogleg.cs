using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

public class TestZeroDogleg {
    [CommandMethod("TEST_ZERO_DOGLEG")]
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

                // Test with custom leader where doglegLen = 0
                Point3d targetPt = new Point3d(400000, 2000000, 0);
                Point3d landingPt = new Point3d(targetPt.X + s.Distance, targetPt.Y + s.Distance, 0);

                MLeader ml = new MLeader();
                ml.SetDatabaseDefaults(db);
                ml.ContentType = ContentType.MTextContent;
                ml.EnableDogleg = false;
                ml.EnableLanding = true;
                double gap = s.Height * 0.1;
                ml.LandingGap = gap;
                ml.TextAttachmentDirection = TextAttachmentDirection.AttachmentHorizontal;
                ml.TextAttachmentType = TextAttachmentType.AttachmentBottomOfTopLine;
                ml.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.LeftLeader);
                ml.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.RightLeader);
                ml.ExtendLeaderToText = true;

                MText mt = new MText();
                mt.SetDatabaseDefaults(db);
                mt.TextHeight = s.Height;
                mt.Contents = Toado100.Format(targetPt, s, "DPT", false);
                mt.BackgroundFill = false;
                mt.Rotation = 0.0;
                mt.Direction = new Vector3d(1.0, 0.0, 0.0);
                ml.MText = mt;

                int leaderIndex = ml.AddLeader();
                int leaderLineIndex = ml.AddLeaderLine(leaderIndex);
                ml.AddFirstVertex(leaderLineIndex, targetPt);
                ml.AddLastVertex(leaderLineIndex, landingPt);

                double yTop = landingPt.Y + s.Height * 1.2;
                double w = mt.ActualWidth > 0 ? mt.ActualWidth : (s.Height * 5.0);
                double xLeft = landingPt.X + gap;
                ml.TextLocation = new Point3d(xLeft, yTop, 0);

                ms.AppendEntity(ml);
                tr.AddNewlyCreatedDBObject(ml, true);

                DBObjectCollection ents = new DBObjectCollection();
                ml.Explode(ents);

                ed.WriteMessage(string.Format("\n[TEST ZERO DOGLEG] Explode Count={0}", ents.Count));
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
                        MText t = (MText)ent;
                        ed.WriteMessage(string.Format("\n  MText: Loc=({0:0.##},{1:0.##}), Contents='{2}'",
                            t.Location.X, t.Location.Y, t.Contents.Replace("\n", " ")));
                    }
                }
                tr.Commit();
            }
        } catch (System.Exception ex) {
            ed.WriteMessage("\n[EX] " + ex.ToString());
        }
    }
}
