using System;
using System.Reflection;
using System.IO;
using System.Linq;

class Disasm {
    static void Main() {
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) => {
            string p = Path.Combine(@"C:\Program Files\Autodesk\AutoCAD 2021", new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(p) ? Assembly.LoadFrom(p) : null;
        };

        var asm = Assembly.LoadFrom(@"C:\Users\ADMIN\.gemini\antigravity\scratch\BG_R52\MEPS060\TVXD.Meps.dll");
        var t = asm.GetType("Tvxd.Meps.Toado100");
        foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
            var body = m.GetMethodBody();
            if (body != null) {
                Console.WriteLine("=== Method: " + m.Name + " (IL size: " + body.GetILAsByteArray().Length + ") ===");
            }
        }
    }
}
