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

        var asm = Assembly.LoadFrom(@"C:\Users\ADMIN\.gemini\antigravity\scratch\TVXD.Meps.Merged.dll");
        Console.WriteLine("Assembly: " + asm.FullName);
        
        var toado = asm.GetType("Tvxd.Meps.Toado100");
        Console.WriteLine("Toado100: " + (toado != null ? "Found" : "NULL"));

        var prep = asm.GetType("Tvxd.Meps.TvxdMEPS012PREP");
        Console.WriteLine("TvxdMEPS012PREP: " + (prep != null ? "Found" : "NULL"));

        var form = asm.GetType("Tvxd.Meps.Toado100Form");
        Console.WriteLine("Toado100Form: " + (form != null ? "Found" : "NULL"));

        foreach (var m in toado.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
            Console.WriteLine("Toado method: " + m.Name);
        }
    }
}
