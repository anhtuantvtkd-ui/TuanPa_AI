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
                Console.WriteLine("Blocks in DWG:");
                foreach (ObjectId bId in bt) {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bId, OpenMode.ForRead);
                    if (btr.Name.IndexOf("dot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        btr.Name.IndexOf("arrow", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        btr.Name.StartsWith("_")) {
                        Console.WriteLine("  Block: " + btr.Name);
                    }
                }
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId id in ms) {
                    if (id.ObjectClass.DxfName == "MULTILEADER") {
                        MLeader ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);
                        Console.WriteLine("\nFound MLeader: ArrowSymbolId=" + ml.ArrowSymbolId + " Text=" + (ml.MText != null ? ml.MText.Contents : "null"));
                    }
                }
            }
        }
    }
}
