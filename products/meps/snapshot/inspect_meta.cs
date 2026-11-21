using System;
using System.Reflection;

class Program {
    static void Main() {
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (s, e) => {
            return Assembly.ReflectionOnlyLoad(e.Name);
        };

        Assembly asm = Assembly.ReflectionOnlyLoadFrom(@"C:\Users\ADMIN\.gemini\antigravity\scratch\acdbmgd.dll");
        Type t = asm.GetType("Autodesk.AutoCAD.DatabaseServices.MLeader");
        foreach (var m in t.GetMembers(BindingFlags.Public | BindingFlags.Instance)) {
            string name = m.Name;
            if (name.IndexOf("Dogleg", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Landing", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Leader", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Vertex", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Attachment", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Location", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Align", StringComparison.OrdinalIgnoreCase) >= 0) {
                Console.WriteLine(m.MemberType + ": " + m.Name + " -> " + m);
            }
        }
    }
}
