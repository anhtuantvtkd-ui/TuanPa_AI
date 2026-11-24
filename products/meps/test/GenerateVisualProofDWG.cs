using System;
using System.IO;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class GenerateVisualProofDWG {
        [CommandMethod("GENERATE_VISUAL_PROOF")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n=======================================================");
            ed.WriteMessage("\n   GENERATING COMPLETE VISUAL PROOF DWG FOR OWNER");
            ed.WriteMessage("\n=======================================================");

            ObjectId dotArrowId = Toado100.EnsureDotArrow(db);
            ToadoSettings s = new ToadoSettings();
            s.Height = 2.5;
            s.ArrowSize = 1.5;
            s.CircleArrow = (dotArrowId != ObjectId.Null);
            s.UnderlineBetween = true;
            s.Vn2000 = true;
            s.Decimals = 3;
            s.Layer = "TOA_DO";

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                Toado100.EnsureRegApp(tr, db);
                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                ObjectId styleId = db.Textstyle;

                // 1. Label all 11 DPT blocks in the drawing
                int dptCount = 0;
                Point3d firstDptPos = Point3d.Origin;
                foreach (ObjectId id in ms) {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj is BlockReference) {
                        BlockReference br = (BlockReference)obj;
                        if (br.Name == "DPT") {
                            dptCount++;
                            Point3d targetPt = br.Position;
                            if (dptCount == 1) firstDptPos = targetPt;
                            string bName = Toado100.NameOf(tr, br);
                            string handle = br.Handle.ToString();

                            Point3d landingPt;
                            if (dptCount % 2 == 1) {
                                landingPt = new Point3d(targetPt.X, targetPt.Y + 20.0, targetPt.Z); // Vertical 90 deg
                            } else {
                                landingPt = new Point3d(targetPt.X + 20.0, targetPt.Y + 15.0, targetPt.Z); // Slanted 45 deg
                            }

                            Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, bName, handle, ObjectId.Null, layerId, styleId, dotArrowId);
                        }
                    }
                }
                ed.WriteMessage(string.Format("\nLabeled {0} DPT block references.", dptCount));

                // 2. Generate 16-angle Compass Star at dedicated benchmark location
                // Benchmark center placed 100m to the right of first DPT block
                Point3d starCenter = new Point3d(firstDptPos.X + 120.0, firstDptPos.Y, firstDptPos.Z);
                double radius = 40.0;
                double[] angles = new double[] { 0, 15, 45, 75, 90, 105, 135, 165, 180, 195, 225, 255, 270, 285, 315, 345 };

                // Draw center circle for benchmark
                Circle centerCirc = new Circle(starCenter, Vector3d.ZAxis, 2.0);
                centerCirc.LayerId = layerId;
                ms.AppendEntity(centerCirc);
                tr.AddNewlyCreatedDBObject(centerCirc, true);

                DBText title = new DBText();
                title.TextString = "T-TOADO 360-DEGREE ROTATION TEST BENCHMARK (16 ANGLES)";
                title.Position = new Point3d(starCenter.X - 50.0, starCenter.Y + radius + 15.0, 0);
                title.Height = 3.5;
                title.LayerId = layerId;
                ms.AppendEntity(title);
                tr.AddNewlyCreatedDBObject(title, true);

                foreach (double deg in angles) {
                    double rad = deg * Math.PI / 180.0;
                    double dx = radius * Math.Cos(rad);
                    double dy = radius * Math.Sin(rad);
                    Point3d landingPt = new Point3d(starCenter.X + dx, starCenter.Y + dy, starCenter.Z);

                    string angleTag = string.Format("ANG_{0:0}", deg);
                    Toado100.CreateOrUpdateLeader(db, tr, ms, starCenter, landingPt, s, angleTag, "BENCH_" + deg, ObjectId.Null, layerId, styleId, dotArrowId);
                }

                tr.Commit();
                ed.WriteMessage("\nCreated 16-angle radial benchmark star.");
            }

            string outPath = @"D:\TuanPa_AI\OUTPUT\Mat bang cap dien trung the_TOADO_TEST.dwg";
            try {
                db.SaveAs(outPath, DwgVersion.Current);
                ed.WriteMessage("\n>>> Saved comprehensive visual proof DWG to: " + outPath);
            } catch (System.Exception ex) {
                ed.WriteMessage("\nSaveAs error: " + ex.Message);
            }
        }
    }
}
