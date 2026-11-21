using System;
using System.IO;
using System.Reflection;

class DecompileToado {
    static void Main() {
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) => {
            string p = Path.Combine(@"C:\Program Files\Autodesk\AutoCAD 2021", new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(p) ? Assembly.LoadFrom(p) : null;
        };

        var asm = Assembly.LoadFrom(@"C:\Users\ADMIN\.gemini\antigravity\scratch\BG_R52\MEPS060\TVXD.Meps.dll");
        var t = asm.GetType("Tvxd.Meps.Toado100");
        
        // Let's dump all string literals and instructions from Toado100 methods
        foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
            Console.WriteLine("\n--- METHOD: " + m.Name + " ---");
            var body = m.GetMethodBody();
            if (body == null) continue;
            byte[] il = body.GetILAsByteArray();
            Console.WriteLine("IL Length: " + il.Length);
        }
    }
}
