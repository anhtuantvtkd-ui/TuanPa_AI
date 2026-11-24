using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;

namespace Tvxd.Meps
{
    public class ModernThemeEngine
    {
        public class ModernColorTable : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground { get { return Color.FromArgb(30, 41, 59); } } // Slate 800
            public override Color MenuBorder { get { return Color.FromArgb(71, 85, 105); } } // Slate 600
            public override Color MenuItemBorder { get { return Color.Transparent; } }
            public override Color MenuItemSelected { get { return Color.FromArgb(51, 65, 85); } } // Slate 700
            public override Color MenuItemSelectedGradientBegin { get { return Color.FromArgb(51, 65, 85); } }
            public override Color MenuItemSelectedGradientEnd { get { return Color.FromArgb(51, 65, 85); } }
            public override Color MenuStripGradientBegin { get { return Color.FromArgb(30, 41, 59); } }
            public override Color MenuStripGradientEnd { get { return Color.FromArgb(30, 41, 59); } }
            public override Color SeparatorDark { get { return Color.FromArgb(51, 65, 85); } }
            public override Color ImageMarginGradientBegin { get { return Color.FromArgb(30, 41, 59); } }
            public override Color ImageMarginGradientMiddle { get { return Color.FromArgb(30, 41, 59); } }
            public override Color ImageMarginGradientEnd { get { return Color.FromArgb(30, 41, 59); } }
        }

