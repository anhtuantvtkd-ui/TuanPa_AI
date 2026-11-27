import { test } from "node:test";
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { execFileSync } from "node:child_process";

const ROOT = path.resolve(import.meta.dirname, "..", "..", "..");
const SRC_LSP = path.join(ROOT, "products", "meps", "src", "bkl_ttll.lsp");
const SCR_PATH = path.join(ROOT, "products", "meps", "test", "RunBklTtllSynthetic.scr");
const OUT_XLSX = path.join(ROOT, "products", "meps", "test", "synthetic_ttll_out.xlsx");
const OUT_CSV = path.join(ROOT, "products", "meps", "test", "synthetic_ttll_out.csv");
const ACCORE = "C:\\Program Files\\Autodesk\\AutoCAD 2021\\accoreconsole.exe";

test("bkl_ttll.lsp exists, defines T-THTTLL/BKL_TTLL/TTLL and avoids layoutlist/vla-get-Layouts", () => {
  const src = fs.readFileSync(SRC_LSP, "utf8");
  assert.match(src, /\(defun c:T-THTTLL/);
  assert.match(src, /\(defun c:BKL_TTLL/);
  assert.match(src, /\(defun c:TTLL/);
  assert.match(src, /\(defun bkl-ttll:run-with-path/);
  assert.equal(/\blayoutlist\b/i.test(src), false);
  assert.equal(/\bvla-get-Layouts\b/i.test(src), false);
});

test("accoreconsole.exe synthetic TTLL produces accurate CSV, XLSX formulas and warnings", { skip: !fs.existsSync(ACCORE) }, () => {
  for (const f of [OUT_XLSX, OUT_CSV]) {
    if (fs.existsSync(f)) fs.unlinkSync(f);
  }

  execFileSync(ACCORE, ["/s", SCR_PATH], { cwd: ROOT, stdio: "pipe", timeout: 60000 });

  assert.ok(fs.existsSync(OUT_CSV), "synthetic_ttll_out.csv should exist");
  assert.ok(fs.existsSync(OUT_XLSX), "synthetic_ttll_out.xlsx should exist");

  const csv = fs.readFileSync(OUT_CSV, "utf8").replace(/^\uFEFF/, "");
  const lines = csv.trim().split(/\r?\n/);
  const warnIdx = lines.indexOf("WARNINGS");
  assert.ok(warnIdx > 1, "CSV should contain WARNINGS section");
  const dataLines = lines.slice(1, warnIdx).filter((l) => l.trim() !== "");
  const warnLines = lines.slice(warnIdx + 1).filter((l) => l.trim() !== "");

  // Strict row and warning counts (no extra rows or legend leaks)
  assert.equal(dataLines.length, 8, `Expected exactly 8 data rows, got ${dataLines.length}`);
  assert.equal(warnLines.length, 2, `Expected exactly 2 warnings, got ${warnLines.length}: ${warnLines.join(" | ")}`);

  // Check expected spans
  assert.ok(dataLines.some((l) => l.includes('"B1","B2",2D110,80,0,15,0')), "B1->B2 2D110 80m with QD 15m");
  assert.ok(dataLines.some((l) => l.includes('"B2","B3",2D110,80,0,0,0') && l.endsWith("35+45")), "B2->B3 multi-dim 35+45");
  assert.ok(dataLines.some((l) => l.includes('"B2","B4",4D110,50,0,0,0')), "B2->B4 4D110 50m across deg-4 node");
  assert.ok(dataLines.some((l) => l.includes('"B1","B3",4D110,168,0,0,0')), "B1->B3 4D110 168m across deg-4 node");
  assert.ok(dataLines.some((l) => l.includes('"B2","TT-01",1D110,1,0,0,0,1,0,0,2,1,2,0,0,0,1')), "1m span B2->TT-01");
  assert.ok(dataLines.some((l) => l.includes('"B3","Cột 19/1",1D110,20,0,25,0,0,2,0,22,1,22')), "B3->Cột 19/1 1D110 with QDHT 25m > KC 20m");
  assert.ok(dataLines.some((l) => l.includes('"TT-01","G1",D60,10,0,0,0,1,0,0,11,2,0,22,0,1,0')), "TT-01->G1 (normalized from B1)");
  assert.ok(dataLines.some((l) => l.includes('"G1","G2",D60,10,0,0,0,0,0,0,10,2,0,20,0,1,0')), "G1->G2 D60");
  assert.ok(warnLines.some((l) => l.includes("2D110") && l.includes("12m")), "Dead-end 2D110(12m) warning");
  assert.ok(warnLines.some((l) => l.includes("QĐHT") && l.includes("25m") && l.includes("20m")), "QDHT > KC warning");

  // Inspect XLSX OpenXML contents via PowerShell ZipFile
  const inspectPs = `
Add-Type -AssemblyName System.IO.Compression.FileSystem
$z = [System.IO.Compression.ZipFile]::OpenRead('${OUT_XLSX.replace(/'/g, "''")}')
$wb = (New-Object System.IO.StreamReader($z.GetEntry('xl/workbook.xml').Open())).ReadToEnd()
$s1 = (New-Object System.IO.StreamReader($z.GetEntry('xl/worksheets/sheet1.xml').Open())).ReadToEnd()
$s2 = (New-Object System.IO.StreamReader($z.GetEntry('xl/worksheets/sheet2.xml').Open())).ReadToEnd()
$s3 = (New-Object System.IO.StreamReader($z.GetEntry('xl/worksheets/sheet3.xml').Open())).ReadToEnd()
$z.Dispose()
Write-Output "WB:$wb"
Write-Output "S1:$s1"
Write-Output "S2:$s2"
Write-Output "S3:$s3"
`;
  const xmlDump = execFileSync("powershell.exe", ["-NoProfile", "-Command", inspectPs], { encoding: "utf8" });
  const s1 = xmlDump.slice(xmlDump.indexOf("S1:") + 3, xmlDump.indexOf("S2:"));
  const s2 = xmlDump.slice(xmlDump.indexOf("S2:") + 3, xmlDump.indexOf("S3:"));
  const s3 = xmlDump.slice(xmlDump.indexOf("S3:") + 3);

  assert.match(xmlDump, /name="THKL"/);
  assert.match(xmlDump, /name="DG TTLL"/);
  assert.match(xmlDump, /name="DM LOAI ONG"/);

  // THKL formulas and values (D3..D18)
  assert.match(s1, /<c r="D3" s="2"><f>(?:'|&apos;)DG TTLL(?:'|&apos;)!AN12<\/f><v>2<\/v><\/c>/);
  assert.match(s1, /<c r="D4" s="2"><f>(?:'|&apos;)DG TTLL(?:'|&apos;)!AL12<\/f><v>4<\/v><\/c>/);
  assert.match(s1, /<c r="D5" s="2"><f>(?:'|&apos;)DG TTLL(?:'|&apos;)!O12<\/f><v>42<\/v><\/c>/);
  assert.match(s1, /<c r="D6" s="2"><f>(?:'|&apos;)DG TTLL(?:'|&apos;)!N12<\/f><v>1216<\/v><\/c>/);
  assert.match(s1, /<c r="D7" s="2"><f>D3\*2\*3<\/f><v>12<\/v><\/c>/);
  assert.match(s1, /<c r="D8" s="2"><f>D3\*2<\/f><v>4<\/v><\/c>/);
  assert.match(s1, /<c r="D9" s="2"><f>(?:'|&apos;)DG TTLL(?:'|&apos;)!S12<\/f><v>1<\/v><\/c>/);
  assert.match(s1, /<c r="D10" s="2"><f>(?:'|&apos;)DG TTLL(?:'|&apos;)!U12<\/f><v>25<\/v><\/c>/);
  assert.match(s1, /<c r="D12" s="2"><f>(?:'|&apos;)DG TTLL(?:'|&apos;)!W12<\/f><v>145<\/v><\/c>/);
  assert.match(s1, /<c r="D13" s="2"><f>(?:'|&apos;)DG TTLL(?:'|&apos;)!Y12<\/f><v>15<\/v><\/c>/);
  assert.match(s1, /<c r="D14" s="2"><f>(?:'|&apos;)DG TTLL(?:'|&apos;)!AC12<\/f><v>218<\/v><\/c>/);
  assert.match(s1, /<c r="D16" s="2"><f>(?:'|&apos;)DG TTLL(?:'|&apos;)!AO12<\/f><v>1<\/v><\/c>/);
  assert.match(s1, /<c r="D17" s="2"><f>D16<\/f><v>1<\/v><\/c>/);
  assert.match(s1, /<c r="D18" s="2"><f>ROUNDUP\(D12\/15,0\)<\/f><v>10<\/v><\/c>/);
  assert.match(s1, /CẢNH BÁO ĐỐI SOÁT CAD/);

  // DG TTLL exact row count (3 header + 8 data + 1 total = 12 rows), formulas, totals, and AP check
  const dgRows = s2.match(/<row r="\d+"/g) || [];
  assert.equal(dgRows.length, 12, `Expected 12 rows in DG TTLL, got ${dgRows.length}`);
  assert.match(s2, /<f>35\+45<\/f><v>80<\/v>/);
  assert.match(s2, /<f>SUM\(E\$4:E11\)<\/f><v>419<\/v>/);
  assert.match(s2, /<f>SUM\(G\$4:G11\)<\/f><v>40<\/v>/);
  assert.match(s2, /<f>SUM\(N\$4:N11\)<\/f><v>1216<\/v>/);
  assert.match(s2, /<f>SUM\(O\$4:O11\)<\/f><v>42<\/v>/);
  assert.match(s2, /<f>SUM\(AL\$4:AL11\)<\/f><v>4<\/v>/);
  assert.match(s2, /<f>SUM\(AN\$4:AN11\)<\/f><v>2<\/v>/);
  assert.match(s2, /<f>SUM\(AO\$4:AO11\)<\/f><v>1<\/v>/);
  assert.match(s2, /<c r="AP\d+" t="inlineStr" s="2"><is><t>CHECK: QĐHT&gt;KC<\/t><\/is><\/c>/);
  const passMatches = s2.match(/<c r="AP\d+" t="inlineStr" s="2"><is><t>PASS<\/t><\/is><\/c>/g) || [];
  assert.equal(passMatches.length, 7, `Expected 7 PASS rows and 1 CHECK row in AP, got ${passMatches.length} PASS`);

  // DM LOAI ONG check
  assert.match(s3, /1D110/);
  assert.match(s3, /4D110/);

  // Clean up generated test artifacts
  for (const f of [OUT_XLSX, OUT_CSV]) {
    if (fs.existsSync(f)) fs.unlinkSync(f);
  }
});
