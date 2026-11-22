using System;
using System.IO;
using System.Text;

class MergeMeps {
    static void Main() {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string toadoPartPath = Path.Combine(baseDir, "ToadoPart.il");
        string vietchuPartPath = Path.Combine(baseDir, "VietchuPart.il");
        string mepsOrigPath = Path.Combine(baseDir, @"base\meps_orig.il");
        if (!File.Exists(mepsOrigPath)) mepsOrigPath = Path.Combine(baseDir, "meps_orig.il");
        string mepsFinalPath = Path.Combine(baseDir, "meps_final.il");

        Console.WriteLine("Reading ToadoPart.il from: " + toadoPartPath);
        var toadoLines = File.ReadAllLines(toadoPartPath);
        int startToado = -1;
        for (int i = 0; i < toadoLines.Length; i++) {
            if (toadoLines[i].StartsWith(".class ") && toadoLines[i].Contains("Tvxd.Meps.ToadoSettings")) {
                startToado = i;
                break;
            }
        }
        var newToadoSection = new StringBuilder();
        if (startToado >= 0) {
            for (int i = startToado; i < toadoLines.Length; i++) {
                newToadoSection.AppendLine(toadoLines[i]);
            }
            Console.WriteLine("Toado section extracted: " + (toadoLines.Length - startToado) + " lines.");
        }

        Console.WriteLine("Reading VietchuPart.il from: " + vietchuPartPath);
        var vietchuLines = File.ReadAllLines(vietchuPartPath);
        int startVietchu = -1;
        for (int i = 0; i < vietchuLines.Length; i++) {
            if (vietchuLines[i].StartsWith(".class ") && vietchuLines[i].Contains("Tvxd.Meps.TextWrite110")) {
                startVietchu = i;
                break;
            }
        }
        var newVietchuSection = new StringBuilder();
        if (startVietchu >= 0) {
            for (int i = startVietchu; i < vietchuLines.Length; i++) {
                newVietchuSection.AppendLine(vietchuLines[i]);
            }
            Console.WriteLine("Vietchu section extracted: " + (vietchuLines.Length - startVietchu) + " lines.");
        }

        // Byte replacements for Palette uppercase title and button name
        // 1. "Triển khai và thống kê" -> "TRIỂN KHAI VÀ THỐNG KÊ"
        string origTitle1 = "54 00 72 00 69 00 C3 1E 6E 00 20 00 6B 00 68 00";
        string newTitle1 =  "54 00 52 00 49 00 C2 1E 4E 00 20 00 4B 00 48 00";
        string origTitle2 = "61 00 69 00 20 00 76 00 E0 00 20 00 74 00 68 00";
        string newTitle2 =  "41 00 49 00 20 00 56 00 C0 00 20 00 54 00 48 00";
        string origTitle3 = "D1 1E 6E 00 67 00 20 00 6B 00 EA 00";
        string newTitle3 =  "D0 1E 4E 00 47 00 20 00 4B 00 CA 00";

        // 2. "VIẾT CHỮ" -> "GHI CHÚ MTEXT"
        string origBtnHex = "56 00 49 00 BE 1E 54 00 20 00 43 00 48 00 EE 1E";
        string newBtnHex =  "47 00 48 00 49 00 20 00 43 00 48 00 DA 00 20 00 4D 00 54 00 45 00 58 00 54 00";

        Console.WriteLine("Merging into base IL: " + mepsOrigPath);
        int replacedToado = 0;
        int replacedVietchu = 0;
        int replacedTitles = 0;
        int replacedButtons = 0;
        int nanCount = 0;
        int infCount = 0;

        using (var reader = new StreamReader(mepsOrigPath, Encoding.UTF8))
        using (var writer = new StreamWriter(mepsFinalPath, false, Encoding.UTF8)) {
            string line;
            bool skippingToado = false;
            bool skippingVietchu = false;

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

                // Palette replacements
                if (line.Contains(origTitle1)) {
                    line = line.Replace(origTitle1, newTitle1);
                    replacedTitles++;
                }
                if (line.Contains(origTitle2)) {
                    line = line.Replace(origTitle2, newTitle2);
                    replacedTitles++;
                }
                if (line.Contains(origTitle3)) {
                    line = line.Replace(origTitle3, newTitle3);
                    replacedTitles++;
                }
                if (line.Contains(origBtnHex)) {
                    line = line.Replace(origBtnHex, newBtnHex);
                    replacedButtons++;
                }

                // Toado replacement
                if (!skippingToado && !skippingVietchu) {
                    if (line.StartsWith(".class ") && line.Contains("Tvxd.Meps.ToadoSettings")) {
                        skippingToado = true;
                        writer.Write(newToadoSection.ToString());
                        replacedToado++;
                        continue;
                    }
                    if (line.StartsWith(".class ") && line.Contains("Tvxd.Meps.TextWrite110")) {
                        skippingVietchu = true;
                        writer.Write(newVietchuSection.ToString());
                        replacedVietchu++;
                        continue;
                    }
                    writer.WriteLine(line);
                } else if (skippingToado) {
                    if (line.StartsWith(".class ") && !line.Contains("Tvxd.Meps.Toado")) {
                        skippingToado = false;
                        if (line.StartsWith(".class ") && line.Contains("Tvxd.Meps.TextWrite110")) {
                            skippingVietchu = true;
                            writer.Write(newVietchuSection.ToString());
                            replacedVietchu++;
                            continue;
                        }
                        writer.WriteLine(line);
                    }
                } else if (skippingVietchu) {
                    if (line.StartsWith(".class ") && !line.Contains("Tvxd.Meps.TextWrite110")) {
                        skippingVietchu = false;
                        if (line.StartsWith(".class ") && line.Contains("Tvxd.Meps.ToadoSettings")) {
                            skippingToado = true;
                            writer.Write(newToadoSection.ToString());
                            replacedToado++;
                            continue;
                        }
                        writer.WriteLine(line);
                    }
                }
            }
        }

        Console.WriteLine(string.Format("Done! Replaced Toado ({0}), Vietchu ({1}), Titles ({2}), Buttons ({3}), Fixed nan ({4}), inf ({5}).",
            replacedToado, replacedVietchu, replacedTitles, replacedButtons, nanCount, infCount));
    }
}
