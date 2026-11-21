using System;
using System.IO;
using System.Text;

class MergeToado {
    static void Main() {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string toadoPartPath = Path.Combine(baseDir, "ToadoPart.il");
        string mepsOrigPath = Path.Combine(baseDir, @"base\meps_orig.il");
        if (!File.Exists(mepsOrigPath)) mepsOrigPath = Path.Combine(baseDir, "meps_orig.il");
        string mepsFinalPath = Path.Combine(baseDir, "meps_final.il");

        Console.WriteLine("Reading ToadoPart.il from: " + toadoPartPath);
        var toadoLines = File.ReadAllLines(toadoPartPath);
        int startToado = -1;
        for (int i = 0; i < toadoLines.Length; i++) {
            if (toadoLines[i].StartsWith(".class public auto ansi sealed beforefieldinit Tvxd.Meps.ToadoSettings") ||
                toadoLines[i].Contains("Tvxd.Meps.ToadoSettings")) {
                if (toadoLines[i].StartsWith(".class ") && toadoLines[i].Contains("Tvxd.Meps.ToadoSettings")) {
                    startToado = i;
                    break;
                }
            }
        }
        if (startToado < 0) {
            Console.WriteLine("Error: ToadoSettings not found in ToadoPart.il");
            Environment.Exit(1);
            return;
        }

        var newToadoSection = new StringBuilder();
        for (int i = startToado; i < toadoLines.Length; i++) {
            newToadoSection.AppendLine(toadoLines[i]);
        }
        Console.WriteLine("Toado section extracted: " + (toadoLines.Length - startToado) + " lines.");

        Console.WriteLine("Merging into base IL: " + mepsOrigPath);
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
                    if (line.StartsWith(".class public auto ansi sealed beforefieldinit Tvxd.Meps.ToadoSettings") ||
                        (line.StartsWith(".class ") && line.Contains("Tvxd.Meps.ToadoSettings"))) {
                        skipping = true;
                        writer.Write(newToadoSection.ToString());
                        replacedCount++;
                        continue;
                    }
                    writer.WriteLine(line);
                } else {
                    if (line.StartsWith(".class ") && !line.Contains("Tvxd.Meps.Toado")) {
                        skipping = false;
                        writer.WriteLine(line);
                    }
                }
            }
        }

        Console.WriteLine(string.Format("Done! Replaced Toado section ({0}), Fixed nan ({1}), inf ({2}).", replacedCount, nanCount, infCount));
    }
}
