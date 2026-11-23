using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Tvxd.Meps {
    public sealed class ToadoSettings {
        public string Style = "Tuan_arial";
        public string Layer = "TOA_DO";
        public double Height = 2.5;
        public double Rotation = 0.0;
        public double ArrowSize = 1.5;
        public double Distance = 15.0;
        public int Decimals = 3;
        public int Corner = 0;
        public bool Vn2000 = true;
        public bool TwoLines = true;
        public bool Mask = false;
        public bool Frame = false;
        public bool Arrow = true;
        public bool Center = false;
        public bool AddName = false;
        public bool PickEach = true;
        public bool Reformat = false;
        public bool UnitIsMm = false;
        public bool CircleArrow = true;
        public bool UnderlineBetween = true;

        public static string IniPath {
            get {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TVXD", "MEPS060");
                try { Directory.CreateDirectory(dir); } catch {}
                return Path.Combine(dir, "toado.ini");
            }
        }
    }

    public sealed class ToadoResult {
        public int created = 0;
        public int updated = 0;
        public int same = 0;
        public int moved = 0;
        public int skipped = 0;
        public List<string> warnings = new List<string>();
    }

    public sealed class Toado100Form : Form {
        public ToadoSettings S;
        public bool TriggerRefreshOnly = false;

        TextBox txtStyle, txtHeight, txtArrowSize, txtDecimals, txtDistance;
        RadioButton rbVn2000, rbCad, rbUnitM, rbUnitMm, rbPickFree, rbBatch;
        CheckBox chkMask, chkCircle, chkUnderline, chkAddName;

        public Toado100Form(ToadoSettings s) {
            S = s ?? new ToadoSettings();
            InitializeComponent();
        }

        public Toado100Form(ToadoSettings s, IList<string> styles, int count) : this(s) {}

        void InitializeComponent() {
            this.Text = "T-TOADO — Nhãn Tọa Độ VN-2000 (Bản Nâng Cấp)";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(530, 570);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular);

            int y = 15;
            // Group 1
            var grpLook = new GroupBox { Text = "1. Trình bày & Đầu mút hình tròn", Location = new System.Drawing.Point(15, y), Size = new Size(485, 145) };
            
            var lblH = new Label { Text = "Chiều cao chữ:", Location = new System.Drawing.Point(15, 25), AutoSize = true };
            txtHeight = new TextBox { Text = S.Height.ToString("0.##", CultureInfo.InvariantCulture), Location = new System.Drawing.Point(120, 22), Width = 70 };
            
            var lblArr = new Label { Text = "Kích thước đầu tròn:", Location = new System.Drawing.Point(220, 25), AutoSize = true };
            txtArrowSize = new TextBox { Text = S.ArrowSize.ToString("0.##", CultureInfo.InvariantCulture), Location = new System.Drawing.Point(375, 22), Width = 70 };

            var lblDec = new Label { Text = "Chữ số sau phẩy:", Location = new System.Drawing.Point(15, 58), AutoSize = true };
            txtDecimals = new TextBox { Text = S.Decimals.ToString(), Location = new System.Drawing.Point(120, 55), Width = 70 };

            var lblStyle = new Label { Text = "Kiểu chữ (Style):", Location = new System.Drawing.Point(220, 58), AutoSize = true };
            txtStyle = new TextBox { Text = S.Style, Location = new System.Drawing.Point(375, 55), Width = 95 };

            chkCircle = new CheckBox { Text = "● Đầu mũi tên là HÌNH TRÒN (Dot)", Checked = S.CircleArrow, Location = new System.Drawing.Point(15, 90), AutoSize = true, ForeColor = System.Drawing.Color.DarkGreen, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            chkUnderline = new CheckBox { Text = "━ Đường gióng XEN GIỮA 2 dòng X/Y", Checked = S.UnderlineBetween, Location = new System.Drawing.Point(15, 115), AutoSize = true, ForeColor = System.Drawing.Color.DarkBlue, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };

            grpLook.Controls.AddRange(new Control[] { lblH, txtHeight, lblArr, txtArrowSize, lblDec, txtDecimals, lblStyle, txtStyle, chkCircle, chkUnderline });
            this.Controls.Add(grpLook);

            // Group 2
            y += 155;
            var grpCoord = new GroupBox { Text = "2. Hệ tọa độ VN-2000 & Đơn vị", Location = new System.Drawing.Point(15, y), Size = new Size(485, 110) };
            
            rbVn2000 = new RadioButton { Text = "VN-2000 (Dòng trên X = Trục Y CAD, Dòng dưới Y = Trục X CAD)", Checked = S.Vn2000, Location = new System.Drawing.Point(15, 23), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = System.Drawing.Color.Firebrick };
            rbCad = new RadioButton { Text = "Hệ CAD thuần (X = Trục ngang X, Y = Trục đứng Y)", Checked = !S.Vn2000, Location = new System.Drawing.Point(15, 48), AutoSize = true };

            var lblUnit = new Label { Text = "Đơn vị CAD:", Location = new System.Drawing.Point(15, 78), AutoSize = true };
            rbUnitM = new RadioButton { Text = "Mét (m)", Checked = !S.UnitIsMm, Location = new System.Drawing.Point(120, 76), AutoSize = true };
            rbUnitMm = new RadioButton { Text = "Milimét (mm - tự chia 1000 ra mét)", Checked = S.UnitIsMm, Location = new System.Drawing.Point(220, 76), AutoSize = true };

            grpCoord.Controls.AddRange(new Control[] { rbVn2000, rbCad, lblUnit, rbUnitM, rbUnitMm });
            this.Controls.Add(grpCoord);

            // Group 3
            y += 120;
            var grpPlace = new GroupBox { Text = "3. Hướng đặt nhãn & Tùy chọn", Location = new System.Drawing.Point(15, y), Size = new Size(485, 110) };

            rbPickFree = new RadioButton { Text = "Click chuột chọn hướng tự do 360° (Khuyên dùng)", Checked = S.PickEach, Location = new System.Drawing.Point(15, 22), AutoSize = true, ForeColor = System.Drawing.Color.DarkGreen, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            rbBatch = new RadioButton { Text = "Tự động lệch góc cố định 45°", Checked = !S.PickEach, Location = new System.Drawing.Point(15, 47), AutoSize = true };

            var lblDist = new Label { Text = "Khoảng cách (Distance):", Location = new System.Drawing.Point(220, 48), AutoSize = true };
            double initialDist = (S.Distance >= S.Height) ? S.Distance : (S.Height * 3.5);
            txtDistance = new TextBox { Text = initialDist.ToString("0.##", CultureInfo.InvariantCulture), Location = new System.Drawing.Point(375, 45), Width = 70 };

            chkMask = new CheckBox { Text = "Nền che nét (Mask)", Checked = S.Mask, Location = new System.Drawing.Point(15, 78), AutoSize = true };
            chkAddName = new CheckBox { Text = "Ghi kèm Tên/TAG Block", Checked = S.AddName, Location = new System.Drawing.Point(220, 78), AutoSize = true };

            grpPlace.Controls.AddRange(new Control[] { rbPickFree, rbBatch, lblDist, txtDistance, chkMask, chkAddName });
            this.Controls.Add(grpPlace);

            // Buttons
            y += 120;
            var btnOk = new Button { Text = "▶ ĐẶT TỌA ĐỘ", Location = new System.Drawing.Point(25, y), Size = new Size(135, 40), BackColor = System.Drawing.Color.LightSteelBlue, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            btnOk.Click += (sender, e) => {
                Collect();
                Toado100.Save(S);
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            var btnRefresh = new Button { Text = "🔄 CẬP NHẬT TẤT CẢ", Location = new System.Drawing.Point(175, y), Size = new Size(185, 40), BackColor = System.Drawing.Color.LemonChiffon, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            btnRefresh.Click += (sender, e) => {
                Collect();
                Toado100.Save(S);
                TriggerRefreshOnly = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            var btnCancel = new Button { Text = "Đóng", Location = new System.Drawing.Point(375, y), Size = new Size(110, 40) };
            btnCancel.Click += (sender, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.AddRange(new Control[] { btnOk, btnRefresh, btnCancel });
        }

        void Collect() {
            double.TryParse(txtHeight.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out S.Height);
            if (S.Height <= 0) S.Height = 2.5;

            double.TryParse(txtArrowSize.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out S.ArrowSize);
            if (S.ArrowSize <= 0) S.ArrowSize = 1.5;

            double.TryParse(txtDistance.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out S.Distance);
            if (S.Distance <= 0) S.Distance = S.Height * 3.5;

            int.TryParse(txtDecimals.Text, out S.Decimals);
            if (S.Decimals < 0 || S.Decimals > 6) S.Decimals = 3;

            S.Style = string.IsNullOrWhiteSpace(txtStyle.Text) ? "Tuan_arial" : txtStyle.Text.Trim();
            S.Vn2000 = rbVn2000.Checked;
            S.UnitIsMm = rbUnitMm.Checked;
            S.CircleArrow = chkCircle.Checked;
            S.UnderlineBetween = chkUnderline.Checked;
            S.PickEach = rbPickFree.Checked;
            S.Mask = chkMask.Checked;
            S.AddName = chkAddName.Checked;
        }
    }

    public static class Toado100 {
        public const string Reg = "TVXD_TOADO";
        public static string IniOverride = null;
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string IniPath {
            get {
                if (IniOverride != null) return IniOverride;
                return ToadoSettings.IniPath;
            }
        }

        public static ToadoSettings Load() {
            var s = new ToadoSettings();
            try {
                if (File.Exists(IniPath)) {
                    foreach (var line in File.ReadAllLines(IniPath, System.Text.Encoding.UTF8)) {
                        int eq = line.IndexOf('=');
                        if (eq <= 0) continue;
                        string k = line.Substring(0, eq).Trim();
                        string v = line.Substring(eq + 1).Trim();
                        if (k.Equals("Style", StringComparison.OrdinalIgnoreCase)) s.Style = v;
                        else if (k.Equals("Layer", StringComparison.OrdinalIgnoreCase)) s.Layer = v;
                        else if (k.Equals("Height", StringComparison.OrdinalIgnoreCase)) double.TryParse(v, NumberStyles.Float, Inv, out s.Height);
                        else if (k.Equals("ArrowSize", StringComparison.OrdinalIgnoreCase)) double.TryParse(v, NumberStyles.Float, Inv, out s.ArrowSize);
                        else if (k.Equals("Distance", StringComparison.OrdinalIgnoreCase)) double.TryParse(v, NumberStyles.Float, Inv, out s.Distance);
                        else if (k.Equals("Decimals", StringComparison.OrdinalIgnoreCase)) int.TryParse(v, out s.Decimals);
                        else if (k.Equals("Vn2000", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out s.Vn2000);
                        else if (k.Equals("UnitIsMm", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out s.UnitIsMm);
                        else if (k.Equals("Mask", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out s.Mask);
                        else if (k.Equals("Frame", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out s.Frame);
                        else if (k.Equals("Arrow", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out s.Arrow);
                        else if (k.Equals("CircleArrow", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out s.CircleArrow);
                        else if (k.Equals("UnderlineBetween", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out s.UnderlineBetween);
                        else if (k.Equals("PickEach", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out s.PickEach);
                        else if (k.Equals("AddName", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out s.AddName);
                        else if (k.Equals("TwoLines", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out s.TwoLines);
                    }
                }
                if (s.Distance <= 0 || s.Distance < s.Height * 0.5) {
                    s.Distance = s.Height * 3.5;
                }
            } catch {}
            return s;
        }

        public static void Save(ToadoSettings s) {
            try {
                using (var w = new StreamWriter(IniPath, false, System.Text.Encoding.UTF8)) {
                    w.WriteLine("Style=" + s.Style);
                    w.WriteLine("Layer=" + s.Layer);
                    w.WriteLine("Height=" + s.Height.ToString(Inv));
                    w.WriteLine("ArrowSize=" + s.ArrowSize.ToString(Inv));
                    w.WriteLine("Distance=" + s.Distance.ToString(Inv));
                    w.WriteLine("Decimals=" + s.Decimals);
                    w.WriteLine("Vn2000=" + s.Vn2000);
                    w.WriteLine("UnitIsMm=" + s.UnitIsMm);
                    w.WriteLine("Mask=" + s.Mask);
                    w.WriteLine("Frame=" + s.Frame);
                    w.WriteLine("Arrow=" + s.Arrow);
                    w.WriteLine("CircleArrow=" + s.CircleArrow);
                    w.WriteLine("UnderlineBetween=" + s.UnderlineBetween);
                    w.WriteLine("PickEach=" + s.PickEach);
                    w.WriteLine("AddName=" + s.AddName);
                    w.WriteLine("TwoLines=" + s.TwoLines);
                }
            } catch {}
        }

        public static string Esc(string text) {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}");
        }

        public static string Format(Point3d p, ToadoSettings s, string blockName, bool unused) {
            double scale = s.UnitIsMm ? 0.001 : 1.0;
            double xVal = (s.Vn2000 ? p.Y : p.X) * scale;
            double yVal = (s.Vn2000 ? p.X : p.Y) * scale;

            string fmt = s.Decimals > 0 ? "0." + new string('0', s.Decimals) : "0";
            string lineX = "X=" + xVal.ToString(fmt, Inv);
            string lineY = "Y=" + yVal.ToString(fmt, Inv);

            string text;
            if (s.TwoLines) {
                text = lineX + "\\P" + lineY;
            } else {
                text = lineX + ", " + lineY;
            }
            if (s.AddName && !string.IsNullOrEmpty(blockName)) {
                text = Esc(blockName) + "\\P" + text;
            }
            return text;
        }

        public static string NameOf(Transaction tr, BlockReference br) {
            if (br == null) return "";
            try {
                // If block has a tag/identifier attribute (e.g. TAG, SO_HIEU, TEN), prioritize it
                if (br.AttributeCollection != null && br.AttributeCollection.Count > 0) {
                    foreach (ObjectId attId in br.AttributeCollection) {
                        if (!attId.IsErased) {
                            var ar = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                            if (ar != null && (ar.Tag.Equals("TAG", StringComparison.OrdinalIgnoreCase) ||
                                               ar.Tag.Equals("SO_HIEU", StringComparison.OrdinalIgnoreCase) ||
                                               ar.Tag.Equals("TEN", StringComparison.OrdinalIgnoreCase))) {
                                if (!string.IsNullOrEmpty(ar.TextString)) {
                                    return ar.TextString.Trim();
                                }
                            }
                        }
                    }
                }

                if (br.IsDynamicBlock) {
                    var btr = (BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord, OpenMode.ForRead);
                    return btr.Name;
                }
                return br.Name;
            } catch {
                return "";
            }
        }

        public static Point3d Tip(BlockReference br, bool center) {
            if (br == null) return Point3d.Origin;
            try {
                var ext = br.GeometricExtents;
                Point3d centerPt = new Point3d(
                    (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
                    (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5,
                    (ext.MinPoint.Z + ext.MaxPoint.Z) * 0.5
                );
                // Circle arrow tip must always point accurately to the geometric center of the block!
                return centerPt;
            } catch {
                return br.Position;
            }
        }

        public static ObjectId EnsureLayer(Transaction tr, Database db, string layerName) {
            LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (!lt.Has(layerName)) {
                lt.UpgradeOpen();
                var ltr = new LayerTableRecord {
                    Name = layerName,
                    Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 2)
                };
                ObjectId id = lt.Add(ltr);
                tr.AddNewlyCreatedDBObject(ltr, true);
                return id;
            }
            return lt[layerName];
        }

        public static void EnsureRegApp(Transaction tr, Database db) {
            RegAppTable rat = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
            if (!rat.Has(Reg)) {
                rat.UpgradeOpen();
                var r = new RegAppTableRecord { Name = Reg };
                rat.Add(r);
                tr.AddNewlyCreatedDBObject(r, true);
            }
        }

        public static ObjectId EnsureTextStyle(Transaction tr, Database db, string styleName) {
            TextStyleTable tt = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);
            if (tt.Has(styleName)) return tt[styleName];
            try {
                tt.UpgradeOpen();
                var trc = new TextStyleTableRecord {
                    Name = styleName,
                    FileName = "arial.ttf",
                    TextSize = 0,
                    XScale = 1.0
                };
                ObjectId id = tt.Add(trc);
                tr.AddNewlyCreatedDBObject(trc, true);
                return id;
            } catch {
                return db.Textstyle;
            }
        }

        public static List<string> StyleNames(Database db) {
            var list = new List<string>();
            try {
                using (Transaction tr = db.TransactionManager.StartTransaction()) {
                    TextStyleTable tt = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);
                    foreach (ObjectId id in tt) {
                        var r = (TextStyleTableRecord)tr.GetObject(id, OpenMode.ForRead);
                        list.Add(r.Name);
                    }
                    tr.Commit();
                }
            } catch {}
            return list;
        }

        public static ResultBuffer Tag(string handle, Point3d p, Point3d landingPt) {
            return new ResultBuffer(
                new TypedValue((int)DxfCode.ExtendedDataRegAppName, Reg),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, handle ?? ""),
                new TypedValue((int)DxfCode.ExtendedDataReal, p.X),
                new TypedValue((int)DxfCode.ExtendedDataReal, p.Y),
                new TypedValue((int)DxfCode.ExtendedDataReal, p.Z),
                new TypedValue((int)DxfCode.ExtendedDataReal, landingPt.X),
                new TypedValue((int)DxfCode.ExtendedDataReal, landingPt.Y),
                new TypedValue((int)DxfCode.ExtendedDataReal, landingPt.Z)
            );
        }

        public static bool GetStoredPoints(MLeader ml, out Point3d targetPt, out Point3d landingPt) {
            targetPt = Point3d.Origin;
            landingPt = Point3d.Origin;
            if (ml == null || ml.LeaderCount == 0) return false;
            try {
                ResultBuffer xdata = ml.GetXDataForApplication(Reg);
                if (xdata != null) {
                    TypedValue[] vals = xdata.AsArray();
                    if (vals.Length >= 8) {
                        Point3d st = new Point3d(
                            Convert.ToDouble(vals[2].Value, Inv),
                            Convert.ToDouble(vals[3].Value, Inv),
                            Convert.ToDouble(vals[4].Value, Inv));
                        Point3d sl = new Point3d(
                            Convert.ToDouble(vals[5].Value, Inv),
                            Convert.ToDouble(vals[6].Value, Inv),
                            Convert.ToDouble(vals[7].Value, Inv));
                        Point3d curArrow = ml.GetFirstVertex(0);
                        Vector3d moveVec = curArrow - st;
                        targetPt = curArrow;
                        landingPt = sl + moveVec;
                        return true;
                    }
                }
            } catch {}
            if (ml.LeaderCount > 0) {
                targetPt = ml.GetFirstVertex(0);
                landingPt = ml.GetLastVertex(0);
                return true;
            }
            return false;
        }

        public static ObjectId EnsureDotArrow(Database db) {
            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (bt.Has("_DOT")) {
                    return bt["_DOT"];
                }
                tr.Commit();
            }
            try {
                object old = App.GetSystemVariable("DIMBLK");
                App.SetSystemVariable("DIMBLK", "_DOT");
                if (old != null) App.SetSystemVariable("DIMBLK", old);
            } catch {}

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (bt.Has("_DOT")) return bt["_DOT"];

                bt.UpgradeOpen();
                BlockTableRecord btr = new BlockTableRecord();
                btr.Name = "_DOT";
                btr.Origin = Point3d.Origin;

                Polyline pl = new Polyline();
                pl.AddVertexAt(0, new Point2d(-0.25, 0), 1.0, 0.5, 0.5);
                pl.AddVertexAt(1, new Point2d(0.25, 0), 1.0, 0.5, 0.5);
                pl.Closed = true;
                pl.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 0);

                btr.AppendEntity(pl);
                ObjectId bid = bt.Add(btr);
                tr.AddNewlyCreatedDBObject(btr, true);
                tr.AddNewlyCreatedDBObject(pl, true);
                tr.Commit();
                return bid;
            }
        }

        public static ObjectId EnsureToadoMLeaderStyle(Database db, Transaction tr, ToadoSettings s, ObjectId textStyleId, ObjectId arrowId) {
            DBDictionary dict = (DBDictionary)tr.GetObject(db.MLeaderStyleDictionaryId, OpenMode.ForRead);
            string name = "TVXD_TOADO";
            if (dict.Contains(name)) {
                ObjectId styleId = dict.GetAt(name);
                MLeaderStyle mls = (MLeaderStyle)tr.GetObject(styleId, OpenMode.ForWrite);
                mls.ContentType = ContentType.MTextContent;
                mls.EnableLanding = true;
                mls.EnableDogleg = true;
                mls.DoglegLength = 0.0;
                mls.LandingGap = s.Height * 0.1;
                mls.TextAttachmentDirection = TextAttachmentDirection.AttachmentHorizontal;
                mls.TextAttachmentType = TextAttachmentType.AttachmentBottomOfTopLine;
                mls.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.LeftLeader);
                mls.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.RightLeader);
                mls.ExtendLeaderToText = s.UnderlineBetween;
                if (arrowId != ObjectId.Null) mls.ArrowSymbolId = arrowId;
                mls.ArrowSize = s.ArrowSize;
                mls.TextHeight = s.Height;
                if (textStyleId != ObjectId.Null) mls.TextStyleId = textStyleId;
                return styleId;
            } else {
                dict.UpgradeOpen();
                MLeaderStyle mls = new MLeaderStyle();
                mls.ContentType = ContentType.MTextContent;
                mls.EnableLanding = true;
                mls.EnableDogleg = true;
                mls.DoglegLength = 0.0;
                mls.LandingGap = s.Height * 0.1;
                mls.TextAttachmentDirection = TextAttachmentDirection.AttachmentHorizontal;
                mls.TextAttachmentType = TextAttachmentType.AttachmentBottomOfTopLine;
                mls.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.LeftLeader);
                mls.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.RightLeader);
                mls.ExtendLeaderToText = s.UnderlineBetween;
                if (arrowId != ObjectId.Null) mls.ArrowSymbolId = arrowId;
                mls.ArrowSize = s.ArrowSize;
                mls.TextHeight = s.Height;
                if (textStyleId != ObjectId.Null) mls.TextStyleId = textStyleId;
                ObjectId styleId = dict.SetAt(name, mls);
                tr.AddNewlyCreatedDBObject(mls, true);
                return styleId;
            }
        }

        public static ObjectId CreateOrUpdateLeader(
            Database db,
            Transaction tr,
            BlockTableRecord ms,
            Point3d targetPt,
            Point3d landingPt,
            ToadoSettings s,
            string blockName,
            string blockHandle,
            ObjectId existingLeaderId,
            ObjectId layerId,
            ObjectId styleId,
            ObjectId dotArrowId)
        {
            ObjectId arrowId = (s.Arrow && dotArrowId != ObjectId.Null) ? dotArrowId : ObjectId.Null;
            ObjectId toadoStyleId = EnsureToadoMLeaderStyle(db, tr, s, styleId, arrowId);

            MLeader ml;
            bool isNew = (existingLeaderId == ObjectId.Null);
            if (isNew) {
                ml = new MLeader();
                ml.SetDatabaseDefaults(db);
                ml.MLeaderStyle = toadoStyleId;
                ml.ContentType = ContentType.MTextContent;
            } else {
                ml = (MLeader)tr.GetObject(existingLeaderId, OpenMode.ForWrite);
                ml.MLeaderStyle = toadoStyleId;
            }

            MText mt = new MText();
            mt.SetDatabaseDefaults(db);
            if (styleId != ObjectId.Null) mt.TextStyleId = styleId;
            mt.TextHeight = s.Height;
            mt.Contents = Format(targetPt, s, blockName, false);
            mt.BackgroundFill = s.Mask;
            if (s.Mask) {
                mt.UseBackgroundColor = true;
                mt.BackgroundScaleFactor = 1.15;
            }

            while (ml.LeaderCount > 0) {
                try { ml.RemoveLeader(0); } catch { break; }
            }

            if (styleId != ObjectId.Null) ml.TextStyleId = styleId;
            ml.TextHeight = s.Height;
            ml.TextAngleType = TextAngleType.InsertAngle;
            if (layerId != ObjectId.Null) ml.LayerId = layerId;

            if (arrowId != ObjectId.Null) {
                ml.ArrowSymbolId = arrowId;
                ml.ArrowSize = s.ArrowSize;
            } else {
                ml.ArrowSymbolId = ObjectId.Null;
                ml.ArrowSize = 0.0;
            }

            double dx = landingPt.X - targetPt.X;
            double dy = landingPt.Y - targetPt.Y;
            bool isVertical = Math.Abs(dx) < 0.25 * Math.Abs(dy) && Math.Abs(dy) > 1.0;
            bool isRight = (dx >= 0);
            double gap = s.Height * 0.1;

            if (isVertical) {
                mt.Rotation = Math.PI / 2;
                mt.Direction = new Vector3d(0.0, 1.0, 0.0);
                ml.MText = mt;
                ml.EnableDogleg = false;
                ml.EnableLanding = false;

                double textLen = mt.ActualWidth > 0 ? mt.ActualWidth : (s.Height * 6.0);
                double textH = mt.ActualHeight > 0 ? mt.ActualHeight : (s.Height * 2.692);
                double shiftX = textH / 2.0;

                if (dy >= 0) {
                    Point3d endPt = new Point3d(targetPt.X, landingPt.Y + textLen, 0);
                    ml.TextLocation = new Point3d(landingPt.X - shiftX, landingPt.Y, 0);

                    int leaderIndex = ml.AddLeader();
                    int leaderLineIndex = ml.AddLeaderLine(leaderIndex);
                    ml.AddFirstVertex(leaderLineIndex, targetPt);
                    ml.AddLastVertex(leaderLineIndex, endPt);
                } else {
                    Point3d endPt = new Point3d(targetPt.X, landingPt.Y - textLen, 0);
                    ml.TextLocation = new Point3d(endPt.X - shiftX, endPt.Y, 0);

                    int leaderIndex = ml.AddLeader();
                    int leaderLineIndex = ml.AddLeaderLine(leaderIndex);
                    ml.AddFirstVertex(leaderLineIndex, targetPt);
                    ml.AddLastVertex(leaderLineIndex, endPt);
                }
            } else {
                ml.EnableDogleg = true;
                ml.DoglegLength = 0.0;
                ml.EnableLanding = true;
                ml.LandingGap = gap;
                ml.TextAttachmentDirection = TextAttachmentDirection.AttachmentHorizontal;
                ml.TextAttachmentType = TextAttachmentType.AttachmentBottomOfTopLine;
                ml.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.LeftLeader);
                ml.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.RightLeader);
                ml.ExtendLeaderToText = s.UnderlineBetween;

                mt.Rotation = 0.0;
                mt.Direction = new Vector3d(1.0, 0.0, 0.0);
                ml.MText = mt;
                ml.TextAngleType = TextAngleType.InsertAngle;

                int leaderIndex = ml.AddLeader();
                int leaderLineIndex = ml.AddLeaderLine(leaderIndex);
                ml.AddFirstVertex(leaderLineIndex, targetPt);
                ml.AddLastVertex(leaderLineIndex, landingPt);

                Vector3d doglegDir = new Vector3d(isRight ? 1.0 : -1.0, 0.0, 0.0);
                try {
                    ml.SetDogleg(leaderIndex, doglegDir);
                } catch {}

                double yTop = landingPt.Y + s.Height * 1.2;
                double w = mt.ActualWidth > 0 ? mt.ActualWidth : (s.Height * 5.0);
                double xLeft = isRight ? (landingPt.X + gap) : (landingPt.X - gap - w);
                ml.TextLocation = new Point3d(xLeft, yTop, 0);

                try {
                    ml.SetLastVertex(leaderLineIndex, landingPt);
                } catch {}
            }

            if (isNew) {
                ms.AppendEntity(ml);
                tr.AddNewlyCreatedDBObject(ml, true);
            }

            ml.XData = Tag(blockHandle, targetPt, landingPt);
            return ml.ObjectId;
        }

        public static Dictionary<string, ObjectId> ScanExistingLeaders(Transaction tr, BlockTableRecord ms) {
            var dict = new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);
            foreach (ObjectId id in ms) {
                if (id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase)) {
                    var obj = tr.GetObject(id, OpenMode.ForRead);
                    var xdata = obj.GetXDataForApplication(Reg);
                    if (xdata != null) {
                        var arr = xdata.AsArray();
                        if (arr.Length > 1) {
                            string h = Convert.ToString(arr[1].Value);
                            if (!string.IsNullOrEmpty(h)) {
                                try {
                                    Handle handle = new Handle(Convert.ToInt64(h, 16));
                                    ObjectId bId;
                                    if (ms.Database.TryGetObjectId(handle, out bId) && !bId.IsErased) {
                                        BlockReference br = tr.GetObject(bId, OpenMode.ForRead) as BlockReference;
                                        if (br != null) {
                                            MLeader ml = (MLeader)obj;
                                            Point3d v0 = ml.GetFirstVertex(0);
                                            Point3d tipPt = Tip(br, false);
                                            if ((v0 - tipPt).Length < 100.0 || (v0 - br.Position).Length < 100.0) {
                                                dict[h] = id;
                                            }
                                        }
                                    }
                                } catch {
                                    if (!dict.ContainsKey(h)) dict[h] = id;
                                }
                            }
                        }
                        xdata.Dispose();
                    }
                }
            }
            return dict;
        }

        [CommandMethod("T-TOADO")]
        [CommandMethod("TTOADO")]
        public static void Run() {
            Document doc = App.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ToadoSettings s = Load();

            using (var form = new Toado100Form(s)) {
                if (App.ShowModalDialog(form) != DialogResult.OK) return;
                s = form.S;
                Save(s);
                if (form.TriggerRefreshOnly) {
                    RefreshAll(doc, s);
                    return;
                }
            }

            ObjectId dotArrowId = EnsureDotArrow(db);

            var pso = new PromptSelectionOptions();
            pso.MessageForAdding = "\nChọn các Block cần gắn nhãn (hoặc Enter để pick điểm tự do trên bản vẽ): ";
            var filter = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, "INSERT") });
            var selRes = ed.GetSelection(pso, filter);

            if (selRes.Status == PromptStatus.OK) {
                List<ObjectId> blockIds = new List<ObjectId>();
                using (Transaction tr = db.TransactionManager.StartTransaction()) {
                    foreach (ObjectId bId in selRes.Value.GetObjectIds()) {
                        if (bId.ObjectClass.DxfName.Equals("INSERT", StringComparison.OrdinalIgnoreCase)) {
                            blockIds.Add(bId);
                        }
                    }
                    tr.Commit();
                }

                if (blockIds.Count == 0) {
                    ed.WriteMessage("\nKhông tìm thấy Block hợp lệ.");
                    return;
                }

                ed.WriteMessage("\nĐang xử lý " + blockIds.Count + " block...");

                using (DocumentLock dl = doc.LockDocument()) {
                    using (Transaction tr = db.TransactionManager.StartTransaction()) {
                        ObjectId layerId = EnsureLayer(tr, db, s.Layer);
                        EnsureRegApp(tr, db);
                        ObjectId styleId = EnsureTextStyle(tr, db, s.Style);
                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                        var map = ScanExistingLeaders(tr, ms);
                        int created = 0, updated = 0;

                        for (int i = 0; i < blockIds.Count; i++) {
                            ObjectId bId = blockIds[i];
                            BlockReference br = tr.GetObject(bId, OpenMode.ForRead) as BlockReference;
                            if (br == null || br.IsErased) continue;

                            Point3d targetPt = Tip(br, s.Center);
                            string blockName = NameOf(tr, br);
                            string handleStr = br.Handle.ToString();

                            Point3d landingPt;
                            double dist = (s.Distance >= s.Height) ? s.Distance : (s.Height * 3.5);
                            double offset = dist * 0.7071067811865475;
                            if (s.PickEach) {
                                var ppo = new PromptPointOptions(string.Format("\nĐiểm đặt nhãn cho {0} [{1}/{2}] (Enter = mặc định, ESC = dừng): ", blockName, i + 1, blockIds.Count));
                                ppo.UseBasePoint = true;
                                ppo.BasePoint = targetPt;
                                ppo.AllowNone = true;
                                var ppr = ed.GetPoint(ppo);
                                if (ppr.Status == PromptStatus.OK) {
                                    landingPt = ppr.Value;
                                } else if (ppr.Status == PromptStatus.None) {
                                    landingPt = new Point3d(targetPt.X + offset, targetPt.Y + offset, targetPt.Z);
                                } else {
                                    ed.WriteMessage("\nĐã dừng gắn nhãn.");
                                    break;
                                }
                            } else {
                                landingPt = new Point3d(targetPt.X + offset, targetPt.Y + offset, targetPt.Z);
                            }

                            ObjectId existingId = map.ContainsKey(handleStr) ? map[handleStr] : ObjectId.Null;
                            if (existingId != ObjectId.Null) {
                                try {
                                    var oldObj = tr.GetObject(existingId, OpenMode.ForRead) as MLeader;
                                    if (oldObj != null && oldObj.LeaderCount > 0) {
                                        // User requirement: When updating an existing/copied label, arrow tip & landing stay put ("đứng yên")
                                        GetStoredPoints(oldObj, out targetPt, out landingPt);
                                    }
                                } catch {}
                            }
                            CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, blockName, handleStr, existingId, layerId, styleId, dotArrowId);
                            if (existingId == ObjectId.Null) created++; else updated++;
                        }

                        tr.Commit();
                        ed.WriteMessage(string.Format("\n[T-TOADO] Hoàn tất: Tạo mới {0}, Cập nhật {1} nhãn tọa độ.", created, updated));
                    }
                }
            } else {
                using (DocumentLock dl = doc.LockDocument()) {
                    while (true) {
                        var ppo1 = new PromptPointOptions("\nPick điểm cần gắn nhãn tọa độ (Enter hoặc ESC để kết thúc): ");
                        var ppr1 = ed.GetPoint(ppo1);
                        if (ppr1.Status != PromptStatus.OK) break;
                        Point3d targetPt = ppr1.Value;

                        var ppo2 = new PromptPointOptions("\nPick điểm đặt nhãn (hướng tự do 360 độ): ");
                        ppo2.UseBasePoint = true;
                        ppo2.BasePoint = targetPt;
                        ppo2.AllowNone = true;
                        var ppr2 = ed.GetPoint(ppo2);
                        Point3d landingPt;
                        double dist = (s.Distance >= s.Height) ? s.Distance : (s.Height * 3.5);
                        double offset = dist * 0.7071067811865475;
                        if (ppr2.Status == PromptStatus.OK) {
                            landingPt = ppr2.Value;
                        } else if (ppr2.Status == PromptStatus.None) {
                            landingPt = new Point3d(targetPt.X + offset, targetPt.Y + offset, targetPt.Z);
                        } else {
                            break;
                        }

                        using (Transaction tr = db.TransactionManager.StartTransaction()) {
                            ObjectId layerId = EnsureLayer(tr, db, s.Layer);
                            EnsureRegApp(tr, db);
                            ObjectId styleId = EnsureTextStyle(tr, db, s.Style);
                            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                            BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                            CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, "", "", ObjectId.Null, layerId, styleId, dotArrowId);
                            tr.Commit();
                        }
                        ed.WriteMessage(" -> Đã tạo nhãn tọa độ thành công.");
                    }
                }
            }
        }

        [CommandMethod("T-TOADOREFRESH")]
        [CommandMethod("TTOADOREFRESH")]
        public static void RefreshCmd() {
            Document doc = App.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            ToadoSettings s = Load();
            RefreshAll(doc, s);
        }

        public static void RefreshAll(Document doc, ToadoSettings s) {
            Database db = doc.Database;
            Editor ed = doc.Editor;
            ObjectId dotArrowId = EnsureDotArrow(db);

            using (DocumentLock dl = doc.LockDocument()) {
                using (Transaction tr = db.TransactionManager.StartTransaction()) {
                    ObjectId layerId = EnsureLayer(tr, db, s.Layer);
                    EnsureRegApp(tr, db);
                    ObjectId styleId = EnsureTextStyle(tr, db, s.Style);
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    var allBlocks = new List<BlockReference>();
                    foreach (ObjectId entId in ms) {
                        if (entId.ObjectClass.DxfName.Equals("INSERT", StringComparison.OrdinalIgnoreCase)) {
                            var br = tr.GetObject(entId, OpenMode.ForRead) as BlockReference;
                            if (br != null && !br.IsErased) {
                                string bname = NameOf(tr, br);
                                if (bname.IndexOf("khung", StringComparison.OrdinalIgnoreCase) < 0) {
                                    allBlocks.Add(br);
                                }
                            }
                        }
                    }

                    int count = 0;
                    foreach (ObjectId id in ms) {
                        if (!id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase)) continue;
                        MLeader ml = tr.GetObject(id, OpenMode.ForWrite) as MLeader;
                        if (ml == null) continue;

                        ResultBuffer xdata = ml.GetXDataForApplication(Reg);
                        if (xdata == null) continue;

                        TypedValue[] vals = xdata.AsArray();
                        string handleStr = vals.Length > 1 ? Convert.ToString(vals[1].Value) : "";

                        Point3d currentArrow = ml.GetFirstVertex(0);
                        Point3d currentLanding = ml.GetLastVertex(0);

                        BlockReference closestBlock = null;
                        double closestDist = double.MaxValue;
                        foreach (var br in allBlocks) {
                            Point3d tip = Tip(br, s.Center);
                            double d = (currentArrow - tip).Length;
                            if (d < closestDist) {
                                closestDist = d;
                                closestBlock = br;
                            }
                        }

                        string blockName = "";
                        double maxMatchDist = Math.Max(s.Height * 15.0, 5000.0);
                        if (closestBlock != null && closestDist < maxMatchDist) {
                            handleStr = closestBlock.Handle.ToString();
                            blockName = NameOf(tr, closestBlock);
                        }

                        // User explicitly demands: Arrow and Landing STAY PUT ("đứng yên"), only X, Y and block name update!
                        Point3d targetPt;
                        Point3d landingPt;
                        if (!GetStoredPoints(ml, out targetPt, out landingPt)) {
                            targetPt = currentArrow;
                            landingPt = currentLanding;
                        }

                        CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, blockName, handleStr, id, layerId, styleId, dotArrowId);
                        count++;
                    }

                    tr.Commit();
                    ed.WriteMessage(string.Format("\n[T-TOADO] Đã làm mới (Refresh) {0} nhãn tọa độ trên bản vẽ.", count));
                }
            }
        }

        public static ToadoResult Apply(
            Database db,
            IEnumerable<ObjectId> ids,
            ToadoSettings s,
            Func<int, int, Point3d, Point3d?> pick)
        {
            var res = new ToadoResult();
            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                ObjectId layerId = EnsureLayer(tr, db, s.Layer);
                EnsureRegApp(tr, db);
                ObjectId styleId = EnsureTextStyle(tr, db, s.Style);
                ObjectId dotArrowId = EnsureDotArrow(db);
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var map = ScanExistingLeaders(tr, ms);
                var list = new List<ObjectId>(ids);

                for (int i = 0; i < list.Count; i++) {
                    ObjectId id = list[i];
                    BlockReference br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                    if (br == null || br.IsErased) {
                        res.skipped++;
                        continue;
                    }

                    Point3d targetPt = Tip(br, s.Center);
                    string blockName = NameOf(tr, br);
                    string handleStr = br.Handle.ToString();

                    double dist = (s.Distance >= s.Height) ? s.Distance : (s.Height * 3.5);
                    double offset = dist * 0.7071067811865475;
                    Point3d landingPt;
                    if (pick != null) {
                        Point3d? picked = pick(i + 1, list.Count, targetPt);
                        landingPt = picked.HasValue ? picked.Value : new Point3d(targetPt.X + offset, targetPt.Y + offset, targetPt.Z);
                    } else {
                        landingPt = new Point3d(targetPt.X + offset, targetPt.Y + offset, targetPt.Z);
                    }

                    ObjectId existingId = map.ContainsKey(handleStr) ? map[handleStr] : ObjectId.Null;
                    if (existingId != ObjectId.Null) {
                        try {
                            var oldObj = tr.GetObject(existingId, OpenMode.ForRead) as MLeader;
                            if (oldObj != null && oldObj.LeaderCount > 0) {
                                // Preserved: Arrow tip & landing stay put ("đứng yên") on update
                                GetStoredPoints(oldObj, out targetPt, out landingPt);
                            }
                        } catch {}
                    }
                    CreateOrUpdateLeader(db, tr, ms, targetPt, landingPt, s, blockName, handleStr, existingId, layerId, styleId, dotArrowId);
                    if (existingId == ObjectId.Null) res.created++; else res.updated++;
                }

                tr.Commit();
            }
            return res;
        }
    }
}
