using ClosedXML.Excel;

namespace LankaLens.DataBuilder.Parsing;

/// <summary>
/// Produces a concise human-readable inspection of an Excel workbook.
/// </summary>
internal sealed class WorkbookInspector
{
    public string Inspect(string filePath, int sampleRows = 5)
    {
        using var workbook = new XLWorkbook(filePath);
        var writer = new StringWriter();
        writer.WriteLine($"Workbook: {Path.GetFileName(filePath)}");
        writer.WriteLine($"Sheets: {workbook.Worksheets.Count}");
        writer.WriteLine();

        foreach (var worksheet in workbook.Worksheets)
        {
            var used = worksheet.RangeUsed();
            writer.WriteLine($"Sheet: {worksheet.Name}");
            if (used is null)
            {
                writer.WriteLine("  (empty)");
                writer.WriteLine();
                continue;
            }

            writer.WriteLine($"  used rows: {used.RowCount()}");
            writer.WriteLine($"  used columns: {used.ColumnCount()}");

            var headers = new List<string>();
            for (var col = 1; col <= used.ColumnCount(); col++)
            {
                headers.Add(worksheet.Cell(1, col).GetFormattedString()?.Trim() ?? string.Empty);
            }

            writer.WriteLine($"  headers: {string.Join(" | ", headers)}");

            var codeLike = headers
                .Select((h, i) => (Header: h, Index: i + 1))
                .Where(x => x.Header.Contains("code", StringComparison.OrdinalIgnoreCase)
                    || x.Header.Contains("UID", StringComparison.OrdinalIgnoreCase)
                    || x.Header.Contains("name", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (codeLike.Count > 0)
            {
                writer.WriteLine("  detected code/name columns:");
                foreach (var item in codeLike)
                {
                    writer.WriteLine($"    - {item.Header}");
                }
            }

            writer.WriteLine("  sample rows:");
            var maxSample = Math.Min(sampleRows, Math.Max(0, used.RowCount() - 1));
            for (var offset = 0; offset < maxSample; offset++)
            {
                var rowNumber = 2 + offset;
                var cells = new List<string>();
                for (var col = 1; col <= Math.Min(used.ColumnCount(), 13); col++)
                {
                    cells.Add(GndListWorkbookParser.ReadCellAsString(worksheet.Cell(rowNumber, col)) ?? string.Empty);
                }

                writer.WriteLine($"    R{rowNumber}: {string.Join(" | ", cells)}");
            }

            var problems = new List<string>();
            if (headers.Any(string.IsNullOrWhiteSpace))
            {
                problems.Add("blank header cell(s)");
            }

            if (headers.Any(h => h.Contains("Sinhala", StringComparison.OrdinalIgnoreCase)))
            {
                writer.WriteLine("  Sinhala column header detected");
            }
            else if (headers.Any(h => h.Contains("Name", StringComparison.OrdinalIgnoreCase)))
            {
                problems.Add("no Sinhala name column detected in headers");
            }

            if (!headers.Any(h => h.Contains("Tamil", StringComparison.OrdinalIgnoreCase))
                && headers.Any(h => h.Contains("Name", StringComparison.OrdinalIgnoreCase)))
            {
                problems.Add("no Tamil name column detected in headers");
            }

            if (problems.Count > 0)
            {
                writer.WriteLine("  potential problems:");
                foreach (var problem in problems)
                {
                    writer.WriteLine($"    - {problem}");
                }
            }

            writer.WriteLine();
        }

        return writer.ToString();
    }
}
