using System;
using System.IO;
using System.Text;

class MergeToado {
    static void Main() {
        string toadoPartPath = @"C:\Users\ADMIN\.gemini\antigravity\scratch\ToadoPart.il";
        string mepsOrigPath = @"C:\Users\ADMIN\.gemini\antigravity\scratch\meps_orig.il";
        string mepsFinalPath = @"C:\Users\ADMIN\.gemini\antigravity\scratch\meps_final.il";

        Console.WriteLine("Reading ToadoPart.il...");
        var toadoLines = File.ReadAllLines(toadoPartPath);
        int startToado = -1;
        for (int i = 0; i < toadoLines.Length; i++) {
            if (toadoLines[i].StartsWith(".class public auto ansi sealed beforefieldinit Tvxd.Meps.ToadoSettings")) {
                startToado = i;
                break;
            }
        }
        if (startToado < 0) {
            Console.WriteLine("Error: ToadoSettings not found in ToadoPart.il");
            return;
        }

        var newToadoSection = new StringBuilder();
        for (int i = startToado; i < toadoLines.Length; i++) {
            newToadoSection.AppendLine(toadoLines[i]);
        }
        Console.WriteLine("Toado section extracted: " + (toadoLines.Length - startToado) + " lines.");

        Console.WriteLine("Merging into meps_orig.il...");
        int replacedCount = 0;
        int nanCount = 0;
        int infCount = 0;
        using (var reader = new StreamReader(mepsOrigPath, Encoding.UTF8))
        using (var writer = new StreamWriter(mepsFinalPath, false, Encoding.UTF8)) {
            string line;
            bool skipping = false;
            while ((line = reader.ReadLine()) != null) {
                // Fix nan and inf
                if (line.Contains("-nan(ind)")) {
                    line = line.Replace("-nan(ind)", "(00 00 00 00 00 00 F8 FF)");
                    nanCount++;
                }
                if (line.Contains("ldc.r8     inf")) {
                    line = line.Replace("inf", "(00 00 00 00 00 00 F0 7F)");
                    infCount++;
                }

                if (!skipping) {
                    if (line.StartsWith(".class public auto ansi sealed beforefieldinit Tvxd.Meps.ToadoSettings")) {
                        skipping = true;
                        writer.Write(newToadoSection.ToString());
                        replacedCount++;
                        continue;
                    }
                    writer.WriteLine(line);
                } else {
                    if (line.StartsWith(".class public abstract auto ansi sealed beforefieldinit Tvxd.Meps.TextWrite110")) {
                        skipping = false;
                        writer.WriteLine(line);
                    }
                }
            }
        }

        Console.WriteLine(string.Format("Done! Replaced Toado section ({0}), Fixed nan ({1}), inf ({2}).", replacedCount, nanCount, infCount));
    }
}
