using System;
using Autodesk.AutoCAD.DatabaseServices;

class Program {
    static void Main() {
        foreach (var val in Enum.GetValues(typeof(TextAttachmentType))) {
            Console.WriteLine("TextAttachmentType: " + val + " = " + (int)val);
        }
        foreach (var val in Enum.GetValues(typeof(AttachmentPoint))) {
            Console.WriteLine("AttachmentPoint: " + val + " = " + (int)val);
        }
        foreach (var val in Enum.GetValues(typeof(TextAlignmentType))) {
            Console.WriteLine("TextAlignmentType: " + val + " = " + (int)val);
        }
    }
}
