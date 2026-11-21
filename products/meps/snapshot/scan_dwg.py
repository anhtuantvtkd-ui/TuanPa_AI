import sys
import re

dwg_path = r"C:\Users\ADMIN\.gemini\antigravity\scratch\Mat bang cap dien trung the.dwg"

with open(dwg_path, "rb") as f:
    data = f.read()

print("File size:", len(data), "bytes")
print("DWG Header Version:", data[:6].decode("ascii", errors="ignore"))

# Search for potential block names or text strings (Unicode / UTF-16LE / ASCII)
# AutoCAD 2018+ DWG (AC1032) stores text as UTF-16LE or compressed.
# Let's search for typical electrical tags: TBA, TRẠM, TỦ, CỘT, CD, CS, ĐÈN, v.v.
patterns = [b"TBA", b"TD", b"RMU", b"COT", b"TRAM", b"TOA_DO", b"VN-2000", b"TAG"]

for p in patterns:
    matches = [m.start() for m in re.finditer(re.escape(p), data, re.IGNORECASE)]
    print(f"Pattern {p.decode()}: {len(matches)} occurrences")

# Find UTF-16 strings
utf16_strings = []
for m in re.finditer(b'(?:[\x20-\x7e\xa0-\xff]\x00){4,}', data):
    try:
        s = m.group().decode('utf-16le')
        if any(keyword in s.upper() for keyword in ["TBA", "TRẠM", "CỘT", "TUYẾN", "DÂY", "ĐIỆN", "TRUNG THẾ", "22KV", "35KV", "0.4KV", "TOA DO"]):
            utf16_strings.append(s)
    except:
        pass

print(f"\nFound {len(utf16_strings)} relevant text strings:")
for s in sorted(set(utf16_strings))[:30]:
    print(" -", s)
