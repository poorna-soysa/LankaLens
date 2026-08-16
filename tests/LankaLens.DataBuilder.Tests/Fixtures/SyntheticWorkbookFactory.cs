using ClosedXML.Excel;

namespace LankaLens.DataBuilder.Tests.Fixtures;

/// <summary>
/// Builds tiny synthetic DCS-shaped workbooks for unit tests.
/// </summary>
internal static class SyntheticWorkbookFactory
{
    public static MemoryStream CreateGndList(
        IEnumerable<SyntheticGndRow> rows,
        bool includeSinhalaTamilColumns = true)
    {
        var stream = new MemoryStream();
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("GNDList");

        var headers = new List<string>
        {
            "Serial Number",
            "GND_UID",
            "Province_Code",
            "Province_Name",
            "District_Code",
            "District_Name",
            "DSD_ Code",
            "DSD_Name",
            "GND_ Code",
            "GND_NUM",
            "GND_Name",
            "LG_Code",
            "LG_Name"
        };

        if (includeSinhalaTamilColumns)
        {
            headers.AddRange(
            [
                "Province_Name_Sinhala",
                "Province_Name_Tamil",
                "District_Name_Sinhala",
                "District_Name_Tamil",
                "DSD_Name_Sinhala",
                "DSD_Name_Tamil",
                "GND_Name_Sinhala",
                "GND_Name_Tamil"
            ]);
        }

        for (var i = 0; i < headers.Count; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
        }

        var rowNumber = 2;
        var serial = 1;
        foreach (var row in rows)
        {
            sheet.Cell(rowNumber, 1).Value = serial;
            sheet.Cell(rowNumber, 2).Value = long.Parse(row.GnUid);
            sheet.Cell(rowNumber, 3).Value = int.Parse(row.ProvinceCode);
            sheet.Cell(rowNumber, 4).Value = row.ProvinceEnglish;
            sheet.Cell(rowNumber, 5).Value = int.Parse(row.DistrictCode);
            sheet.Cell(rowNumber, 6).Value = row.DistrictEnglish;
            sheet.Cell(rowNumber, 7).Value = int.Parse(row.DsCode);
            sheet.Cell(rowNumber, 8).Value = row.DsEnglish;
            sheet.Cell(rowNumber, 9).Value = int.Parse(row.GnCode);
            sheet.Cell(rowNumber, 10).Value = row.GnNumber ?? string.Empty;
            sheet.Cell(rowNumber, 11).Value = row.GnEnglish;
            sheet.Cell(rowNumber, 12).Value = row.LgCode ?? string.Empty;
            sheet.Cell(rowNumber, 13).Value = row.LgName ?? string.Empty;

            if (includeSinhalaTamilColumns)
            {
                sheet.Cell(rowNumber, 14).Value = row.ProvinceSinhala ?? string.Empty;
                sheet.Cell(rowNumber, 15).Value = row.ProvinceTamil ?? string.Empty;
                sheet.Cell(rowNumber, 16).Value = row.DistrictSinhala ?? string.Empty;
                sheet.Cell(rowNumber, 17).Value = row.DistrictTamil ?? string.Empty;
                sheet.Cell(rowNumber, 18).Value = row.DsSinhala ?? string.Empty;
                sheet.Cell(rowNumber, 19).Value = row.DsTamil ?? string.Empty;
                sheet.Cell(rowNumber, 20).Value = row.GnSinhala ?? string.Empty;
                sheet.Cell(rowNumber, 21).Value = row.GnTamil ?? string.Empty;
            }

            rowNumber++;
            serial++;
        }

        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    public static IReadOnlyList<SyntheticGndRow> ValidTrilingualHierarchy() =>
    [
        new(
            GnUid: "1103005",
            ProvinceCode: "1",
            ProvinceEnglish: "Western",
            ProvinceSinhala: "බස්නාහිර",
            ProvinceTamil: "மேல்",
            DistrictCode: "1",
            DistrictEnglish: "Colombo",
            DistrictSinhala: "කොළඹ",
            DistrictTamil: "கொழும்பு",
            DsCode: "3",
            DsEnglish: "Colombo",
            DsSinhala: "කොළඹ",
            DsTamil: "கொழும்பு",
            GnCode: "5",
            GnNumber: "1",
            GnEnglish: "Sammanthranapura",
            GnSinhala: "සම්මන්ත්‍රණපුර",
            GnTamil: "சம்மந்திரணபுர"),
        new(
            GnUid: "1103010",
            ProvinceCode: "1",
            ProvinceEnglish: "Western",
            ProvinceSinhala: "බස්නාහිර",
            ProvinceTamil: "மேல்",
            DistrictCode: "1",
            DistrictEnglish: "Colombo",
            DistrictSinhala: "කොළඹ",
            DistrictTamil: "கொழும்பு",
            DsCode: "3",
            DsEnglish: "Colombo",
            DsSinhala: "කොළඹ",
            DsTamil: "கொழும்பு",
            GnCode: "10",
            GnNumber: "2",
            GnEnglish: "Mattakkuliya",
            GnSinhala: "මට්ටක්කුලිය",
            GnTamil: "மட்டக்குளிய"),
        new(
            GnUid: "2103015",
            ProvinceCode: "2",
            ProvinceEnglish: "Central",
            ProvinceSinhala: "මධ්‍යම",
            ProvinceTamil: "மத்திய",
            DistrictCode: "1",
            DistrictEnglish: "Kandy",
            DistrictSinhala: "මහනුවර",
            DistrictTamil: "கண்டி",
            DsCode: "3",
            DsEnglish: "Thumpane",
            DsSinhala: "තුම්පනේ",
            DsTamil: "தும்பனே",
            GnCode: "15",
            GnNumber: "3",
            GnEnglish: "Sample GN",
            GnSinhala: "නියැදි",
            GnTamil: "மாதிரி")
    ];
}

internal sealed record SyntheticGndRow(
    string GnUid,
    string ProvinceCode,
    string ProvinceEnglish,
    string DistrictCode,
    string DistrictEnglish,
    string DsCode,
    string DsEnglish,
    string GnCode,
    string GnEnglish,
    string? ProvinceSinhala = null,
    string? ProvinceTamil = null,
    string? DistrictSinhala = null,
    string? DistrictTamil = null,
    string? DsSinhala = null,
    string? DsTamil = null,
    string? GnSinhala = null,
    string? GnTamil = null,
    string? GnNumber = null,
    string? LgCode = null,
    string? LgName = null);
