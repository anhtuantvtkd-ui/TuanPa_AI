using System;
using System.IO;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

class Program {
    static void Main(string[] args) {
        string dwgPath = @"C:\Users\ADMIN\.gemini\antigravity\scratch\Mat bang cap dien trung the.dwg";
        if (args.Length > 0) dwgPath = args[0];

        Console.WriteLine("Reading: " + dwgPath);
        using (Database db = new Database(false, true)) {
            db.ReadDwgFile(dwgPath, FileShare.ReadWrite, true, "");
            Console.WriteLine("INSUNITS: " + db.Insunits);
            Console.WriteLine("MEASUREMENT: " + db.Measurement);
            Console.WriteLine("EXTMIN: " + db.Extmin);
            Console.WriteLine("EXTMAX: " + db.Extmax);

            Dictionary<string, int> blocks = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            List<string> sampleCoords = new List<string>();

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                int count = 0;
                foreach (ObjectId id in ms) {
                    if (id.ObjectClass.DxfName.Equals("INSERT", StringComparison.OrdinalIgnoreCase)) {
                        BlockReference br = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                        string name = br.Name;
                        if (br.IsDynamicBlock) {
                            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord, OpenMode.ForRead);
                            name = btr.Name;
                        }
                        if (!blocks.ContainsKey(name)) blocks[name] = 0;
                        blocks[name]++;

                        if (sampleCoords.Count < 25) {
                            Point3d pos = br.Position;
                            sampleCoords.Add(string.Format("Block: {0,-25} | Layer: {1,-20} | Pos: ({2:F2}, {3:F2})", name, br.Layer, pos.X, pos.Y));
                        }
                        count++;
                    }
                }
                tr.Commit();
                Console.WriteLine("Total Block References in ModelSpace: " + count);
                Console.WriteLine("\n--- SAMPLE POSITIONS ---");
                foreach (var s in sampleCoords) Console.WriteLine(s);

                Console.WriteLine("\n--- BLOCK COUNTS ---");
                foreach (var kv in blocks) {
                    Console.WriteLine(string.Format("{0,-35}: {1}", kv.Key, kv.Value));
                }
            }
        }
    }
}
