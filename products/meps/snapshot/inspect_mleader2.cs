using System;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;

class Program {
    static void Main() {
        string dwg = @"C:\Users\ADMIN\.gemini\antigravity\scratch\Mat bang cap dien trung the.dwg";
        using (Database db = new Database(false, true)) {
            db.ReadDwgFile(dwg, FileShare.ReadWrite, true, "");
            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                int count = 0;
                foreach (ObjectId id in ms) {
                    if (id.ObjectClass.DxfName == "MULTILEADER") {
                        count++;
                        MLeader ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);
                        Console.WriteLine("=== MLeader #" + count + " ===");
                        Console.WriteLine("EnableDogleg: " + ml.EnableDogleg);
                        Console.WriteLine("DoglegLength: " + ml.DoglegLength);
                        Console.WriteLine("EnableLanding: " + ml.EnableLanding);
                        Console.WriteLine("LandingGap: " + ml.LandingGap);
                        Console.WriteLine("ExtendLeaderToText: " + ml.ExtendLeaderToText);
                        Console.WriteLine("TextLocation: " + ml.TextLocation);
                        Console.WriteLine("TextAlignmentType: " + ml.TextAlignmentType);
                        Console.WriteLine("TextAttachmentType Left: " + ml.GetTextAttachmentType(LeaderDirectionType.LeftLeader));
                        Console.WriteLine("TextAttachmentType Right: " + ml.GetTextAttachmentType(LeaderDirectionType.RightLeader));
                        Console.WriteLine("LeaderCount: " + ml.LeaderCount);
                        for (int l = 0; l < ml.LeaderCount; l++) {
                            Console.WriteLine("  Leader #" + l + " Dogleg: " + ml.GetDogleg(l));
                            int numLines = ml.GetLeaderLineIndexes(l).Count;
                            Console.WriteLine("  LeaderLines: " + numLines);
                            foreach (int lineIdx in ml.GetLeaderLineIndexes(l)) {
                                int nVert = ml.NumVertices(lineIdx);
                                Console.WriteLine("    Line " + lineIdx + " numVert: " + nVert);
                                for (int v = 0; v < nVert; v++) {
                                    Console.WriteLine("      v[" + v + "]: " + ml.GetVertex(lineIdx, v));
                                }
                            }
                        }
                        if (ml.MText != null) {
                            Console.WriteLine("MText Contents: " + ml.MText.Contents);
                            Console.WriteLine("MText Attachment: " + ml.MText.Attachment);
                            Console.WriteLine("MText Location: " + ml.MText.Location);
                            Console.WriteLine("MText Width: " + ml.MText.Width);
                            Console.WriteLine("MText ActualWidth: " + ml.MText.ActualWidth);
                            Console.WriteLine("MText ActualHeight: " + ml.MText.ActualHeight);
                        }
                        if (count >= 5) break;
                    }
                }
                Console.WriteLine("Total MLeaders examined: " + count);
            }
        }
    }
}
