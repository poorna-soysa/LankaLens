using HtmlAgilityPack;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;

namespace LankaLens.DataBuilder.Parsing;

/// <summary>
/// Parses official MOHA LIFe GN report HTML tables (rpt_gn_list.php).
/// Expected headers: LIFe Code, GN Code, Name in Sinhala, Name in Tamil,
/// Name in English, MPA Code, Province, District, Divisional Secretariat.
/// </summary>
internal sealed class MohaGnReportParser
{
    public const string SourceId = "moha-life-location-codes";

    public MohaParseResult ParseFile(string filePath)
    {
        var html = File.ReadAllText(filePath);
        return ParseHtml(html, Path.GetFileName(filePath));
    }

    public MohaParseResult ParseDirectory(string reportsDirectory)
    {
        if (!Directory.Exists(reportsDirectory))
        {
            return new MohaParseResult([], []);
        }

        var records = new List<RawMohaGnRecord>();
        var invalid = new List<InvalidLifeCode>();
        foreach (var path in Directory.GetFiles(reportsDirectory, "*.html").OrderBy(p => p, StringComparer.Ordinal))
        {
            var parsed = ParseFile(path);
            records.AddRange(parsed.Records);
            invalid.AddRange(parsed.InvalidCodes);
        }

        return new MohaParseResult(records, invalid);
    }

    public MohaParseResult ParseHtml(string html, string? sourceReportFile = null)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var records = new List<RawMohaGnRecord>();
        var invalid = new List<InvalidLifeCode>();
        var tables = document.DocumentNode.SelectNodes("//table") ?? Enumerable.Empty<HtmlNode>();
        var rowNumber = 0;

        foreach (var table in tables)
        {
            var rows = table.SelectNodes(".//tr") ?? Enumerable.Empty<HtmlNode>();
            foreach (var row in rows)
            {
                var cells = row.SelectNodes("./th|./td");
                if (cells is null || cells.Count < 9)
                {
                    continue;
                }

                var texts = cells.Take(9).Select(ReadCell).ToArray();
                if (IsHeaderRow(texts))
                {
                    continue;
                }

                rowNumber++;
                var lifeRaw = texts[0];
                if (!LifeCodeParser.TryParse(lifeRaw, out var parsed, out var error) || parsed is null)
                {
                    invalid.Add(new InvalidLifeCode(
                        lifeRaw ?? string.Empty,
                        error ?? "Invalid LIFe code.",
                        sourceReportFile,
                        rowNumber));
                    continue;
                }

                var province = MohaHierarchyLabelParser.Parse(texts[6]);
                var district = MohaHierarchyLabelParser.Parse(texts[7]);
                var ds = MohaHierarchyLabelParser.Parse(texts[8]);

                records.Add(new RawMohaGnRecord
                {
                    LifeCode = parsed.LifeCode,
                    NormalizedLifeCode = parsed.NormalizedLifeCode,
                    ProvinceComponent = parsed.ProvinceComponent,
                    DistrictComponent = parsed.DistrictComponent,
                    DsComponent = parsed.DsComponent,
                    GnComponent = parsed.GnComponent,
                    HierarchicalProvinceCode = parsed.HierarchicalProvinceCode,
                    HierarchicalDistrictCode = parsed.HierarchicalDistrictCode,
                    HierarchicalDsCode = parsed.HierarchicalDsCode,
                    SinhalaName = MohaNameNormalizer.Normalize(texts[2]),
                    TamilName = MohaNameNormalizer.Normalize(texts[3]),
                    EnglishName = MohaNameNormalizer.Normalize(texts[4]),
                    MpaCode = MohaNameNormalizer.Normalize(texts[5]),
                    ProvinceEnglish = province.English,
                    ProvinceSinhala = province.Sinhala,
                    ProvinceTamil = province.Tamil,
                    DistrictEnglish = district.English,
                    DistrictSinhala = district.Sinhala,
                    DistrictTamil = district.Tamil,
                    DsEnglish = ds.English,
                    DsSinhala = ds.Sinhala,
                    DsTamil = ds.Tamil,
                    ProvinceLabelPrefix = province.NumericPrefix,
                    DistrictLabelPrefix = district.NumericPrefix,
                    DsLabelPrefix = ds.NumericPrefix,
                    SourceReportFile = sourceReportFile,
                    SourceRowNumber = rowNumber
                });
            }
        }

        return new MohaParseResult(records, invalid);
    }

    private static bool IsHeaderRow(IReadOnlyList<string?> texts)
    {
        var first = texts[0] ?? string.Empty;
        return first.Contains("LIFe", StringComparison.OrdinalIgnoreCase)
            || first.Contains("Life Code", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ReadCell(HtmlNode node)
    {
        var text = HtmlEntity.DeEntitize(node.InnerText);
        return text;
    }
}
