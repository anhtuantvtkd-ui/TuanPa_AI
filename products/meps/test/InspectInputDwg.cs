using System;
using System.IO;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class InspectInputDwg {
        [CommandMethod("INSPECT_INPUT_DWG")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n=======================================================");
            ed.WriteMessage("\n   ANALYZING INPUT DRAWING: " + doc.Name);
            ed.WriteMessage("\n=======================================================");

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                int blockCount = 0;
                int mleaderCount = 0;
                int textCount = 0;

                foreach (ObjectId id in ms) {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj is BlockReference) {
                        blockCount++;
                        BlockReference br = (BlockReference)obj;
                        string bName = Toado100.NameOf(tr, br);
                        if (blockCount <= 10) {
                            ed.WriteMessage(string.Format("\n  Block #{0}: '{1}', Pos=({2:0.##}, {3:0.##}), Layer={4}",
                                blockCount, bName, br.Position.X, br.Position.Y, br.Layer));
                        }
                    } else if (obj is MLeader) {
                        mleaderCount++;
                        MLeader ml = (MLeader)obj;
                        if (mleaderCount <= 10) {
                            ed.WriteMessage(string.Format("\n  MLeader #{0}: Layer={1}, ContentType={2}", mleaderCount, ml.Layer, ml.ContentType));
                            if (ml.ContentType == ContentType.MTextContent && ml.MText != null) {
                                ed.WriteMessage(string.Format(", Text='{0}', Rot={1:0.#} deg, TextLoc=({2:0.##},{3:0.##})",
                                    ml.MText.Contents.Replace("\n", " "), ml.MText.Rotation * 180 / Math.PI, ml.TextLocation.X, ml.TextLocation.Y));
                            }
                        }
                    } else if (obj is DBText || obj is MText) {
                        textCount++;
                    }
                }

                ed.WriteMessage(string.Format("\n\n>>> STATS: Blocks={0}, MLeaders={1}, Texts={2} <<<", blockCount, mleaderCount, textCount));
                tr.Commit();
            }
        }
    }
}
