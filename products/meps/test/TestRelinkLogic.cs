using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

public class TestRelinkLogic {
    [CommandMethod("TEST_RELINK")]
    public static void Run() {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        try {
            ToadoSettings s = Toado100.Load();
            ObjectId dotArrowId = Toado100.EnsureDotArrow(db);

            BlockReference b1 = null, b2 = null;
            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                Toado100.EnsureRegApp(tr, db);
                ObjectId styleId = Toado100.EnsureTextStyle(tr, db, s.Style);

                // Find 2 DPT blocks
                foreach (ObjectId id in ms) {
                    var obj = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                    if (obj != null && obj.Name == "DPT") {
                        if (b1 == null) b1 = obj;
                        else if (b2 == null) { b2 = obj; break; }
                    }
                }

                if (b1 == null || b2 == null) {
                    ed.WriteMessage("\nCould not find 2 DPT blocks.");
                    return;
                }

                ed.WriteMessage(string.Format("\nB1 Handle={0} at ({1:0.##},{2:0.##})", b1.Handle, b1.Position.X, b1.Position.Y));
                ed.WriteMessage(string.Format("\nB2 Handle={0} at ({1:0.##},{2:0.##})", b2.Handle, b2.Position.X, b2.Position.Y));

                // Step 1: Create Leader on B1
                Point3d target1 = b1.Position;
                Point3d landing1 = new Point3d(target1.X + 25, target1.Y + 25, target1.Z);
                ObjectId mlId1 = Toado100.CreateOrUpdateLeader(db, tr, ms, target1, landing1, s, "DPT", b1.Handle.ToString(), ObjectId.Null, layerId, styleId, dotArrowId);

                // Step 2: Clone Leader 1 to B2 (simulating AutoCAD COPY)
                var ml1 = (MLeader)tr.GetObject(mlId1, OpenMode.ForRead);
                var ml2 = (MLeader)ml1.Clone();
                Point3d target2 = b2.Position;
                Point3d landing2 = new Point3d(target2.X + 25, target2.Y + 25, target2.Z);
                ml2.SetFirstVertex(0, target2);
                ml2.SetLastVertex(0, landing2);
                ObjectId mlId2 = ms.AppendEntity(ml2);
                tr.AddNewlyCreatedDBObject(ml2, true);

                ed.WriteMessage(string.Format("\nCreated ML1 ({0}) on B1, Copied ML2 ({1}) on B2", mlId1.Handle, mlId2.Handle));
                tr.Commit();
            }

            // Step 3: Run Re-linking Refresh
            ed.WriteMessage("\nRunning RefreshAll...");
            Toado100.RefreshAll(doc, s);

            // Step 4: Verify positions and text of ML1 and ML2
            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                int idx = 0;
                foreach (ObjectId id in ms) {
                    if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase)) {
                        var ml = tr.GetObject(id, OpenMode.ForRead) as MLeader;
                        var xdata = ml.GetXDataForApplication(Toado100.Reg);
                        if (xdata != null) {
                            idx++;
                            Point3d v0 = ml.GetFirstVertex(0);
                            Point3d v1 = ml.GetLastVertex(0);
                            TypedValue[] vals = xdata.AsArray();
                            string h = vals.Length > 1 ? Convert.ToString(vals[1].Value) : "";
                            ed.WriteMessage(string.Format("\n[LEADER #{0} Handle {1}] Target=({2:0.##},{3:0.##}), Landing=({4:0.##},{5:0.##}), LinkedBlockHandle={6}, Text='{7}'",
                                idx, ml.Handle, v0.X, v0.Y, v1.X, v1.Y, h, ml.MText != null ? ml.MText.Contents.Replace("\n", " ") : ""));
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
