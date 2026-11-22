using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class TestCopyLeader {
        [CommandMethod("TEST_COPY_LEADER")]
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

                    // Find Block 1 (DPT #1) and Block 2 (DPT #2)
                    BlockReference b1 = null, b2 = null;
                    foreach (ObjectId id in ms) {
                        DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                        if (obj is BlockReference) {
                            BlockReference br = (BlockReference)obj;
                            if (br.Name == "DPT") {
                                if (b1 == null) b1 = br;
                                else if (b2 == null) { b2 = br; break; }
                            }
                        }
                    }

                    if (b1 == null || b2 == null) {
                        ed.WriteMessage("\nCould not find 2 DPT blocks.");
                        return;
                    }

                    ed.WriteMessage(string.Format("\n[TEST] B1 at ({0:0.##},{1:0.##}), B2 at ({2:0.##},{3:0.##})",
                        b1.Position.X, b1.Position.Y, b2.Position.X, b2.Position.Y));

                    // Step 1: Create Leader for B1
                    Point3d target1 = b1.Position;
                    Point3d landing1 = new Point3d(target1.X + 20, target1.Y + 15, target1.Z);
                    ObjectId mlId1 = Toado100.CreateOrUpdateLeader(db, tr, ms, target1, landing1, s, "DPT", b1.Handle.ToString(), ObjectId.Null, layerId, styleId, dotArrowId);
                    MLeader ml1 = tr.GetObject(mlId1, OpenMode.ForRead) as MLeader;
                    ed.WriteMessage(string.Format("\n[STEP 1] Created ML1: V0=({0:0.##},{1:0.##}), Text='{2}'",
                        ml1.GetFirstVertex(0).X, ml1.GetFirstVertex(0).Y, ml1.MText.Contents.Replace("\n", " ")));

                    // Step 2: Simulate AutoCAD COPY of ML1 to B2
                    // In CAD, copying duplicates the entity including XData, then user moves it so V0 is at B2.Position
                    MLeader ml2 = (MLeader)ml1.Clone();
                    // User moves arrow to B2
                    Point3d target2 = b2.Position;
                    Point3d landing2 = new Point3d(target2.X + 20, target2.Y + 15, target2.Z);
                    ml2.SetFirstVertex(0, target2);
                    ml2.SetLastVertex(0, landing2);
                    ms.AppendEntity(ml2);
                    tr.AddNewlyCreatedDBObject(ml2, true);

                    ed.WriteMessage(string.Format("\n[STEP 2] Copied ML2 to B2: V0=({0:0.##},{1:0.##}), but XData still points to B1 ({2})",
                        ml2.GetFirstVertex(0).X, ml2.GetFirstVertex(0).Y, b1.Handle.ToString()));

                    tr.Commit();
                }

                // Step 3: Run RefreshAll
                ed.WriteMessage("\n[STEP 3] Running RefreshAll...");
                Toado100.RefreshAll(doc, s);

                // Step 4: Verify positions of ML1 and ML2 after RefreshAll
                using (Transaction tr = db.TransactionManager.StartTransaction()) {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    int count = 0;
                    foreach (ObjectId id in ms) {
                        DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                        if (obj is MLeader) {
                            MLeader ml = (MLeader)obj;
                            ResultBuffer rb = ml.GetXDataForApplication(Toado100.Reg);
                            if (rb != null) {
                                count++;
                                Point3d v0 = ml.GetFirstVertex(0);
                                Point3d v1 = ml.GetLastVertex(0);
                                ed.WriteMessage(string.Format("\n[RESULT ML #{0}] V0=({1:0.##},{2:0.##}), V1=({3:0.##},{4:0.##}), Text='{5}'",
                                    count, v0.X, v0.Y, v1.X, v1.Y, ml.MText != null ? ml.MText.Contents.Replace("\n", " ") : ""));
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