        public class ModernMenuRenderer : ToolStripProfessionalRenderer
        {
            public ModernMenuRenderer() : base(new ModernColorTable())
            {
                this.RoundedEdges = true;
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(3, 1, e.Item.Width - 6, e.Item.Height - 2);

                if (e.Item.Selected)
                {
                    using (var path = RoundedRect(rect, 4))
                    using (var brush = new SolidBrush(Color.FromArgb(51, 65, 85))) // Slate 700
                    using (var pen = new Pen(Color.FromArgb(71, 85, 105), 1f))
                    {
                        g.FillPath(brush, path);
                        g.DrawPath(pen, path);
                    }
                }
                else
                {
                    using (var brush = new SolidBrush(Color.FromArgb(30, 41, 59))) // Slate 800
                    {
                        g.FillRectangle(brush, e.Item.ContentRectangle);
                    }
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
                using (var pen = new Pen(Color.FromArgb(71, 85, 105), 1.2f)) // Slate 600
                {
                    g.DrawRectangle(pen, rect);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                string text = e.Text;
                Rectangle rect = e.Item.ContentRectangle;
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                string cmd = null;
                string title = text;
                string[] separators = new string[] { " \u2014 ", " \u2013 ", " \u00B7 ", " - ", "\u2014", "\u2013" };
                foreach (string sep in separators)
                {
                    int idx = text.IndexOf(sep);
                    if (idx > 0)
                    {
                        cmd = text.Substring(0, idx).Trim();
                        title = text.Substring(idx + sep.Length).Trim();
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(cmd) && cmd.Length <= 14)
                {
                    int badgeWidth = Math.Max(74, (int)g.MeasureString(cmd, new Font("Consolas", 8.2f, FontStyle.Bold)).Width + 14);
                    Rectangle badgeRect = new Rectangle(rect.X + 6, rect.Y + (rect.Height - 22) / 2, badgeWidth, 22);
                    Color badgeColor = GetBadgeColor(cmd);

                    using (var bPath = RoundedRect(badgeRect, 4))
                    {
                        using (var bBrush = new SolidBrush(Color.FromArgb(45, badgeColor.R, badgeColor.G, badgeColor.B)))
                        {
                            g.FillPath(bBrush, bPath);
                        }
                        using (var bPen = new Pen(badgeColor, 1.2f))
                        {
                            g.DrawPath(bPen, bPath);
                        }
                    }

                    using (var fontCmd = new Font("Consolas", 8.2f, FontStyle.Bold))
                    using (var brushCmd = new SolidBrush(badgeColor))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        g.DrawString(cmd, fontCmd, brushCmd, badgeRect, sf);
                    }

                    int textLeft = rect.X + badgeWidth + 14;
                    Rectangle titleRect = new Rectangle(textLeft, rect.Y, rect.Width - textLeft - 4, rect.Height);
                    using (var fontTitle = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (var brushTitle = new SolidBrush(e.Item.Selected ? Color.White : Color.FromArgb(241, 245, 249)))
                    using (var sfTitle = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                    {
                        g.DrawString(title, fontTitle, brushTitle, titleRect, sfTitle);
                    }
                }
                else
                {
                    using (var fontTitle = new Font("Segoe UI", 9f, FontStyle.Regular))
                    using (var brushTitle = new SolidBrush(e.Item.Selected ? Color.White : Color.FromArgb(226, 232, 240)))
                    using (var sfTitle = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                    {
                        Rectangle textRect = new Rectangle(rect.X + 12, rect.Y, rect.Width - 16, rect.Height);
                        g.DrawString(text, fontTitle, brushTitle, textRect, sfTitle);
                    }
                }
            }

            private static Color GetBadgeColor(string cmd)
            {
                if (cmd.StartsWith("T-KLHD")) return Color.FromArgb(16, 185, 129); // Emerald (#10B981)
                if (cmd.StartsWith("CDS") || cmd.StartsWith("T-THONGSO")) return Color.FromArgb(14, 165, 233); // Sky Blue (#0EA5E9)
                if (cmd.StartsWith("T-TH")) return Color.FromArgb(245, 158, 11); // Amber (#F59E0B)
                if (cmd.StartsWith("STT")) return Color.FromArgb(168, 85, 247); // Purple (#A855F7)
                if (cmd.StartsWith("T-CK") || cmd.StartsWith("T-DT") || cmd.StartsWith("T-VEDAY")) return Color.FromArgb(99, 102, 241); // Indigo (#6366F1)
                if (cmd.StartsWith("T-KIEMTRA")) return Color.FromArgb(239, 68, 68); // Rose (#EF4444)
                return Color.FromArgb(45, 212, 191); // Teal (#2DD4BF)
            }

            private static GraphicsPath RoundedRect(RectangleF bounds, float radius)
            {
                GraphicsPath path = new GraphicsPath();
                float d = radius * 2;
                path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
                path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
                path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                return path;
            }
        }

        private static Button GetFieldBtn(Type t, object obj, string name, BindingFlags f)
        {
            FieldInfo fi = t.GetField(name, f);
            return fi != null ? fi.GetValue(obj) as Button : null;
        }

        public static void ApplyTheme(Control cv)
        {
            if (cv == null) return;
            Type cvType = cv.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var routeBar = GetFieldBtn(cvType, cv, "routeBar", flags);
            var layoutBar = GetFieldBtn(cvType, cv, "layoutBar", flags);
            var excelBar = GetFieldBtn(cvType, cv, "excelBar", flags);
            var floorBar = GetFieldBtn(cvType, cv, "floorBar", flags);
            var sdnlBar = GetFieldBtn(cvType, cv, "sdnlBar", flags);
            var roomBar = GetFieldBtn(cvType, cv, "roomBar", flags);
            var drawingIndexBar = GetFieldBtn(cvType, cv, "drawingIndexBar", flags);
            var tagBar = GetFieldBtn(cvType, cv, "tagBar", flags);
            var textStandard106 = GetFieldBtn(cvType, cv, "textStandard106", flags);
            var textWrite110 = GetFieldBtn(cvType, cv, "textWrite110", flags);
            var flexBar = GetFieldBtn(cvType, cv, "flexBar", flags);

            FieldInfo fiHost = cvType.GetField("layoutHost", flags);
            Panel layoutHost = fiHost != null ? fiHost.GetValue(cv) as Panel : null;

            Color cEmerald = Color.FromArgb(16, 185, 129);
            Color cEmeraldDark = Color.FromArgb(5, 150, 105);
            Color cIndigo = Color.FromArgb(99, 102, 241);
            Color cIndigoDark = Color.FromArgb(79, 70, 229);
            Color cAmber = Color.FromArgb(245, 158, 11);
            Color cAmberDark = Color.FromArgb(217, 119, 6);
            Color cCyan = Color.FromArgb(14, 165, 233);
            Color cCyanDark = Color.FromArgb(2, 132, 199);
            Color cSlate = Color.FromArgb(30, 41, 59);
            Color cSlateLine = Color.FromArgb(51, 65, 85);
            Color cSlateHover = Color.FromArgb(51, 65, 85);

            MethodInfo miColors = null;
            if (routeBar != null)
            {
                miColors = routeBar.GetType().GetMethod("Colors", flags);
            }

            Action<Button, Color, Color, Color, Color, string> styleBtn = delegate(Button btn, Color fill, Color border, Color hover, Color fore, string text) {
                if (btn == null) return;
                btn.Text = text;
                if (btn == routeBar || btn == textStandard106)
                {
                    btn.Font = new Font("Segoe UI", 8.2f, FontStyle.Bold);
                }
                else
                {
                    btn.Font = new Font("Segoe UI", 7.2f, FontStyle.Bold);
                }
                btn.Padding = new Padding(0);
                btn.Margin = new Padding(0);
                if (miColors != null)
                {
                    miColors.Invoke(btn, new object[] { fill, border, hover, fore });
                }
            };

            styleBtn(routeBar, cEmerald, cEmeraldDark, Color.FromArgb(52, 211, 153), Color.White, "\uD83D\uDCCA TH\u1ED0NG K\u00CA TUY\u1EBEN \u25BE");
            styleBtn(excelBar, cSlate, cSlateLine, cSlateHover, Color.FromArgb(226, 232, 240), "\uD83D\uDCD1 XU\u1EA4T EXCEL");
            styleBtn(floorBar, cSlate, cSlateLine, cSlateHover, Color.FromArgb(226, 232, 240), "\uD83D\uDCE5 INPUT BKL");
            styleBtn(layoutBar, cIndigo, cIndigoDark, Color.FromArgb(129, 140, 248), Color.White, "\uD83D\uDCD0 D\u00C0N TRANG \u25BE");
            styleBtn(drawingIndexBar, cSlate, cSlateLine, cSlateHover, Color.FromArgb(226, 232, 240), "\uD83D\uDDA8\uFE0F DANH M\u1EE4C & IN");
            styleBtn(tagBar, cAmber, cAmberDark, Color.FromArgb(251, 191, 36), Color.White, "\uD83C\uDFF7\uFE0F TAG NH\u00C3N \u25BE");
            styleBtn(textWrite110, cSlate, cSlateLine, cSlateHover, Color.FromArgb(226, 232, 240), "\uD83D\uDCDD GHI CH\u00DA MTEXT");
            styleBtn(sdnlBar, cCyan, cCyanDark, Color.FromArgb(56, 189, 248), Color.White, "\u26A1 SDNL T\u1EE6 \u25BE");
            styleBtn(roomBar, cSlate, cSlateLine, cSlateHover, Color.FromArgb(226, 232, 240), "\uD83C\uDFE0 B\u1ED0 TR\u00CD PH\u00D2NG");
            styleBtn(textStandard106, cSlate, cSlateLine, cSlateHover, Color.FromArgb(203, 213, 225), "\uD83D\uDD24 CHU\u1EA8N H\u00D3A TI\u00CAU CHU\u1EA8N");
            styleBtn(flexBar, cSlate, cSlateLine, cSlateHover, Color.FromArgb(203, 213, 225), "\uD83D\uDCA8 V\u1EBD TUY\u1EBEN \u1ED0NG GI\u00D3 M\u1EC0M");

            if (layoutHost != null)
            {
                int w = layoutHost.Width > 0 ? layoutHost.Width : 208;
                int pad = 4;
                int gap = 4;
                int fullW = w - 2 * pad;
                int colW = (fullW - gap) / 2;
                int col0 = pad;
                int col1 = pad + colW + gap;
                int btnH = 32;
                int rowGap = 4;
                int y = 4;

                if (routeBar != null) { routeBar.Bounds = new Rectangle(col0, y, fullW, 36); y += 36 + rowGap; }
                if (excelBar != null) { excelBar.Bounds = new Rectangle(col0, y, colW, btnH); }
                if (floorBar != null) { floorBar.Bounds = new Rectangle(col1, y, colW, btnH); }
                y += btnH + rowGap;

                if (layoutBar != null) { layoutBar.Bounds = new Rectangle(col0, y, colW, btnH); }
                if (drawingIndexBar != null) { drawingIndexBar.Bounds = new Rectangle(col1, y, colW, btnH); }
                y += btnH + rowGap;

                if (tagBar != null) { tagBar.Bounds = new Rectangle(col0, y, colW, btnH); }
                if (textWrite110 != null) { textWrite110.Bounds = new Rectangle(col1, y, colW, btnH); }
                y += btnH + rowGap;

                if (sdnlBar != null) { sdnlBar.Bounds = new Rectangle(col0, y, colW, btnH); }
                if (roomBar != null) { roomBar.Bounds = new Rectangle(col1, y, colW, btnH); }
                y += btnH + rowGap;

                if (textStandard106 != null) { textStandard106.Bounds = new Rectangle(col0, y, fullW, btnH); y += btnH + rowGap; }

                layoutHost.Height = y + 8;
            }

            var renderer = new ModernMenuRenderer();
            string[] menuNames = new string[] { "routeMenu", "layoutMenu", "sdnlMenu", "tagMenu", "drawingIndexMenu" };
            foreach (string mName in menuNames)
            {
                FieldInfo fiMenu = cvType.GetField(mName, flags);
                ContextMenuStrip menu = fiMenu != null ? fiMenu.GetValue(cv) as ContextMenuStrip : null;
                if (menu != null)
                {
                    menu.Renderer = renderer;
                    menu.ShowImageMargin = false;
                    menu.ShowCheckMargin = false;
                    menu.Font = new Font("Segoe UI", 9f);

                    // Configure existing items
                    foreach (ToolStripItem item in menu.Items)
                    {
                        item.Height = 32;
                        item.AutoSize = false;
                        item.Size = new Size(320, 32);
                    }

                    // Attach Opening handler to ensure all items are styled when opened
                    menu.Opening -= Menu_Opening;
                    menu.Opening += Menu_Opening;
                }
            }
        }

        private static void Menu_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip menu = sender as ContextMenuStrip;
            if (menu != null)
            {
                foreach (ToolStripItem item in menu.Items)
                {
                    item.Height = 32;
                    item.AutoSize = false;
                    item.Size = new Size(320, 32);
                }
            }
        }
    }
}
