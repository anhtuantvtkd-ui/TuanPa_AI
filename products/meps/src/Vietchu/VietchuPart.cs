using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcColor = Autodesk.AutoCAD.Colors.Color;

namespace Tvxd.Meps
{
    public static class TextWrite110
    {
        public static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        public const string StyleName = "Tuan_arial";
        public const string LayerName = "T_TEXT";
        public const double DefaultHeight = 250.0;
        public const double DefaultWeight = 0.18;

        public class Opts
        {
            public double Height = DefaultHeight;
            public double Weight = DefaultWeight;
            public double Rotation = 0.0;
            public string Style = StyleName;
        }

        public class Written
        {
            public ObjectId id = ObjectId.Null;
            public List<string> warnings = new List<string>();
        }

        public static string IniPath
        {
            get
            {
                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dir = Path.Combine(local, @"TVXD\MEPS060");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return Path.Combine(dir, "vietchu.ini");
            }
        }

        public static Opts Load()
        {
            var opt = new Opts();
            try
            {
                string path = IniPath;
                if (File.Exists(path))
                {
                    foreach (string line in File.ReadAllLines(path))
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            string k = parts[0].Trim().ToUpperInvariant();
                            string v = parts[1].Trim();
                            if (k == "HEIGHT") double.TryParse(v, NumberStyles.Float, Inv, out opt.Height);
                        }
                    }
                }
            }
            catch { }
            if (opt.Height <= 0) opt.Height = DefaultHeight;
            return opt;
        }

        public static void Save(Opts opt)
        {
            try
            {
                string path = IniPath;
                File.WriteAllLines(path, new string[] {
                    "HEIGHT=" + opt.Height.ToString("0.###", Inv)
                });
            }
            catch { }
        }

        public static ObjectId EnsureLayer(Database db, Transaction tr, string layerName)
        {
            LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (!lt.Has(layerName))
            {
                lt.UpgradeOpen();
                var ltr = new LayerTableRecord
                {
                    Name = layerName,
                    Color = AcColor.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 7), // Mau trang
                    LineWeight = LineWeight.LineWeight018
                };
                ObjectId id = lt.Add(ltr);
                tr.AddNewlyCreatedDBObject(ltr, true);
                return id;
            }
            return lt[layerName];
        }

        public static ObjectId EnsureTextStyle(Database db, Transaction tr, string styleName)
        {
            TextStyleTable st = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);
            if (!st.Has(styleName))
            {
                st.UpgradeOpen();
                var str = new TextStyleTableRecord
                {
                    Name = styleName,
                    FileName = "arial.ttf"
                };
                ObjectId id = st.Add(str);
                tr.AddNewlyCreatedDBObject(str, true);
                return id;
            }
            return st[styleName];
        }

        public static Written WriteMText(Database db, Point3d pos, string text, Opts opt, double ucsAngle)
        {
            if (string.IsNullOrEmpty(text)) throw new Exception("Nội dung chữ trống.");
            if (opt.Height <= 0) throw new Exception("Chiều cao chữ phải lớn hơn 0.");

            var res = new Written();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ObjectId styleId = EnsureTextStyle(db, tr, StyleName);
                EnsureLayer(db, tr, LayerName);

                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite);

                var mt = new MText();
                mt.SetDatabaseDefaults(db);
                mt.Layer = LayerName;
                mt.Color = AcColor.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 7);
                mt.LineWeight = LineWeight.LineWeight018;
                mt.TextStyleId = styleId;
                mt.TextHeight = opt.Height;
                mt.Contents = text;
                mt.Location = pos;
                mt.Rotation = ucsAngle;

                res.id = btr.AppendEntity(mt);
                tr.AddNewlyCreatedDBObject(mt, true);
                tr.Commit();
            }
            return res;
        }
    }

    public class TextWrite110Commands
    {
        [CommandMethod("T-VIETCHU", CommandFlags.Modal)]
        public void Run()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            TextWrite110.Opts opt = TextWrite110.Load();

            // Prompt ONLY for Height (Nhap chieu cao chu)
            var pdo = new PromptDoubleOptions("\nChiều cao chữ <" + opt.Height.ToString("0.###", TextWrite110.Inv) + ">: ");
            pdo.DefaultValue = opt.Height;
            pdo.UseDefaultValue = true;
            pdo.AllowNegative = false;
            pdo.AllowZero = false;

            PromptDoubleResult pdr = ed.GetDouble(pdo);
            if (pdr.Status != PromptStatus.OK) return;

            opt.Height = pdr.Value;
            TextWrite110.Save(opt);

            // Calculate UCS Angle
            CoordinateSystem3d ucs = ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            double ucsAngle = Vector3d.XAxis.GetAngleTo(ucs.Xaxis, Vector3d.ZAxis);

            // Infinite loop to place MText
            while (true)
            {
                var ppo = new PromptPointOptions("\nChọn điểm chèn chữ: ");
                PromptPointResult ppr = ed.GetPoint(ppo);
                if (ppr.Status != PromptStatus.OK) break;

                Point3d insertPt = ppr.Value.TransformBy(ed.CurrentUserCoordinateSystem);

                var pso = new PromptStringOptions("\nNhập nội dung chữ: ");
                pso.AllowSpaces = true;
                PromptResult psr = ed.GetString(pso);
                if (psr.Status != PromptStatus.OK || string.IsNullOrEmpty(psr.StringResult)) break;

                try
                {
                    TextWrite110.WriteMText(db, insertPt, psr.StringResult, opt, ucsAngle);
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage("\nLỗi tạo MText: " + ex.Message);
                    break;
                }
            }
        }
    }
}
