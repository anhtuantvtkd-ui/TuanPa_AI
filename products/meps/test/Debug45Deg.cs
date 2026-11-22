using System;
using System.IO;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class Debug45Deg {
        [CommandMethod("DEBUG_45_DEG")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            try {
                ToadoSettings s = Toado100.Load();
                ed.WriteMessage(string.Format("\n[DEBUG] Settings from toado.ini: Height={0}, Distance={1}, PickEach={2}", s.Height, s.Distance, s.PickEach));

                ObjectId dotArrowId = Toado100.EnsureDotArrow(db);

                using (Transaction tr = db.TransactionManager.StartTransaction()) {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    Toado100.EnsureRegApp(tr, db);
                    ObjectId layerId = Toado100.EnsureLayer(tr, db, s.Layer);
                    ObjectId styleId = db.Textstyle;

                    // Find first DPT block
                    BlockReference br = null;
                    foreach (ObjectId id in ms) {
                        DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                        if (obj is BlockReference) {
                            BlockReference b = (BlockReference)obj;
                            if (b.Name == "DPT") {
                                br = b;
                                break;
                            }
                        }
                    }

                    if (br != null) {
                        Point3d targetPt = br.Position;
                        Point3d landingPt = new Point3d(targetPt.X + s.Distance, targetPt.Y + s.Distance, targetPt.Z);
                        ed.WriteMessage(string.Format("\n[DEBUG] TargetPt=({0},{1}), LandingPt=({2},{3}), dx={4}, dy={5}",
                            targetPt.X, targetPt.Y, landingPt.X, landingPt.Y, landingPt.X - targetPt.X, landingPt.Y - targetPt.Y));

                        ObjectId mlId = Toado100.CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, "DPT", br.Handle.ToString(), ObjectId.Null, layerId, styleId, dotArrowId);
                        MLeader ml = tr.GetObject(mlId, OpenMode.ForRead) as MLeader;
                        if (ml != null) {
                            Point3d v0 = ml.GetFirstVertex(0);
                            Point3d v1 = ml.GetLastVertex(0);
                            ed.WriteMessage(string.Format("\n[DEBUG] MLeader Vertices: V0=({0:0.##},{1:0.##}), V1=({2:0.##},{3:0.##})", v0.X, v0.Y, v1.X, v1.Y));
                            if (ml.MText != null) {
                                ed.WriteMessage(string.Format("\n[DEBUG] MText: Contents='{0}', Rotation={1} rad ({2} deg), Location=({3:0.##},{4:0.##})",
                                    ml.MText.Contents.Replace("\n", " "), ml.MText.Rotation, ml.MText.Rotation * 180 / Math.PI, ml.TextLocation.X, ml.TextLocation.Y));
                            }
                        }
                    } else {
                        ed.WriteMessage("\n[DEBUG] No DPT block found!");
                    }
                    tr.Commit();
                }
            } catch (System.Exception ex) {
                ed.WriteMessage("\n[DEBUG EXCEPTION] " + ex.ToString());
            }
        }
    }
}
