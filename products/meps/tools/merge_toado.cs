using System;
using System.IO;
using System.Text;

class MergeMeps {
    static void Main() {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string toadoPartPath = Path.Combine(baseDir, "ToadoPart.il");
        string vietchuPartPath = Path.Combine(baseDir, "VietchuPart.il");
        string tagSeqPartPath = Path.Combine(baseDir, "TagSeqPart.il");
        string modernThemePartPath = Path.Combine(baseDir, "ModernThemeEngine.il");
        string htktWirePartPath = Path.Combine(baseDir, "HtktWireFix027.il");
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

        Console.WriteLine("Reading TagSeqPart.il from: " + tagSeqPartPath);
        var tagSeqLines = File.ReadAllLines(tagSeqPartPath);
        int startTagSeq = -1;
        for (int i = 0; i < tagSeqLines.Length; i++) {
            if (tagSeqLines[i].StartsWith(".class ") && tagSeqLines[i].Contains("Tvxd.Meps.TagSeqDir073")) {
                startTagSeq = i;
                break;
            }
        }
        var newTagSeqSection = new StringBuilder();
        if (startTagSeq >= 0) {
            for (int i = startTagSeq; i < tagSeqLines.Length; i++) {
                newTagSeqSection.AppendLine(tagSeqLines[i]);
            }
            Console.WriteLine("TagSeq section extracted: " + (tagSeqLines.Length - startTagSeq) + " lines.");
        }

        Console.WriteLine("Reading ModernThemeEngine.il from: " + modernThemePartPath);
        var modernThemeLines = File.Exists(modernThemePartPath) ? File.ReadAllLines(modernThemePartPath) : new string[0];
        int startModernTheme = -1;
        for (int i = 0; i < modernThemeLines.Length; i++) {
            if (modernThemeLines[i].StartsWith(".class ") && modernThemeLines[i].Contains("Tvxd.Meps.ModernThemeEngine")) {
                startModernTheme = i;
                break;
            }
        }
        var newModernThemeSection = new StringBuilder();
        if (startModernTheme >= 0) {
            for (int i = startModernTheme; i < modernThemeLines.Length; i++) {
                if (modernThemeLines[i].StartsWith("// =============================================================")) break;
                newModernThemeSection.AppendLine(modernThemeLines[i]);
            }
            Console.WriteLine("ModernThemeEngine section extracted: " + (modernThemeLines.Length - startModernTheme) + " lines.");
        }

        var newHtktWireSection = new StringBuilder();
        if (File.Exists(htktWirePartPath)) {
            var hwLines = File.ReadAllLines(htktWirePartPath);
            int startHw = -1;
            for (int i = 0; i < hwLines.Length; i++) {
                if (hwLines[i].StartsWith(".class ") && hwLines[i].Contains("Tvxd.Meps.HtktWireFix027")) {
                    startHw = i;
                    break;
                }
            }
            if (startHw >= 0) {
                for (int i = startHw; i < hwLines.Length; i++) {
                    newHtktWireSection.AppendLine(hwLines[i]);
                }
                Console.WriteLine("HtktWireFix027 section extracted: " + (hwLines.Length - startHw) + " lines.");
            }
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
        int replacedTagSeq = 0;
        int injectedModernTheme = 0;
        int hookedArrangeActions = 0;
        int replacedSectionHeight = 0;
        int replacedTitles = 0;
        int replacedButtons = 0;
        int nanCount = 0;
        int infCount = 0;
        int patchedRouteStatsCommands = 0;
        int patchedHtktLayer = 0;
        int patchedHtktKnown = 0;
        int patchedWireEnsure = 0;
        int patchedWireCheck = 0;

        using (var reader = new StreamReader(mepsOrigPath, Encoding.UTF8))
        using (var writer = new StreamWriter(mepsFinalPath, false, Encoding.UTF8)) {
            string line;
            bool skippingToado = false;
            bool skippingVietchu = false;
            bool skippingTagSeq = false;
            bool insideArrangeActions = false;
            bool afterToolbox106 = false;
            bool insideRouteStatsCctor = false;
            bool afterCdsInRouteStats = false;
            bool insideHtktLayerName = false;
            bool insideHtktKnown = false;
            bool insideEnsureType = false;
            bool insideWireCheckSettings = false;

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

                // Hook ArrangeActions to apply ModernThemeEngine
                if (line.Contains("ArrangeActions() cil managed")) {
                    insideArrangeActions = true;
                }
                if (insideArrangeActions && line.Contains("leave.s    IL_039a")) {
                    writer.WriteLine("      ldarg.0");
                    writer.WriteLine("      call       void Tvxd.Meps.ModernThemeEngine::ApplyTheme(class [System.Windows.Forms]System.Windows.Forms.Control)");
                    insideArrangeActions = false;
                    hookedArrangeActions++;
                }

                // Expand TRIEN KHAI VA THONG KE section height from 186 (0xba) to 240 (0xf0)
                if (line.Contains("Tvxd.Meps.CompactView06::Toolbox106")) {
                    afterToolbox106 = true;
                }
                if (afterToolbox106 && line.Contains("ldc.i4     0xba")) {
                    line = line.Replace("0xba", "0xf0");
                    afterToolbox106 = false;
                    replacedSectionHeight++;
                }

                // Whitelist T-KLHD in RouteStats076::Commands
                if (line.Contains("RouteStats076::'<Hook>b__6'")) {
                    insideRouteStatsCctor = true;
                }
                if (insideRouteStatsCctor) {
                    if (line.Contains("ldc.i4.7") && !line.Contains("stelem.ref")) {
                        line = line.Replace("ldc.i4.7", "ldc.i4.8");
                    }
                    if (line.Contains("\"CDS\"")) {
                        afterCdsInRouteStats = true;
                    }
                    if (afterCdsInRouteStats && line.Contains("stelem.ref")) {
                        writer.WriteLine(line);
                        writer.WriteLine("    ldloc.0");
                        writer.WriteLine("    ldc.i4.7");
                        writer.WriteLine("    ldstr      \"T-KLHD\"");
                        writer.WriteLine("    stelem.ref");
                        afterCdsInRouteStats = false;
                        patchedRouteStatsCommands++;
                        continue;
                    }
                    if (line.Contains("} // end of method RouteStats076::.cctor")) {
                        insideRouteStatsCctor = false;
                    }
                }

                // 1. Patch HtktLayer096::Name to resolve HTKT-BE-*, HTKT-GANIVO-*, HTKT-TU-*, Cot den kep *
                if (patchedHtktLayer == 0 && line.Contains("Name(class Tvxd.Meps.LibEntry e)")) insideHtktLayerName = true;
                if (insideHtktLayerName) {
                    if (line.Contains("IL_0096:  ldstr")) {
                        writer.WriteLine("    IL_0096:  ldloc.0");
                        writer.WriteLine("    call       string Tvxd.Meps.HtktWireFix027::ResolveHtktLayer(string)");
                        writer.WriteLine("    dup");
                        writer.WriteLine("    brfalse.s  IL_0096_THROW");
                        writer.WriteLine("    ret");
                        writer.WriteLine("    IL_0096_THROW: pop");
                        writer.WriteLine(line.Replace("IL_0096:", "         "));
                        patchedHtktLayer++;
                        insideHtktLayerName = false;
                        continue;
                    }
                    if (line.Contains("} // end of method")) insideHtktLayerName = false;
                }

                // 1b. Patch HtktLayer096::Known to accept E-HT-TTLL
                if (patchedHtktKnown == 0 && line.Contains("Known(string name)")) insideHtktKnown = true;
                if (insideHtktKnown) {
                    if (line.Contains("IL_0000:  nop")) {
                        writer.WriteLine(line);
                        writer.WriteLine("    ldarg.0");
                        writer.WriteLine("    ldstr      \"E-HT-TTLL\"");
                        writer.WriteLine("    call       bool [mscorlib]System.String::Equals(string, string)");
                        writer.WriteLine("    brfalse.s  IL_0001");
                        writer.WriteLine("    ldc.i4.1");
                        writer.WriteLine("    ret");
                        patchedHtktKnown++;
                        insideHtktKnown = false;
                        continue;
                    }
                    if (line.Contains("} // end of method")) insideHtktKnown = false;
                }

                // 2. Patch WireCad010::EnsureType to load DIEN_SS_*_SS from linetypes076
                if (line.Contains("EnsureType(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db,")) insideEnsureType = true;
                if (insideEnsureType) {
                    if (line.Contains("IL_0000:  nop")) {
                        writer.WriteLine(line);
                        writer.WriteLine("    ldarg.0");
                        writer.WriteLine("    ldarg.1");
                        writer.WriteLine("    ldarg.2");
                        writer.WriteLine("    ldfld      string Tvxd.Meps.WireStyle010::linetype");
                        writer.WriteLine("    call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.HtktWireFix027::EnsureCustomLinetype(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database, class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction, string)");
                        writer.WriteLine("    stloc.s    V_7");
                        writer.WriteLine("    ldloca.s   V_7");
                        writer.WriteLine("    call       instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_IsNull()");
                        writer.WriteLine("    brtrue.s   IL_0001");
                        writer.WriteLine("    ldloc.s    V_7");
                        writer.WriteLine("    ret");
                        patchedWireEnsure++;
                        continue;
                    }
                    if (line.Contains("} // end of method WireCad010::EnsureType")) insideEnsureType = false;
                }

                // 3. Patch WireModel010::CheckSettings to allow custom DIEN_SS_*_SS linetypes regardless of MSLTSCALE
                if (line.Contains("CheckSettings(class Tvxd.Meps.WireStyle010 s,")) insideWireCheckSettings = true;
                if (insideWireCheckSettings) {
                    if (line.Contains("IL_000e:  nop")) {
                        writer.WriteLine(line);
                        writer.WriteLine("    ldarg.0");
                        writer.WriteLine("    ldfld      string Tvxd.Meps.WireStyle010::linetype");
                        writer.WriteLine("    call       bool Tvxd.Meps.HtktWireFix027::IsCustomLinetype(string)");
                        writer.WriteLine("    brfalse.s  IL_000f");
                        writer.WriteLine("    ret");
                        patchedWireCheck++;
                        continue;
                    }
                    if (line.Contains("} // end of method WireModel010::CheckSettings")) insideWireCheckSettings = false;
                }

                // Inject ModernThemeEngine & HtktWireFix027 before disassembly completion marker
                if (line.StartsWith("// *********** DISASSEMBLY COMPLETE")) {
                    if (newHtktWireSection.Length > 0) {
                        writer.Write(newHtktWireSection.ToString());
                    }
                    if (newModernThemeSection.Length > 0) {
                        writer.Write(newModernThemeSection.ToString());
                        injectedModernTheme++;
                    }
                }

                // Section replacements
                if (!skippingToado && !skippingVietchu && !skippingTagSeq) {
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
                    if (line.StartsWith(".class ") && line.Contains("Tvxd.Meps.TagSeqDir073")) {
                        skippingTagSeq = true;
                        writer.Write(newTagSeqSection.ToString());
                        replacedTagSeq++;
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
                        if (line.StartsWith(".class ") && line.Contains("Tvxd.Meps.TagSeqDir073")) {
                            skippingTagSeq = true;
                            writer.Write(newTagSeqSection.ToString());
                            replacedTagSeq++;
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
                        if (line.StartsWith(".class ") && line.Contains("Tvxd.Meps.TagSeqDir073")) {
                            skippingTagSeq = true;
                            writer.Write(newTagSeqSection.ToString());
                            replacedTagSeq++;
                            continue;
                        }
                        writer.WriteLine(line);
                    }
                } else if (skippingTagSeq) {
                    // TagSeq ends after TagSeqCad073
                    if (line.StartsWith(".class ") && !line.Contains("Tvxd.Meps.TagSeqDir073") &&
                        !line.Contains("Tvxd.Meps.TagSeqEntry073") &&
                        !line.Contains("Tvxd.Meps.TagSeqOptions073") &&
                        !line.Contains("Tvxd.Meps.TagSeqRow073") &&
                        !line.Contains("Tvxd.Meps.TagSequence073") &&
                        !line.Contains("Tvxd.Meps.TagSeqCad073")) {
                        skippingTagSeq = false;
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
                    }
                }
            }
        }

        Console.WriteLine(string.Format("Done! Replaced Toado ({0}), Vietchu ({1}), TagSeq ({2}), Injected ModernTheme ({3}), Hooked ArrangeActions ({4}), Height ({5}), Titles ({6}), Buttons ({7}), Fixed nan ({8}), inf ({9}), RouteStats076 ({10}), HtktLayer ({11}), HtktKnown ({12}), WireEnsure ({13}), WireCheck ({14}).",
            replacedToado, replacedVietchu, replacedTagSeq, injectedModernTheme, hookedArrangeActions, replacedSectionHeight, replacedTitles, replacedButtons, nanCount, infCount, patchedRouteStatsCommands, patchedHtktLayer, patchedHtktKnown, patchedWireEnsure, patchedWireCheck));
        if (patchedHtktLayer != 1 || patchedHtktKnown != 1 || patchedWireEnsure != 1 || patchedWireCheck != 1) {
            throw new Exception("Missing required Task-0027 IL hook!");
        }
    }
}
