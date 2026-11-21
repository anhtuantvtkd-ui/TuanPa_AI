using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

class Program {
    static void Main() {
        using (Database db = new Database(true, true)) {
            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                MLeader ml = new MLeader();
                ml.SetDatabaseDefaults(db);
                ml.ContentType = ContentType.MTextContent;

                MText mt = new MText();
                mt.SetDatabaseDefaults(db);
                mt.TextHeight = 2.5;
                mt.Contents = "\\pxqc;X=100.000\\PY=200.000";
                ml.MText = mt;

                ml.TextHeight = 2.5;
                ml.EnableDogleg = false;
                ml.EnableLanding = true;
                ml.LandingGap = 0.5;

                ml.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.LeftLeader);
                ml.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.RightLeader);
                ml.ExtendLeaderToText = true;

                Point3d targetPt = new Point3d(0, 0, 0);
                Point3d landingPt = new Point3d(20, 20, 0);

                ml.TextLocation = landingPt;

                int lIdx = ml.AddLeader();
                int lineIdx = ml.AddLeaderLine(lIdx);
                ml.AddFirstVertex(lineIdx, targetPt);
                ml.AddLastVertex(lineIdx, landingPt);

                ms.AppendEntity(ml);
                tr.AddNewlyCreatedDBObject(ml, true);

                Console.WriteLine("Leader count: " + ml.LeaderCount);
                Console.WriteLine("LeaderLine count: " + ml.LeaderLineCount);
                int numVertices = ml.NumVertices(lineIdx);
                Console.WriteLine("Vertices in line: " + numVertices);
                for (int v = 0; v < numVertices; v++) {
                    Console.WriteLine("  v[" + v + "] = " + ml.GetVertex(lineIdx, v));
                }
                Console.WriteLine("Dogleg: " + ml.GetDogleg(lIdx));
                Console.WriteLine("TextLocation: " + ml.TextLocation);
                tr.Commit();
            }
        }
    }
}
