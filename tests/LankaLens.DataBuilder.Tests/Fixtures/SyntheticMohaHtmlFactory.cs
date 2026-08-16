namespace LankaLens.DataBuilder.Tests.Fixtures;

internal static class SyntheticMohaHtmlFactory
{
    public static string GnReport(params SyntheticMohaGnRow[] rows)
    {
        var body = string.Join(
            Environment.NewLine,
            rows.Select(r => $"""
                <tr>
                  <td><b>{r.LifeCode}</b></td>
                  <td><b>{r.GnComponent}</b></td>
                  <td>{r.Sinhala}</td>
                  <td>{r.Tamil}</td>
                  <td>{r.English}</td>
                  <td>{r.MpaCode}</td>
                  <td>{r.ProvinceLabel}</td>
                  <td>{r.DistrictLabel}</td>
                  <td>{r.DsLabel}</td>
                </tr>
                """));

        return $"""
            <table class="table table-bordered table-striped">
              <thead>
                <tr></tr>
                <tr>
                  <th>LIFe Code</th>
                  <th>GN Code</th>
                  <th>Name in Sinhala</th>
                  <th>Name in Tamil</th>
                  <th>Name in English</th>
                  <th>MPA Code</th>
                  <th>Province</th>
                  <th>District</th>
                  <th>Divisional Secretariat</th>
                </tr>
              </thead>
              <tbody>
                {body}
              </tbody>
            </table>
            """;
    }

    public static SyntheticMohaGnRow Sammanthranapura() => new(
        LifeCode: "1-1-03-005",
        GnComponent: "005",
        Sinhala: "සම්මන්ත්‍රණපුර",
        Tamil: "சம்மந்திரணபுர",
        English: "Sammanthranapura",
        MpaCode: "",
        ProvinceLabel: "1: බස්නාහිර/ மேற்கு/ Western",
        DistrictLabel: "1: කොළඹ/ கொழும்பு/ Colombo",
        DsLabel: "3: කොළඹ/ கொழும்பு/ Colombo");

    public static SyntheticMohaGnRow Mattakkuliya() => new(
        LifeCode: "1-1-03-010",
        GnComponent: "010",
        Sinhala: "මට්ටක්කුලිය",
        Tamil: "மட்டக்குளி",
        English: "Mattakkuliya",
        MpaCode: "C26",
        ProvinceLabel: "1: බස්නාහිර/ மேற்கு/ Western",
        DistrictLabel: "1: කොළඹ/ கொழும்பு/ Colombo",
        DsLabel: "3: කොළඹ/ கொழும்பு/ Colombo");
}

internal sealed record SyntheticMohaGnRow(
    string LifeCode,
    string GnComponent,
    string Sinhala,
    string Tamil,
    string English,
    string MpaCode,
    string ProvinceLabel,
    string DistrictLabel,
    string DsLabel);
