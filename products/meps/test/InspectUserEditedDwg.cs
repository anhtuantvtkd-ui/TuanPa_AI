using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Tvxd.Meps;

namespace Tvxd.Meps.Test {
    public class InspectUserEditedDwg {
        [CommandMethod("INSPECT_USER_EDITED")]
        public static void Run() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                int count = 0;
                foreach (ObjectId id in ms) {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj is MLeader) {
                        count++;
                        MLeader ml = (MLeader)obj;
                        Point3d v0 = Point3d.Origin;
                        Point3d v1 = Point3d.Origin;
                        try { v0 = ml.GetFirstVertex(0); } catch {}
                        try { v1 = ml.GetLastVertex(0); } catch {}

                        string txt = ml.MText != null ? ml.MText.Contents.Replace("\n", " ") : "";
                        double rot = ml.MText != null ? ml.MText.Rotation * 180 / Math.PI : 0;

                        // Read XData
                        string handleStr = "";
                        Point3d xdataTarget = Point3d.Origin;
                        Point3d xdataLanding = Point3d.Origin;
                        ResultBuffer rb = ml.GetXDataForApplication(Toado100.Reg);
                        if (rb != null) {
                            var arr = rb.AsArray();
                            if (arr.Length > 1) handleStr = Convert.ToString(arr[1].Value);
                            if (arr.Length >= 5) xdataTarget = new Point3d(Convert.ToDouble(arr[2].Value), Convert.ToDouble(arr[3].Value), Convert.ToDouble(arr[4].Value));
                            if (arr.Length >= 8) xdataLanding = new Point3d(Convert.ToDouble(arr[5].Value), Convert.ToDouble(arr[6].Value), Convert.ToDouble(arr[7].Value));
                            rb.Dispose();
                        }

                        ed.WriteMessage(string.Format("\n[ML #{0}] Handle={1}, Text='{2}', Rot={3:0.#} deg, V0=({4:0.##},{5:0.##}), V1=({6:0.##},{7:0.##}), XDataH={8}, XDTarget=({9:0.##},{10:0.##}), XDLanding=({11:0.##},{12:0.##})",
                            count, ml.Handle, txt, rot, v0.X, v0.Y, v1.X, v1.Y, handleStr, xdataTarget.X, xdataTarget.Y, xdataLanding.X, xdataLanding.Y));
                    }
                }
                ed.WriteMessage(string.Format("\nTotal MLeaders: {0}", count));
                tr.Commit();
            }
        }
    }
}
