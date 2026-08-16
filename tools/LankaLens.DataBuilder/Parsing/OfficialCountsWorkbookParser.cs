using ClosedXML.Excel;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;

namespace LankaLens.DataBuilder.Parsing;

/// <summary>
/// Parses the official DCS "No. of GNDs by DSD &amp; District" workbook for count cross-checks.
/// </summary>
internal sealed class OfficialCountsWorkbookParser
{
    public IReadOnlyList<OfficialCountExpectation> ParseDistrictTotals(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        return ParseDistrictTotals(workbook);
    }

    public IReadOnlyList<OfficialCountExpectation> ParseDistrictTotals(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        return ParseDistrictTotals(workbook);
    }

    public IReadOnlyList<OfficialCountExpectation> ParseDistrictTotals(XLWorkbook workbook)
    {
        var worksheet = workbook.Worksheets.FirstOrDefault(ws =>
                ws.Name.Contains("GNDs & DSDs by District", StringComparison.OrdinalIgnoreCase)
                || ws.Name.Contains("GNDs and DSDs by District", StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.ElementAtOrDefault(1)
            ?? workbook.Worksheets.First();

        var results = new List<OfficialCountExpectation>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 1; rowNumber <= lastRow; rowNumber++)
        {
            var district = TextNormalizer.NormalizeOptionalText(
                GndListWorkbookParser.ReadCellAsString(worksheet.Cell(rowNumber, 1)));
            var dsCountText = TextNormalizer.NormalizeOptionalText(
                GndListWorkbookParser.ReadCellAsString(worksheet.Cell(rowNumber, 2)));
            var gnCountText = TextNormalizer.NormalizeOptionalText(
                GndListWorkbookParser.ReadCellAsString(worksheet.Cell(rowNumber, 3)));

            if (district is null
                || string.Equals(district, "District", StringComparison.OrdinalIgnoreCase)
                || string.Equals(district, "Total", StringComparison.OrdinalIgnoreCase)
                || district.Contains("Table-", StringComparison.OrdinalIgnoreCase)
                || district.Contains("As at", StringComparison.OrdinalIgnoreCase)
                || district.Contains("Source", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!int.TryParse(dsCountText, out var dsCount) || !int.TryParse(gnCountText, out var gnCount))
            {
                continue;
            }

            results.Add(new OfficialCountExpectation(
                DistrictEnglish: district,
                DsEnglish: null,
                GnCount: gnCount,
                DsCount: dsCount));
        }

        return results;
    }

    public IReadOnlyList<OfficialCountExpectation> ParseDsTotals(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        return ParseDsTotals(workbook);
    }

    public IReadOnlyList<OfficialCountExpectation> ParseDsTotals(XLWorkbook workbook)
    {
        var worksheet = workbook.Worksheets.FirstOrDefault(ws =>
                ws.Name.Contains("GNDs by DSD", StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.First();

        var results = new List<OfficialCountExpectation>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        string? currentDistrict = null;

        for (var rowNumber = 1; rowNumber <= lastRow; rowNumber++)
        {
            var districtCell = TextNormalizer.NormalizeOptionalText(
                GndListWorkbookParser.ReadCellAsString(worksheet.Cell(rowNumber, 1)));
            var dsCell = TextNormalizer.NormalizeOptionalText(
                GndListWorkbookParser.ReadCellAsString(worksheet.Cell(rowNumber, 2)));
            var gnCountText = TextNormalizer.NormalizeOptionalText(
                GndListWorkbookParser.ReadCellAsString(worksheet.Cell(rowNumber, 3)));

            if (districtCell is not null
                && !string.Equals(districtCell, "District", StringComparison.OrdinalIgnoreCase)
                && !districtCell.Contains("Table-", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(districtCell, "Total", StringComparison.OrdinalIgnoreCase))
            {
                currentDistrict = districtCell;
            }

            if (currentDistrict is null || dsCell is null)
            {
                continue;
            }

            if (string.Equals(dsCell, "DS Division", StringComparison.OrdinalIgnoreCase)
                || string.Equals(dsCell, "Total", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!int.TryParse(gnCountText, out var gnCount))
            {
                continue;
            }

            results.Add(new OfficialCountExpectation(
                DistrictEnglish: currentDistrict,
                DsEnglish: dsCell,
                GnCount: gnCount));
        }

        return results;
    }
}
