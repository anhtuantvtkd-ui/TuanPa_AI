using System;
using System.Collections.Generic;
using Tvxd.Meps;

class Program
{
    static int Main()
    {
        Console.WriteLine("Running Unit Tests for TagSequence073 Row Banding (TASK-0018)...");

        // Scenario: 2 rows of blocks, slightly uneven Y
        // Row 1 (around Y=100): Block A (X=10, Y=100.2), Block B (X=50, Y=99.8), Block C (X=100, Y=100.1)
        // Row 2 (around Y=50):  Block D (X=12, Y=50.1),  Block E (X=48, Y=49.9),  Block F (X=98, Y=50.0)
        // User selection order (arbitrary): C, A, E, F, D, B
        var entries = new List<TagSeqEntry073>
        {
            new TagSeqEntry073 { id = "H_C", name = "TAG", oldText = "?", x = 100.0, y = 100.1 },
            new TagSeqEntry073 { id = "H_A", name = "TAG", oldText = "?", x = 10.0,  y = 100.2 },
            new TagSeqEntry073 { id = "H_E", name = "TAG", oldText = "?", x = 48.0,  y = 49.9 },
            new TagSeqEntry073 { id = "H_F", name = "TAG", oldText = "?", x = 98.0,  y = 50.0 },
            new TagSeqEntry073 { id = "H_D", name = "TAG", oldText = "?", x = 12.0,  y = 50.1 },
            new TagSeqEntry073 { id = "H_B", name = "TAG", oldText = "?", x = 50.0,  y = 99.8 }
        };

        var opt = new TagSeqOptions073
        {
            direction = "L-R",
            prefix = "TAG-",
            start = 1,
            width = 2,
            toleranceY = 2.0 // row tolerance 2 drawing units
        };

        var plan = TagSequence073.Plan(entries, opt);

        Console.WriteLine("Resulting Plan for L-R:");
        foreach (var r in plan)
        {
            Console.WriteLine(string.Format("  Ordinal {0}: id={1}, newText={2}, X={3}, Y={4}", r.ordinal, r.id, r.newText, r.x, r.y));
        }

        // Expected order:
        // Top row (Y ~ 100): A (X=10) -> B (X=50) -> C (X=100)  [TAG-01, TAG-02, TAG-03]
        // Bottom row (Y ~ 50): D (X=12) -> E (X=48) -> F (X=98) [TAG-04, TAG-05, TAG-06]
        if (plan.Count != 6) { Console.WriteLine("FAIL: Count != 6"); return 1; }
        if (plan[0].id != "H_A" || plan[0].newText != "TAG-01") { Console.WriteLine("FAIL: Item 0 is not H_A"); return 1; }
        if (plan[1].id != "H_B" || plan[1].newText != "TAG-02") { Console.WriteLine("FAIL: Item 1 is not H_B"); return 1; }
        if (plan[2].id != "H_C" || plan[2].newText != "TAG-03") { Console.WriteLine("FAIL: Item 2 is not H_C"); return 1; }
        if (plan[3].id != "H_D" || plan[3].newText != "TAG-04") { Console.WriteLine("FAIL: Item 3 is not H_D"); return 1; }
        if (plan[4].id != "H_E" || plan[4].newText != "TAG-05") { Console.WriteLine("FAIL: Item 4 is not H_E"); return 1; }
        if (plan[5].id != "H_F" || plan[5].newText != "TAG-06") { Console.WriteLine("FAIL: Item 5 is not H_F"); return 1; }

        Console.WriteLine("SUCCESS: All row-banding assertions passed 100%!");
        return 0;
    }
}
