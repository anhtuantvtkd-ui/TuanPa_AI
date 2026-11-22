using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

public class TestLeftRightZeroDogleg {
    [CommandMethod("TEST_LR_ZERO")]
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

                // Case 1: Right-pointing leader
                Point3d targetPt = new Point3d(100, 100, 0);
                Point3d landingR = new Point3d(125, 125, 0);
                Point3d landingL = new Point3d(75, 125, 0);

                // Right
                MLeader mlR = new MLeader();
                mlR.SetDatabaseDefaults(db);
                mlR.ContentType = ContentType.MTextContent;
                mlR.EnableDogleg = true;
                mlR.DoglegLength = 0.0;
                mlR.EnableLanding = true;
                double gap = s.Height * 0.1;
                mlR.LandingGap = gap;
                mlR.TextAttachmentDirection = TextAttachmentDirection.AttachmentHorizontal;
                mlR.TextAttachmentType = TextAttachmentType.AttachmentBottomOfTopLine;
                mlR.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.LeftLeader);
                mlR.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.RightLeader);
                mlR.ExtendLeaderToText = true;

                MText mtR = new MText();
                mtR.SetDatabaseDefaults(db);
                mtR.TextHeight = s.Height;
                mtR.Contents = Toado100.Format(targetPt, s, "DPT", false);
                mtR.BackgroundFill = false;
                mtR.Rotation = 0.0;
                mtR.Direction = new Vector3d(1.0, 0.0, 0.0);
                mlR.MText = mtR;

                int lIndexR = mlR.AddLeader();
                int lineIndexR = mlR.AddLeaderLine(lIndexR);
                mlR.AddFirstVertex(lineIndexR, targetPt);
                mlR.AddLastVertex(lineIndexR, landingR);
                mlR.SetDogleg(lIndexR, new Vector3d(1.0, 0.0, 0.0));

                double yTop = landingR.Y + s.Height * 1.2;
                double w = mtR.ActualWidth > 0 ? mtR.ActualWidth : (s.Height * 5.0);
                mlR.TextLocation = new Point3d(landingR.X + gap, yTop, 0);
                ms.AppendEntity(mlR);
                tr.AddNewlyCreatedDBObject(mlR, true);

                DBObjectCollection entsR = new DBObjectCollection();
                mlR.Explode(entsR);
                ed.WriteMessage(string.Format("\n[RIGHT LEADER] Explode Count={0}", entsR.Count));
                foreach (DBObject ent in entsR) {
                    if (ent is Polyline) {
                        Polyline pl = (Polyline)ent;
                        ed.WriteMessage(string.Format("\n  PL: ({0:0.##},{1:0.##}) -> ({2:0.##},{3:0.##})",
                            pl.GetPoint2dAt(0).X, pl.GetPoint2dAt(0).Y,
                            pl.GetPoint2dAt(pl.NumberOfVertices-1).X, pl.GetPoint2dAt(pl.NumberOfVertices-1).Y));
                    } else if (ent is MText) {
                        MText t = (MText)ent;
                        ed.WriteMessage(string.Format("\n  MText: Loc=({0:0.##},{1:0.##})", t.Location.X, t.Location.Y));
                    }
                }

                // Left
                MLeader mlL = new MLeader();
                mlL.SetDatabaseDefaults(db);
                mlL.ContentType = ContentType.MTextContent;
                mlL.EnableDogleg = true;
                mlL.DoglegLength = 0.0;
                mlL.EnableLanding = true;
                mlL.LandingGap = gap;
                mlL.TextAttachmentDirection = TextAttachmentDirection.AttachmentHorizontal;
                mlL.TextAttachmentType = TextAttachmentType.AttachmentBottomOfTopLine;
                mlL.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.LeftLeader);
                mlL.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.RightLeader);
                mlL.ExtendLeaderToText = true;

                MText mtL = new MText();
                mtL.SetDatabaseDefaults(db);
                mtL.TextHeight = s.Height;
                mtL.Contents = Toado100.Format(targetPt, s, "DPT", false);
                mtL.BackgroundFill = false;
                mtL.Rotation = 0.0;
                mtL.Direction = new Vector3d(1.0, 0.0, 0.0);
                mlL.MText = mtL;

                int lIndexL = mlL.AddLeader();
                int lineIndexL = mlL.AddLeaderLine(lIndexL);
                mlL.AddFirstVertex(lineIndexL, targetPt);
                mlL.AddLastVertex(lineIndexL, landingL);
                mlL.SetDogleg(lIndexL, new Vector3d(-1.0, 0.0, 0.0));

                double xLeftL = landingL.X - gap - w;
                mlL.TextLocation = new Point3d(xLeftL, yTop, 0);
                ms.AppendEntity(mlL);
                tr.AddNewlyCreatedDBObject(mlL, true);

                DBObjectCollection entsL = new DBObjectCollection();
                mlL.Explode(entsL);
                ed.WriteMessage(string.Format("\n[LEFT LEADER] Explode Count={0}", entsL.Count));
                foreach (DBObject ent in entsL) {
                    if (ent is Polyline) {
                        Polyline pl = (Polyline)ent;
                        ed.WriteMessage(string.Format("\n  PL: ({0:0.##},{1:0.##}) -> ({2:0.##},{3:0.##})",
                            pl.GetPoint2dAt(0).X, pl.GetPoint2dAt(0).Y,
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
