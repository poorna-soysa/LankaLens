import re
import zipfile
import xml.etree.ElementTree as ET
from collections import Counter
from pathlib import Path

XLSX = Path(__file__).parent / "GNDList_Final.xlsx"
HTML = Path(__file__).parent / "GNbyDistrict.html"
NS = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
ns = {"m": NS}


def col_ref(cell_ref: str) -> str:
    return "".join(ch for ch in cell_ref if ch.isalpha())


def analyze_xlsx() -> None:
    with zipfile.ZipFile(XLSX) as z:
        sst = ET.fromstring(z.read("xl/sharedStrings.xml"))
        strings = [
            "".join(t.text or "" for t in si.iter(f"{{{NS}}}t"))
            for si in sst.findall(".//m:si", ns)
        ]
        sheet = ET.fromstring(z.read("xl/worksheets/sheet1.xml"))
        rows = sheet.findall(".//m:sheetData/m:row", ns)

        def row_vals(row):
            d = {}
            for c in row.findall("m:c", ns):
                v = c.find("m:v", ns)
                if v is None or v.text is None:
                    continue
                val = strings[int(v.text)] if c.get("t") == "s" else v.text
                d[col_ref(c.get("r"))] = val
            keys = sorted(d, key=lambda k: (len(k), k))
            return [d[k] for k in keys]

        headers = row_vals(rows[0])
        print("HEADERS:", headers)
        uids = []
        lgs = Counter()
        short_rows = 0
        nonempty_gndnum = 0
        prov = set()
        dist = set()
        dsd = set()

        for row in rows[1:]:
            v = row_vals(row)
            if len(v) < 13:
                short_rows += 1
                continue
            uids.append(v[1])
            lgs[(v[11], v[12])] += 1
            if str(v[9]).strip() not in ("", "None", " "):
                nonempty_gndnum += 1
            prov.add(str(v[2]))
            dist.add((str(v[2]), str(v[4])))
            dsd.add((str(v[2]), str(v[4]), str(v[6])))

        print("ROWS:", len(rows) - 1)
        print("SHORT_ROWS:", short_rows)
        print("PROVINCES:", len(prov), sorted(prov, key=int))
        print("DISTRICTS:", len(dist))
        print("DSDS:", len(dsd))
        print("UNIQUE GND_UID:", len(set(uids)), "TOTAL:", len(uids))
        print("GND_NUM non-empty:", nonempty_gndnum)
        print("TOP LG:", lgs.most_common(10))
        print("SAMPLE ROW:", row_vals(rows[1]))


def analyze_html() -> None:
    text = HTML.read_bytes().decode("utf-8", "replace")
    print("HTML_BYTES:", len(text))
    title = re.search(r"<title>(.*?)</title>", text, re.I | re.S)
    print("TITLE:", (title.group(1).strip() if title else "none")[:160])
    for pat in [
        "Grama Niladhari",
        "No. of GN",
        "Number of Grama",
        "Divisional Secretariat",
    ]:
        print(f"HAS {pat!r}:", pat.lower() in text.lower())
    for m in re.finditer(r"<a[^>]+href=\"([^\"]+)\"[^>]*>([^<]+)</a>", text, re.I):
        href, label = m.group(1), m.group(2).strip()
        if any(k in label for k in ("District", "Grama", "GND", "Colombo", "Kandy")):
            print("LINK:", label[:80], "=>", href[:120])


if __name__ == "__main__":
    analyze_xlsx()
    print("---")
    analyze_html()
