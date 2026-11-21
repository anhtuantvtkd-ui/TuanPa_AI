using System;
using System.IO;
using System.Reflection;

class Program {
    static void Main() {
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) => {
            string name = new AssemblyName(e.Name).Name + ".dll";
            string p1 = Path.Combine(@"C:\Program Files\Autodesk\AutoCAD 2021", name);
            if (File.Exists(p1)) return Assembly.LoadFrom(p1);
            string p2 = Path.Combine(@"C:\Users\ADMIN\.gemini\antigravity\scratch\BG_R52\MEPS060", name);
            if (File.Exists(p2)) return Assembly.LoadFrom(p2);
            return null;
        };

        var asm = Assembly.LoadFrom(@"C:\Users\ADMIN\.gemini\antigravity\scratch\BG_R52\MEPS060\TVXD.Meps.dll");
        foreach (var t in asm.GetTypes()) {
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
                try {
                    foreach (var attr in m.GetCustomAttributes(false)) {
                        if (attr.ToString().Contains("CommandMethod")) {
                            var prop = attr.GetType().GetProperty("GlobalName");
                            string name = prop != null ? prop.GetValue(attr, null) as string : "";
                            if (name != null && (name.IndexOf("TOADO", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("MEP", StringComparison.OrdinalIgnoreCase) >= 0)) {
                                Console.WriteLine("Found command: " + name + " on " + t.FullName + "." + m.Name);
                            }
                        }
                    }
                } catch {}
            }
        }
    }
}
