using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

class FixV3Checksums {
    static string HashFile(string path) {
        using (var sha = SHA256.Create())
        using (var stream = File.OpenRead(path)) {
            byte[] hash = sha.ComputeHash(stream);
            var sb = new StringBuilder();
            foreach (byte b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    static void Main() {
        string baseDir = @"C:\Users\ADMIN\.gemini\antigravity\scratch\TVXD.BG V3";
        string dllPath = Path.Combine(baseDir, @"MEPS060\TVXD.Meps.dll");
        string dllHash = HashFile(dllPath);
        Console.WriteLine("DLL SHA256: " + dllHash);

        // Delete LOGS_0_6 if exists
        string logDir = Path.Combine(baseDir, @"MEPS060\LOGS_0_6");
        if (Directory.Exists(logDir)) Directory.Delete(logDir, true);

        // 1. Update PHIEN_BAN.json
        string phienBanPath = Path.Combine(baseDir, "PHIEN_BAN.json");
        string pbText = File.ReadAllText(phienBanPath, Encoding.UTF8);
        pbText = System.Text.RegularExpressions.Regex.Replace(pbText, "\"dll_sha256\":\\s*\"[^\"]+\"", "\"dll_sha256\":  \"" + dllHash + "\"");
        File.WriteAllText(phienBanPath, pbText, Encoding.UTF8);
        Console.WriteLine("Updated PHIEN_BAN.json");

        // 2. Update RELEASE.json
        string releasePath = Path.Combine(baseDir, @"MEPS060\RELEASE.json");
        string relText = File.ReadAllText(releasePath, Encoding.UTF8);
        relText = System.Text.RegularExpressions.Regex.Replace(relText, "\"dll_sha256\":\\s*\"[^\"]+\"", "\"dll_sha256\":  \"" + dllHash + "\"");
        File.WriteAllText(releasePath, relText, Encoding.UTF8);
        Console.WriteLine("Updated RELEASE.json");

        // 3. Update SHA256SUMS.json
        string sumsPath = Path.Combine(baseDir, "SHA256SUMS.json");
        string sumsText = File.ReadAllText(sumsPath, Encoding.UTF8);

        // Simple JSON parser / serializer for the array of objects
        var matches = System.Text.RegularExpressions.Regex.Matches(sumsText, "\\{\"path\":\"(?<path>[^\"]+)\",\"sha256\":\"(?<sha>[^\"]+)\"\\}");
        var entries = new List<string>();
        int updated = 0;

        foreach (System.Text.RegularExpressions.Match m in matches) {
            string relPath = m.Groups["path"].Value;
            string sysRelPath = relPath.Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(baseDir, sysRelPath);
            if (!File.Exists(fullPath)) {
                Console.WriteLine("Warning: File does not exist: " + fullPath);
                continue;
            }
            string currentHash = HashFile(fullPath);
            entries.Add(string.Format("{{\"path\":\"{0}\",\"sha256\":\"{1}\"}}", relPath, currentHash));
            if (!currentHash.Equals(m.Groups["sha"].Value, StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine(string.Format("Updated {0}: {1} -> {2}", relPath, m.Groups["sha"].Value, currentHash));
                updated++;
            }
        }

        string newSums = "[" + string.Join(",", entries.ToArray()) + "]";
        File.WriteAllText(sumsPath, newSums, Encoding.UTF8);
        Console.WriteLine(string.Format("SHA256SUMS.json written with {0} items ({1} hashes updated).", entries.Count, updated));
    }
}
