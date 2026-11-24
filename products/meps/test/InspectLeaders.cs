using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

public class InspectLeaders {
    [CommandMethod("INSPECT_LEADERS")]
    public static void Run() {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;
        using (var tr = db.TransactionManager.StartTransaction()) {
            var ms = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
            int count = 0;
            foreach (ObjectId id in ms) {
                if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase)) {
                    count++;
                    var ml = (MLeader)tr.GetObject(id, OpenMode.ForRead);
                    ed.WriteMessage(string.Format("\n=== MLeader #{0} [Handle {1}] ===", count, ml.Handle));
                    if (ml.MText != null) {
                        ed.WriteMessage(string.Format("\n  MText Contents: '{0}'", ml.MText.Contents));
                        ed.WriteMessage(string.Format("\n  TextLocation: {0}", ml.TextLocation));
                        ed.WriteMessage(string.Format("\n  BackgroundFill: {0}, UseBackgroundColor: {1}", ml.MText.BackgroundFill, ml.MText.UseBackgroundColor));
                    }
                    ed.WriteMessage(string.Format("\n  TextAttachmentType: {0}", ml.TextAttachmentType));
                    ed.WriteMessage(string.Format("\n  ExtendLeaderToText: {0}", ml.ExtendLeaderToText));
                    ed.WriteMessage(string.Format("\n  DoglegLength: {0}, EnableDogleg: {1}, EnableLanding: {2}", ml.DoglegLength, ml.EnableDogleg, ml.EnableLanding));
                    ed.WriteMessage(string.Format("\n  LeaderCount: {0}", ml.LeaderCount));
                    for (int i = 0; i < ml.LeaderCount; i++) {
                        foreach (int lineIdx in ml.GetLeaderLineIndexes(i)) {
                            Point3d v0 = ml.GetFirstVertex(lineIdx);
                            Point3d v1 = ml.GetLastVertex(lineIdx);
                            ed.WriteMessage(string.Format("\n    Line {0}: {1} -> {2}", lineIdx, v0, v1));
                        }
                    }
                    var xdata = ml.GetXDataForApplication("TVXD_TOADO");
                    if (xdata != null) {
                        ed.WriteMessage("\n  XData: ");
                        foreach (TypedValue tv in xdata) {
                            ed.WriteMessage(string.Format("[{0}:{1}] ", tv.TypeCode, tv.Value));
                        }
                    }
                }
            }
            ed.WriteMessage(string.Format("\nTotal MLeaders: {0}\n", count));
            tr.Commit();
        }
    }
}
