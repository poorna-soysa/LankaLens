import zipfile
import xml.etree.ElementTree as ET
from pathlib import Path

XLSX = Path(__file__).parent / "GNbyDistrict.xlsx"
NS = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
ns = {"m": NS}


def col_ref(cell_ref: str) -> str:
    return "".join(ch for ch in cell_ref if ch.isalpha())


def load_strings(z):
    sst = ET.fromstring(z.read("xl/sharedStrings.xml"))
    return [
        "".join(t.text or "" for t in si.iter(f"{{{NS}}}t"))
        for si in sst.findall(".//m:si", ns)
    ]


def sheet_rows(z, strings, sheet_path: str, max_rows: int = 8):
    sheet = ET.fromstring(z.read(sheet_path))
    rows = sheet.findall(".//m:sheetData/m:row", ns)[:max_rows]
    out = []
    for row in rows:
        d = {}
        for c in row.findall("m:c", ns):
            v = c.find("m:v", ns)
            if v is None or v.text is None:
                continue
            val = strings[int(v.text)] if c.get("t") == "s" else v.text
            d[col_ref(c.get("r"))] = val
        keys = sorted(d, key=lambda k: (len(k), k))
        out.append([d[k] for k in keys])
    return out


with zipfile.ZipFile(XLSX) as z:
    cp = ET.fromstring(z.read("docProps/core.xml"))
    dc = "http://purl.org/dc/elements/1.1/"
    dcterms = "http://purl.org/dc/terms/"
    print("modified:", cp.find(f"{{{dcterms}}}modified").text)
    print("created:", cp.find(f"{{{dcterms}}}created").text)
    strings = load_strings(z)
    print("shared_strings:", len(strings))
    print("first_strings:", strings[:20])
    for i, name in [(1, "No. of GNDs by DSD & District"), (2, "No. of GNDs & DSDs by District")]:
        print(f"\n=== SHEET {i}: {name} ===")
        rows = sheet_rows(z, strings, f"xl/worksheets/sheet{i}.xml", max_rows=10)
        for r in rows:
            print(r)

    sheet2 = ET.fromstring(z.read("xl/worksheets/sheet2.xml"))
    strings = load_strings(z)
    total_gn = 0
    total_ds = 0
    district_rows = 0
    for row in sheet2.findall(".//m:sheetData/m:row", ns)[3:]:
        d = {}
        for c in row.findall("m:c", ns):
            v = c.find("m:v", ns)
            if v is None or v.text is None:
                continue
            val = strings[int(v.text)] if c.get("t") == "s" else v.text
            d[col_ref(c.get("r"))] = val
        keys = sorted(d, key=lambda k: (len(k), k))
        vals = [d[k] for k in keys]
        if len(vals) >= 3 and str(vals[0]).strip().lower() != "total":
            district_rows += 1
            total_ds += int(float(vals[1]))
            total_gn += int(float(vals[2]))
        elif len(vals) >= 3 and str(vals[0]).strip().lower() == "total":
            print("\nSHEET2 TOTAL ROW:", vals)
    print("\nSHEET2 SUM (district rows):", district_rows, "DS:", total_ds, "GN:", total_gn)
