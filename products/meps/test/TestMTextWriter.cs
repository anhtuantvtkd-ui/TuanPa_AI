using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Tvxd.Meps.Test
{
    public class TestMTextWriter
    {
        [CommandMethod("TEST_MTEXT_WRITER")]
        public void Run()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            string reportPath = @"D:\TuanPa_AI\products\meps\test\mtext_native_report.txt";

            var opt = new Tvxd.Meps.TextWrite110.Opts();
            opt.Height = 250.0;

            var res = Tvxd.Meps.TextWrite110.WriteMText(db, new Point3d(100, 200, 0), "TEST NOIDUNG MTEXT V11", opt, 0.0);

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                var ent = tr.GetObject(res.id, OpenMode.ForRead);
                var mt = ent as MText;
                if (mt == null)
                {
                    File.WriteAllText(reportPath, "FAIL: Entity is not MText. Actual: " + ent.GetType().FullName);
                    return;
                }

                string report = string.Format(
                    "PASS: Entity is AcDbMText\nLayer: {0}\nColor: {1}\nLineWeight: {2}\nTextHeight: {3}\nContents: {4}",
                    mt.Layer,
                    mt.Color.ColorIndex,
                    (int)mt.LineWeight,
                    mt.TextHeight,
                    mt.Contents
                );
                File.WriteAllText(reportPath, report);
                tr.Commit();
            }
        }
    }
}
