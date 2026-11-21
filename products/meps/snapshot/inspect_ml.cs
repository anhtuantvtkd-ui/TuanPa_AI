using System;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;

class Program {
    static void Main() {
        var t = typeof(MLeader);
        foreach (var m in t.GetMembers(BindingFlags.Public | BindingFlags.Instance)) {
            if (m.Name.IndexOf("Dogleg", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.Name.IndexOf("Landing", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.Name.IndexOf("Leader", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.Name.IndexOf("Vertex", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.Name.IndexOf("Attachment", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.Name.IndexOf("Location", StringComparison.OrdinalIgnoreCase) >= 0) {
                Console.WriteLine(m);
            }
        }
    }
}
