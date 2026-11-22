using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class TestBatchAllBlocks {
        [CommandMethod("TEST_BATCH_ALL_BLOCKS")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            try {
                ToadoSettings s = Toado100.Load();
                s.PickEach = false; // Tự động lệch góc cố định 45°
                ObjectId dotArrowId = Toado100.EnsureDotArrow(db);

                using (Transaction tr = db.TransactionManager.StartTransaction()) {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    Toado100.EnsureRegApp(tr, db);
                    ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                    ObjectId styleId = db.Textstyle;

                    var map = Toado100.ScanExistingLeaders(tr, ms);
                    int count = 0;

                    foreach (ObjectId id in ms) {
                        DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                        if (obj is BlockReference) {
                            BlockReference br = (BlockReference)obj;
                            if (br.Name == "DPT") {
                                count++;
                                Point3d targetPt = Toado100.Tip(br, s.Center);
                                string blockName = Toado100.NameOf(tr, br);
                                string handleStr = br.Handle.ToString();

                                Point3d landingPt = new Point3d(targetPt.X + s.Distance, targetPt.Y + s.Distance, targetPt.Z);

                                ObjectId existingId = map.ContainsKey(handleStr) ? map[handleStr] : ObjectId.Null;
                                ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, blockName, handleStr, existingId, layerId, styleId, dotArrowId);

                                MLeader ml = tr.GetObject(mlId, OpenMode.ForRead) as MLeader;
                                double textRot = ml.MText != null ? ml.MText.Rotation * 180 / Math.PI : 0;
                                Point3d v0 = ml.GetFirstVertex(0);
                                Point3d v1 = ml.GetLastVertex(0);
                                double leaderAngle = Math.Atan2(v1.Y - v0.Y, v1.X - v0.X) * 180 / Math.PI;

                                ed.WriteMessage(string.Format("\n[BLOCK #{0}] Handle={1}, LeaderAngle={2:0.##} deg, TextRot={3:0.##} deg, V0=({4:0.##},{5:0.##}), V1=({6:0.##},{7:0.##})",
                                    count, handleStr, leaderAngle, textRot, v0.X, v0.Y, v1.X, v1.Y));
                            }
                        }
                    }
                    tr.Commit();
                }
            } catch (System.Exception ex) {
                ed.WriteMessage("\n[EX] " + ex.ToString());
            }
        }
    }
}
