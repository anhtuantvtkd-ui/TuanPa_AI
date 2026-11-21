using System;
using System.Reflection;

class Program {
    static void Main() {
        var asm = Assembly.LoadFrom(@"C:\Users\ADMIN\.gemini\antigravity\scratch\BG_R52\MEPS060\TVXD.Meps.dll");
        foreach (var t in asm.GetTypes()) {
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
                foreach (var attr in m.GetCustomAttributes(false)) {
                    if (attr.ToString().Contains("CommandMethod")) {
                        var prop = attr.GetType().GetProperty("GlobalName");
                        string name = prop != null ? prop.GetValue(attr, null) as string : "";
                        if (name != null && name.IndexOf("TOADO", StringComparison.OrdinalIgnoreCase) >= 0) {
                            Console.WriteLine("Found command: " + name + " on " + t.FullName + "." + m.Name);
                        }
                    }
                }
            }
        }
    }
}
