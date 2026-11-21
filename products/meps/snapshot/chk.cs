using System;
using System.IO;
using System.Text.RegularExpressions;

class Program {
    static void Main() {
        string ilPath = @"C:\Users\ADMIN\.gemini\antigravity\scratch\toado100.il";
        Console.WriteLine("toado100.il size: " + new FileInfo(ilPath).Length);
    }
}
