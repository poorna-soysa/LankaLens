using ClosedXML.Excel;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;

namespace LankaLens.DataBuilder.Parsing;

/// <summary>
/// Parses the DCS GNDList workbook into raw administrative records.
/// </summary>
internal sealed class GndListWorkbookParser
{
    public const string ExpectedSheetName = "GNDList";

    private static readonly string[] RequiredHeaders =
    [
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
        "GND_Name"
    ];

    public IReadOnlyList<RawAdministrativeRecord> Parse(Stream workbookStream)
    {
        using var workbook = new XLWorkbook(workbookStream);
        return Parse(workbook);
    }

    public IReadOnlyList<RawAdministrativeRecord> Parse(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        return Parse(workbook);
    }

    public IReadOnlyList<RawAdministrativeRecord> Parse(XLWorkbook workbook)
    {
        var worksheet = workbook.Worksheets.FirstOrDefault(ws =>
            string.Equals(ws.Name, ExpectedSheetName, StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.First();

        var headers = ReadHeaderMap(worksheet);
        EnsureRequiredHeaders(headers);

        var records = new List<RawAdministrativeRecord>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);
            var serial = GetCellText(row, headers, "Serial Number");
            var gnUid = GetCellText(row, headers, "GND_UID");

            // Skip footnote / blank trailing rows (non-numeric serial and empty UID).
            var serialNormalized = TextNormalizer.NormalizeOptionalText(serial);
            var uidNormalized = TextNormalizer.NormalizeCode(gnUid);
            if (uidNormalized is null
                || serialNormalized is null
                || !serialNormalized.All(char.IsDigit))
            {
                continue;
            }

            records.Add(new RawAdministrativeRecord
            {
                SourceRowNumber = rowNumber,
                SerialNumber = serialNormalized,
                GnUid = uidNormalized,
                ProvinceCode = GetCellText(row, headers, "Province_Code"),
                ProvinceEnglish = GetCellText(row, headers, "Province_Name"),
                ProvinceSinhala = GetOptionalLanguage(row, headers, "Province_Name_Sinhala", "Province_Sinhala"),
                ProvinceTamil = GetOptionalLanguage(row, headers, "Province_Name_Tamil", "Province_Tamil"),
                DistrictCode = GetCellText(row, headers, "District_Code"),
                DistrictEnglish = GetCellText(row, headers, "District_Name"),
                DistrictSinhala = GetOptionalLanguage(row, headers, "District_Name_Sinhala", "District_Sinhala"),
                DistrictTamil = GetOptionalLanguage(row, headers, "District_Name_Tamil", "District_Tamil"),
                DsCode = GetCellText(row, headers, "DSD_ Code"),
                DsEnglish = GetCellText(row, headers, "DSD_Name"),
                DsSinhala = GetOptionalLanguage(row, headers, "DSD_Name_Sinhala", "DSD_Sinhala"),
                DsTamil = GetOptionalLanguage(row, headers, "DSD_Name_Tamil", "DSD_Tamil"),
                GnCode = GetCellText(row, headers, "GND_ Code"),
                GnNumber = GetCellText(row, headers, "GND_NUM"),
                GnEnglish = GetCellText(row, headers, "GND_Name"),
                GnSinhala = GetOptionalLanguage(row, headers, "GND_Name_Sinhala", "GND_Sinhala"),
                GnTamil = GetOptionalLanguage(row, headers, "GND_Name_Tamil", "GND_Tamil"),
                LgCode = GetCellText(row, headers, "LG_Code"),
                LgName = GetCellText(row, headers, "LG_Name")
            });
        }

        return records;
    }

    private static Dictionary<string, int> ReadHeaderMap(IXLWorksheet worksheet)
    {
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        var headerRow = worksheet.Row(1);
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        for (var col = 1; col <= lastColumn; col++)
        {
            var header = headerRow.Cell(col).GetFormattedString()?.Trim();
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            map[header] = col;
        }

        return map;
    }

    private static void EnsureRequiredHeaders(IReadOnlyDictionary<string, int> headers)
    {
        var missing = RequiredHeaders.Where(h => !headers.ContainsKey(h)).ToList();
        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                "GNDList workbook is missing required headers: " + string.Join(", ", missing));
        }
    }

    private static string? GetCellText(
        IXLRow row,
        IReadOnlyDictionary<string, int> headers,
        string header)
    {
        if (!headers.TryGetValue(header, out var column))
        {
            return null;
        }

        return ReadCellAsString(row.Cell(column));
    }

    private static string? GetOptionalLanguage(
        IXLRow row,
        IReadOnlyDictionary<string, int> headers,
        params string[] headerCandidates)
    {
        foreach (var header in headerCandidates)
        {
            if (headers.TryGetValue(header, out var column))
            {
                return ReadCellAsString(row.Cell(column));
            }
        }

        return null;
    }

    internal static string? ReadCellAsString(IXLCell cell)
    {
        if (cell.IsEmpty())
        {
            return null;
        }

        // Prefer the underlying value so numeric codes keep their numeric identity,
        // then format without scientific notation / trailing .0 when integral.
        if (cell.DataType == XLDataType.Number)
        {
            var number = cell.GetDouble();
            if (number == Math.Floor(number)
                && number >= 0
                && number < 1_000_000_000_000d)
            {
                return ((long)number).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            return number.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        var formatted = cell.GetFormattedString();
        if (!string.IsNullOrWhiteSpace(formatted))
        {
            return formatted;
        }

        return cell.GetString();
    }
}
