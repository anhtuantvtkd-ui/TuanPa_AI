using System;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;

namespace Tvxd.Meps
{
    public static class HtktWireFix027
    {
        public static string ResolveHtktLayer(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;
            if (code.StartsWith("HTKT-BE-", StringComparison.OrdinalIgnoreCase) ||
                code.StartsWith("HTKT-GANIVO-", StringComparison.OrdinalIgnoreCase))
            {
                return "E-HT-TTLL";
            }
            if (code.StartsWith("HTKT-TU-", StringComparison.OrdinalIgnoreCase))
            {
                return "E-HT-TU";
            }
            if (code.StartsWith("Cot den kep ", StringComparison.OrdinalIgnoreCase))
            {
                return "E-HT-DEN";
            }
            return null;
        }

        public static bool IsCustomLinetype(string ltName)
        {
            return !string.IsNullOrEmpty(ltName) &&
                   ltName.StartsWith("DIEN_SS_", StringComparison.OrdinalIgnoreCase);
        }

        public static ObjectId EnsureCustomLinetype(Database db, Transaction tr, string ltName)
        {
            if (!IsCustomLinetype(ltName)) return ObjectId.Null;

            var table = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForRead);
            if (table.Has(ltName))
            {
                var existing = (LinetypeTableRecord)tr.GetObject(table[ltName], OpenMode.ForRead);
                if (!existing.IsErased) return table[ltName];
            }

            string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string linPath = Path.Combine(dllDir, "LIBRARY_0_6", "linetypes076", "DayDien_" + ltName + ".lin");
            if (!File.Exists(linPath))
            {
                string localDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TVXD", "MEPS060", "LIBRARY_0_6", "linetypes076");
                string alt = Path.Combine(localDir, "DayDien_" + ltName + ".lin");
                if (File.Exists(alt)) linPath = alt;
            }
            if (!File.Exists(linPath)) return ObjectId.Null;

            // Ensure TextStyle "Tuan_arial" exists only after .lin file is verified
            var st = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);
            if (!st.Has("Tuan_arial"))
            {
                st.UpgradeOpen();
                var rec = new TextStyleTableRecord
                {
                    Name = "Tuan_arial",
                    FileName = "arial.ttf",
                    XScale = 1.0,
                    ObliquingAngle = 0.0
                };
                st.Add(rec);
                tr.AddNewlyCreatedDBObject(rec, true);
            }

            db.LoadLineTypeFile(ltName, linPath);
            if (table.Has(ltName)) return table[ltName];
            return ObjectId.Null;
        }
    }
}
