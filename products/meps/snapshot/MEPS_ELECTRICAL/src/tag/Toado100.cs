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
    public sealed class ToadoConfig {
        public string Style = "Tuan_arial";
        public string Layer = "TOA_DO";
        public double Height = 2.5;
        public double ArrowSize = 1.5;
        public double Distance = 15.0;
        public int Decimals = 3;
        public bool Vn2000 = true;
        public bool UnitIsMm = false; // true = chia 1000 ra mét
        public bool Mask = false;      // Không che nét mặc định để vạch phân vùng nét căng
        public bool Frame = false;
        public bool CircleArrow = true;
        public bool UnderlineBetween = true; // Landing line xen giữa 2 dòng
        public bool PickDirection = true;    // Chọn hướng tự do bằng chuột 360 độ
        public bool AddName = false;

        public static string IniPath {
            get {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TVXD", "MEPS060");
                try { Directory.CreateDirectory(dir); } catch {}
                return Path.Combine(dir, "toado.ini");
            }
        }

        public void Save() {
            try {
                using (var w = new StreamWriter(IniPath, false, System.Text.Encoding.UTF8)) {
                    w.WriteLine("Style=" + Style);
                    w.WriteLine("Layer=" + Layer);
                    w.WriteLine("Height=" + Height.ToString(CultureInfo.InvariantCulture));
                    w.WriteLine("ArrowSize=" + ArrowSize.ToString(CultureInfo.InvariantCulture));
                    w.WriteLine("Distance=" + Distance.ToString(CultureInfo.InvariantCulture));
                    w.WriteLine("Decimals=" + Decimals);
                    w.WriteLine("Vn2000=" + Vn2000);
                    w.WriteLine("UnitIsMm=" + UnitIsMm);
                    w.WriteLine("Mask=" + Mask);
                    w.WriteLine("Frame=" + Frame);
                    w.WriteLine("CircleArrow=" + CircleArrow);
                    w.WriteLine("UnderlineBetween=" + UnderlineBetween);
                    w.WriteLine("PickDirection=" + PickDirection);
                    w.WriteLine("AddName=" + AddName);
                }
            } catch {}
        }

        public static ToadoConfig Load() {
            var c = new ToadoConfig();
            try {
                if (File.Exists(IniPath)) {
                    foreach (var line in File.ReadAllLines(IniPath, System.Text.Encoding.UTF8)) {
                        int eq = line.IndexOf('=');
                        if (eq <= 0) continue;
                        string k = line.Substring(0, eq).Trim();
                        string v = line.Substring(eq + 1).Trim();
                        if (k.Equals("Style", StringComparison.OrdinalIgnoreCase)) c.Style = v;
                        else if (k.Equals("Layer", StringComparison.OrdinalIgnoreCase)) c.Layer = v;
                        else if (k.Equals("Height", StringComparison.OrdinalIgnoreCase)) double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out c.Height);
                        else if (k.Equals("ArrowSize", StringComparison.OrdinalIgnoreCase)) double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out c.ArrowSize);
                        else if (k.Equals("Distance", StringComparison.OrdinalIgnoreCase)) double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out c.Distance);
                        else if (k.Equals("Decimals", StringComparison.OrdinalIgnoreCase)) int.TryParse(v, out c.Decimals);
                        else if (k.Equals("Vn2000", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out c.Vn2000);
                        else if (k.Equals("UnitIsMm", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out c.UnitIsMm);
                        else if (k.Equals("Mask", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out c.Mask);
                        else if (k.Equals("Frame", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out c.Frame);
                        else if (k.Equals("CircleArrow", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out c.CircleArrow);
                        else if (k.Equals("UnderlineBetween", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out c.UnderlineBetween);
                        else if (k.Equals("PickDirection", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out c.PickDirection);
                        else if (k.Equals("AddName", StringComparison.OrdinalIgnoreCase)) bool.TryParse(v, out c.AddName);
                    }
                }
            } catch {}
            return c;
        }
    }

    public sealed class Toado100Form : Form {
        public ToadoConfig Config;
        public bool TriggerRefreshOnly = false;

        TextBox txtStyle, txtHeight, txtArrowSize, txtDecimals;
        RadioButton rbVn2000, rbCad, rbUnitM, rbUnitMm, rbPickFree, rbBatch;
        CheckBox chkMask, chkCircle, chkUnderline, chkAddName;

        public Toado100Form(ToadoConfig cfg) {
            Config = cfg;
            InitializeComponent();
        }

        void InitializeComponent() {
            this.Text = "T-TOADO — Nhãn Tọa Độ VN-2000 (Bản Nâng Cấp)";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(520, 560);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular);

            int y = 15;
            // Group 1: Trình bày & Đầu tròn
            var grpLook = new GroupBox { Text = "1. Định dạng chữ & Đầu mút hình tròn", Location = new System.Drawing.Point(15, y), Size = new Size(475, 145) };
            
            var lblH = new Label { Text = "Chiều cao chữ:", Location = new System.Drawing.Point(15, 25), AutoSize = true };
            txtHeight = new TextBox { Text = Config.Height.ToString("0.##", CultureInfo.InvariantCulture), Location = new System.Drawing.Point(120, 22), Width = 70 };
            
            var lblArr = new Label { Text = "Kích thước đầu tròn:", Location = new System.Drawing.Point(220, 25), AutoSize = true };
            txtArrowSize = new TextBox { Text = Config.ArrowSize.ToString("0.##", CultureInfo.InvariantCulture), Location = new System.Drawing.Point(365, 22), Width = 70 };

            var lblDec = new Label { Text = "Chữ số sau phẩy:", Location = new System.Drawing.Point(15, 58), AutoSize = true };
            txtDecimals = new TextBox { Text = Config.Decimals.ToString(), Location = new System.Drawing.Point(120, 55), Width = 70 };

            var lblStyle = new Label { Text = "Kiểu chữ (Style):", Location = new System.Drawing.Point(220, 58), AutoSize = true };
            txtStyle = new TextBox { Text = Config.Style, Location = new System.Drawing.Point(365, 55), Width = 95 };

            chkCircle = new CheckBox { Text = "● Đầu mũi tên là HÌNH TRÒN (Dot)", Checked = Config.CircleArrow, Location = new System.Drawing.Point(15, 90), AutoSize = true, ForeColor = System.Drawing.Color.DarkGreen, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            chkUnderline = new CheckBox { Text = "━ Đường gióng XEN GIỮA 2 dòng X/Y", Checked = Config.UnderlineBetween, Location = new System.Drawing.Point(15, 115), AutoSize = true, ForeColor = System.Drawing.Color.DarkBlue, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };

            grpLook.Controls.AddRange(new Control[] { lblH, txtHeight, lblArr, txtArrowSize, lblDec, txtDecimals, lblStyle, txtStyle, chkCircle, chkUnderline });
            this.Controls.Add(grpLook);

            // Group 2: Hệ tọa độ & Đơn vị
            y += 155;
            var grpCoord = new GroupBox { Text = "2. Hệ tọa độ VN-2000 & Đơn vị", Location = new System.Drawing.Point(15, y), Size = new Size(475, 110) };
            
            rbVn2000 = new RadioButton { Text = "VN-2000 (Dòng trên X = Trục Y CAD, Dòng dưới Y = Trục X CAD)", Checked = Config.Vn2000, Location = new System.Drawing.Point(15, 23), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = System.Drawing.Color.Firebrick };
            rbCad = new RadioButton { Text = "Hệ CAD thuần (X = Trục ngang X, Y = Trục đứng Y)", Checked = !Config.Vn2000, Location = new System.Drawing.Point(15, 48), AutoSize = true };

            var lblUnit = new Label { Text = "Đơn vị CAD:", Location = new System.Drawing.Point(15, 78), AutoSize = true };
            rbUnitM = new RadioButton { Text = "Mét (m)", Checked = !Config.UnitIsMm, Location = new System.Drawing.Point(120, 76), AutoSize = true };
            rbUnitMm = new RadioButton { Text = "Milimét (mm - tự chia 1000 ra mét)", Checked = Config.UnitIsMm, Location = new System.Drawing.Point(220, 76), AutoSize = true };

            grpCoord.Controls.AddRange(new Control[] { rbVn2000, rbCad, lblUnit, rbUnitM, rbUnitMm });
            this.Controls.Add(grpCoord);

            // Group 3: Chọn hướng tự do 360 độ
            y += 120;
            var grpPlace = new GroupBox { Text = "3. Hướng đặt nhãn & Tùy chọn", Location = new System.Drawing.Point(15, y), Size = new Size(475, 105) };

            rbPickFree = new RadioButton { Text = "Click chuột chọn hướng tự do 360° (Khuyên dùng)", Checked = Config.PickDirection, Location = new System.Drawing.Point(15, 22), AutoSize = true, ForeColor = System.Drawing.Color.DarkGreen, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            rbBatch = new RadioButton { Text = "Tự động lệch góc cố định 45°", Checked = !Config.PickDirection, Location = new System.Drawing.Point(15, 47), AutoSize = true };

            chkMask = new CheckBox { Text = "Nền che nét (Mask)", Checked = Config.Mask, Location = new System.Drawing.Point(15, 75), AutoSize = true };
            chkAddName = new CheckBox { Text = "Ghi kèm Tên/TAG Block", Checked = Config.AddName, Location = new System.Drawing.Point(220, 75), AutoSize = true };

            grpPlace.Controls.AddRange(new Control[] { rbPickFree, rbBatch, chkMask, chkAddName });
            this.Controls.Add(grpPlace);

            // Buttons
            y += 115;
            var btnOk = new Button { Text = "▶ ĐẶT TỌA ĐỘ", Location = new System.Drawing.Point(25, y), Size = new Size(135, 40), BackColor = System.Drawing.Color.LightSteelBlue, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            btnOk.Click += (s, e) => {
                Collect();
                Config.Save();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            var btnRefresh = new Button { Text = "🔄 CẬP NHẬT TẤT CẢ", Location = new System.Drawing.Point(175, y), Size = new Size(185, 40), BackColor = System.Drawing.Color.LemonChiffon, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            btnRefresh.Click += (s, e) => {
                Collect();
                Config.Save();
                TriggerRefreshOnly = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            var btnCancel = new Button { Text = "Đóng", Location = new System.Drawing.Point(375, y), Size = new Size(100, 40) };
            btnCancel.Click += (s, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.AddRange(new Control[] { btnOk, btnRefresh, btnCancel });
        }

        void Collect() {
            double.TryParse(txtHeight.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out Config.Height);
            if (Config.Height <= 0) Config.Height = 2.5;

            double.TryParse(txtArrowSize.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out Config.ArrowSize);
            if (Config.ArrowSize <= 0) Config.ArrowSize = 1.5;

            int.TryParse(txtDecimals.Text, out Config.Decimals);
            if (Config.Decimals < 0 || Config.Decimals > 6) Config.Decimals = 3;

            Config.Style = string.IsNullOrWhiteSpace(txtStyle.Text) ? "Tuan_arial" : txtStyle.Text.Trim();
            Config.Vn2000 = rbVn2000.Checked;
            Config.UnitIsMm = rbUnitMm.Checked;
            Config.CircleArrow = chkCircle.Checked;
            Config.UnderlineBetween = chkUnderline.Checked;
            Config.PickDirection = rbPickFree.Checked;
            Config.Mask = chkMask.Checked;
            Config.AddName = chkAddName.Checked;
        }
    }

    public static class Toado100 {
        public const string Reg = "TVXD_TOADO";

        public static void EnsureLayer(Database db, Transaction tr, string layerName) {
            LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (!lt.Has(layerName)) {
                lt.UpgradeOpen();
                var ltr = new LayerTableRecord {
                    Name = layerName,
                    Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 2)
                };
                lt.Add(ltr);
                tr.AddNewlyCreatedDBObject(ltr, true);
            }
        }

        public static void EnsureRegApp(Database db, Transaction tr) {
            RegAppTable rat = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
            if (!rat.Has(Reg)) {
                rat.UpgradeOpen();
                var r = new RegAppTableRecord { Name = Reg };
                rat.Add(r);
                tr.AddNewlyCreatedDBObject(r, true);
            }
        }

        public static ObjectId EnsureTextStyle(Database db, Transaction tr, string styleName) {
            TextStyleTable tt = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);
            if (tt.Has(styleName)) return tt[styleName];
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
        }

        public static ObjectId EnsureDotArrowBlock(Database db, Transaction tr) {
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            string name = "_DOT";
            if (bt.Has(name)) return bt[name];
            if (bt.Has("_Origin2")) return bt["_Origin2"];
            if (bt.Has("_DotSmall")) return bt["_DotSmall"];

            bt.UpgradeOpen();
            BlockTableRecord btr = new BlockTableRecord();
            btr.Name = name;
            btr.Origin = Point3d.Origin;

            Polyline pl = new Polyline();
            pl.AddVertexAt(0, new Point2d(-0.5, 0), 1.0, 0, 0);
            pl.AddVertexAt(1, new Point2d(0.5, 0), 1.0, 0, 0);
            pl.Closed = true;
            pl.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 0);

            btr.AppendEntity(pl);

            try {
                Hatch h = new Hatch();
                h.SetDatabaseDefaults();
                h.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 0);
                h.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                btr.AppendEntity(h);
                ObjectIdCollection ids = new ObjectIdCollection();
                ids.Add(pl.ObjectId);
                h.AppendLoop(HatchLoopTypes.Default, ids);
                h.EvaluateHatch(true);
            } catch {}

            ObjectId bid = bt.Add(btr);
            tr.AddNewlyCreatedDBObject(btr, true);
            tr.AddNewlyCreatedDBObject(pl, true);
            return bid;
        }

        public static string FormatCoord(Point3d pt, ToadoConfig cfg, string blockName) {
            double scale = cfg.UnitIsMm ? 0.001 : 1.0;
            double xVal = (cfg.Vn2000 ? pt.Y : pt.X) * scale;
            double yVal = (cfg.Vn2000 ? pt.X : pt.Y) * scale;

            string fmt = "0." + new string('0', cfg.Decimals);
            string lineX = "X=" + xVal.ToString(fmt, CultureInfo.InvariantCulture);
            string lineY = "Y=" + yVal.ToString(fmt, CultureInfo.InvariantCulture);

            string content = lineX + "\\P" + lineY;
            if (cfg.AddName && !string.IsNullOrEmpty(blockName)) {
                content = blockName + "\\P" + content;
            }
            return content;
        }

        [CommandMethod("T-TOADO")]
        [CommandMethod("TTOADO")]
        public static void Run() {
            Document doc = App.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ToadoConfig cfg = ToadoConfig.Load();
            using (var form = new Toado100Form(cfg)) {
                if (App.ShowModalDialog(form) != DialogResult.OK) return;
                cfg = form.Config;

                if (form.TriggerRefreshOnly) {
                    UpdateAll(doc, cfg);
                    return;
                }
            }

            var pso = new PromptSelectionOptions();
            pso.MessageForAdding = "\nChọn các Block cần gắn nhãn (hoặc Enter để chọn điểm trực tiếp trên bản vẽ): ";
            var filter = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, "INSERT") });
            var selRes = ed.GetSelection(pso, filter);

            List<Point3d> points = new List<Point3d>();
            List<string> names = new List<string>();
            List<ObjectId> blockIds = new List<ObjectId>();

            if (selRes.Status == PromptStatus.OK) {
                using (var tr = db.TransactionManager.StartTransaction()) {
                    foreach (ObjectId bId in selRes.Value.GetObjectIds()) {
                        BlockReference br = tr.GetObject(bId, OpenMode.ForRead) as BlockReference;
                        if (br != null) {
                            points.Add(br.Position);
                            names.Add(br.Name);
                            blockIds.Add(bId);
                        }
                    }
                    tr.Commit();
                }
            } else {
                while (true) {
                    var ptOpt = new PromptPointOptions("\nPick điểm cần gắn nhãn tọa độ (Enter/Esc để kết thúc): ");
                    var ptRes = ed.GetPoint(ptOpt);
                    if (ptRes.Status != PromptStatus.OK) break;
                    points.Add(ptRes.Value);
                    names.Add("");
                    blockIds.Add(ObjectId.Null);

                    if (cfg.PickDirection) {
                        PlaceSingleLabel(doc, db, ed, cfg, ptRes.Value, "", ObjectId.Null);
                        points.Clear();
                        names.Clear();
                        blockIds.Clear();
                    }
                }
                if (cfg.PickDirection) return;
            }

            if (points.Count == 0) {
                ed.WriteMessage("\nKhông có đối tượng nào được chọn.");
                return;
            }

            ed.WriteMessage("\nĐang tạo nhãn tọa độ cho " + points.Count + " vị trí...");

            for (int i = 0; i < points.Count; i++) {
                PlaceSingleLabel(doc, db, ed, cfg, points[i], names[i], blockIds[i]);
            }
        }

        public static void PlaceSingleLabel(Document doc, Database db, Editor ed, ToadoConfig cfg, Point3d targetPt, string blockName, ObjectId blockId) {
            Point3d landingPt;
            if (cfg.PickDirection) {
                var ppo = new PromptPointOptions("\nClick chọn hướng/vị trí đặt nhãn (Esc = hướng mặc định 45°): ");
                ppo.UseBasePoint = true;
                ppo.BasePoint = targetPt;
                var ppr = ed.GetPoint(ppo);
                if (ppr.Status == PromptStatus.OK) {
                    landingPt = ppr.Value;
                } else {
                    landingPt = new Point3d(targetPt.X + cfg.Distance, targetPt.Y + cfg.Distance, 0);
                }
            } else {
                landingPt = new Point3d(targetPt.X + cfg.Distance, targetPt.Y + cfg.Distance, 0);
            }

            using (DocumentLock dl = doc.LockDocument()) {
                using (Transaction tr = db.TransactionManager.StartTransaction()) {
                    EnsureLayer(db, tr, cfg.Layer);
                    EnsureRegApp(db, tr);
                    ObjectId styleId = EnsureTextStyle(db, tr, cfg.Style);
                    ObjectId arrowId = cfg.CircleArrow ? EnsureDotArrowBlock(db, tr) : ObjectId.Null;

                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    MLeader ml = new MLeader();
                    ml.SetDatabaseDefaults();
                    ml.Layer = cfg.Layer;
                    ml.ContentType = ContentType.MTextContent;

                    MText mt = new MText();
                    mt.SetDatabaseDefaults();
                    mt.TextHeight = cfg.Height;
                    if (styleId != ObjectId.Null) mt.TextStyleId = styleId;
                    mt.Contents = FormatCoord(targetPt, cfg, blockName);
                    
                    if (cfg.Mask) {
                        mt.BackgroundFill = true;
                        mt.UseBackgroundColor = true;
                        mt.BackgroundScaleFactor = 1.15;
                    }
                    ml.MText = mt;

                    int ldrIdx = ml.AddLeader();
                    int lineIdx = ml.AddLeaderLine(ldrIdx);
                    ml.AddFirstVertex(lineIdx, targetPt);
                    ml.AddLastVertex(lineIdx, landingPt);

                    ml.EnableDogleg = true;
                    ml.DoglegLength = cfg.Height * 1.5;
                    ml.EnableLanding = true;
                    ml.LandingGap = cfg.Height * 0.2;

                    // Đường gióng xen vào giữa 2 dòng X và Y
                    if (cfg.UnderlineBetween) {
                        ml.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.LeftLeader);
                        ml.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.RightLeader);
                        ml.ExtendLeaderToText = true;
                    }

                    // Đầu mút hình tròn Dot
                    if (cfg.CircleArrow && arrowId != ObjectId.Null) {
                        ml.ArrowSymbolId = arrowId;
                    }
                    ml.ArrowSize = cfg.ArrowSize;

                    // XDATA
                    string handleStr = blockId.IsValid ? blockId.Handle.ToString() : "";
                    using (var rb = new ResultBuffer(
                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, Reg),
                        new TypedValue((int)DxfCode.ExtendedDataAsciiString, handleStr),
                        new TypedValue((int)DxfCode.ExtendedDataReal, targetPt.X),
                        new TypedValue((int)DxfCode.ExtendedDataReal, targetPt.Y),
                        new TypedValue((int)DxfCode.ExtendedDataReal, targetPt.Z)
                    )) {
                        ml.XData = rb;
                    }

                    ms.AppendEntity(ml);
                    tr.AddNewlyCreatedDBObject(ml, true);
                    tr.Commit();
                }
            }
        }

        [CommandMethod("T-TOADOREFRESH")]
        [CommandMethod("TTOADOREFRESH")]
        public static void RefreshCmd() {
            Document doc = App.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            ToadoConfig cfg = ToadoConfig.Load();
            UpdateAll(doc, cfg);
        }

        public static void UpdateAll(Document doc, ToadoConfig cfg) {
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (DocumentLock dl = doc.LockDocument()) {
                using (Transaction tr = db.TransactionManager.StartTransaction()) {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    int updated = 0;
                    foreach (ObjectId id in ms) {
                        if (!id.ObjectClass.DxfName.Equals("MULTILEADER", StringComparison.OrdinalIgnoreCase)) continue;
                        MLeader ml = tr.GetObject(id, OpenMode.ForRead) as MLeader;
                        if (ml == null) continue;

                        ResultBuffer xdata = ml.GetXDataForApplication(Reg);
                        if (xdata == null) continue;

                        TypedValue[] vals = xdata.AsArray();
                        string handleStr = vals.Length > 1 ? vals[1].Value.ToString() : "";

                        Point3d currentPt = Point3d.Origin;
                        string blockName = "";
                        bool hasBlock = false;

                        if (!string.IsNullOrEmpty(handleStr)) {
                            try {
                                Handle h = new Handle(Convert.ToInt64(handleStr, 16));
                                ObjectId blockId;
                                if (db.TryGetObjectId(h, out blockId) && !blockId.IsErased) {
                                    BlockReference br = tr.GetObject(blockId, OpenMode.ForRead) as BlockReference;
                                    if (br != null) {
                                        currentPt = br.Position;
                                        blockName = br.Name;
                                        hasBlock = true;
                                    }
                                }
                            } catch {}
                        }

                        if (!hasBlock) {
                            currentPt = ml.GetFirstVertex(0);
                        }

                        ml.UpgradeOpen();

                        if (hasBlock && !ml.GetFirstVertex(0).IsEqualTo(currentPt)) {
                            ml.SetFirstVertex(0, currentPt);
                        }

                        MText mt = ml.MText;
                        if (mt != null) {
                            mt.Contents = FormatCoord(currentPt, cfg, blockName);
                            if (cfg.Mask) {
                                mt.BackgroundFill = true;
                                mt.UseBackgroundColor = true;
                                mt.BackgroundScaleFactor = 1.15;
                            }
                            ml.MText = mt;
                        }

                        if (cfg.UnderlineBetween) {
                            ml.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.LeftLeader);
                            ml.SetTextAttachmentType(TextAttachmentType.AttachmentBottomOfTopLine, LeaderDirectionType.RightLeader);
                            ml.ExtendLeaderToText = true;
                        }

                        if (cfg.CircleArrow) {
                            ObjectId arrowId = EnsureDotArrowBlock(db, tr);
                            if (arrowId != ObjectId.Null) ml.ArrowSymbolId = arrowId;
                        }
                        ml.ArrowSize = cfg.ArrowSize;

                        updated++;
                    }

                    tr.Commit();
                    ed.WriteMessage("\nĐã làm mới (Refresh) thành công " + updated + " nhãn tọa độ!");
                }
            }
        }
    }
}
