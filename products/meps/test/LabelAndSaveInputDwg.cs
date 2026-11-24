using System;
using System.IO;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class LabelAndSaveInputDwg {
        [CommandMethod("LABEL_AND_SAVE_INPUT")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n=======================================================");
            ed.WriteMessage("\n   LABELING ALL DPT BLOCKS ON: " + doc.Name);
            ed.WriteMessage("\n=======================================================");

            ObjectId dotArrowId = Toado100.EnsureDotArrow(db);

            ToadoSettings s = new ToadoSettings();
            s.Height = 2.5;
            s.ArrowSize = 1.5;
            s.CircleArrow = (dotArrowId != ObjectId.Null);
            s.UnderlineBetween = true;
            s.Vn2000 = true; // VN-2000 coordinate convention
            s.Decimals = 3;
            s.Layer = "TOA_DO";

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                Toado100.EnsureRegApp(tr, db);
                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                ObjectId styleId = db.Textstyle;

                int count = 0;
                foreach (ObjectId id in ms) {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj is BlockReference) {
                        BlockReference br = (BlockReference)obj;
                        if (br.Name == "DPT") {
                            count++;
                            Point3d targetPt = br.Position;
                            string bName = Toado100.NameOf(tr, br);
                            string handle = br.Handle.ToString();

                            // Place alternating: odd blocks = Vertical Up (90 deg), even blocks = Slanted (45 deg)
                            Point3d landingPt;
                            if (count % 2 == 1) {
                                // Vertical Up
                                landingPt = new Point3d(targetPt.X, targetPt.Y + 15.0, targetPt.Z);
                            } else {
                                // Slanted Q1
                                landingPt = new Point3d(targetPt.X + 12.0, targetPt.Y + 10.0, targetPt.Z);
                            }

                            ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, bName, handle, ObjectId.Null, layerId, styleId, dotArrowId);
                            ed.WriteMessage(string.Format("\n  Labeled DPT #{0}: Mode={1}, Pos=({2:0.##},{3:0.##})",
                                count, (count % 2 == 1 ? "VERTICAL" : "SLANTED"), targetPt.X, targetPt.Y));
                        }
                    }
                }
                tr.Commit();
                ed.WriteMessage(string.Format("\n\n>>> Successfully labeled {0} DPT blocks! <<<", count));
            }

            // Save labeled drawing
            string outPath = @"D:\TuanPa_AI\OUTPUT\Mat bang cap dien trung the_TOADO_TEST.dwg";
            try {
                db.SaveAs(outPath, DwgVersion.Current);
                ed.WriteMessage("\n>>> Saved labeled drawing to: " + outPath);
            } catch (System.Exception ex) {
                ed.WriteMessage("\nSaveAs error: " + ex.Message);
            }
        }
    }
}
