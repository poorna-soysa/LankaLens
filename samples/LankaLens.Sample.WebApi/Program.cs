using LankaLens.AdministrativeDivisions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var sriLanka = AdministrativeDivisions.Default;

app.MapGet("/", () => Results.Ok(new
{
    message = "LankaLens.Sample.WebApi",
    provinces = sriLanka.GetProvinces().Count,
    districts = sriLanka.GetDistricts().Count,
    divisionalSecretariats = sriLanka.GetDivisionalSecretariats().Count,
    gramaNiladhariDivisions = sriLanka.GetGramaNiladhariDivisions().Count
}));

app.MapGet("/provinces", () =>
    sriLanka.GetProvinces().Select(p => new
    {
        p.Code,
        english = p.Name.English,
        sinhala = p.Name.Sinhala,
        tamil = p.Name.Tamil
    }));

app.MapGet("/districts/{code}", (string code) =>
{
    var district = sriLanka.GetDistrictByCode(code);
    return district is null
        ? Results.NotFound()
        : Results.Ok(new
        {
            district.Code,
            district.ProvinceCode,
            english = district.Name.English,
            sinhala = district.Name.Sinhala,
            tamil = district.Name.Tamil
        });
});

app.MapGet("/search", (string q, Language? language, int? maxResults) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.BadRequest(new { error = "Query parameter 'q' is required." });
    }

    var results = sriLanka.Search(
        q,
        new AdministrativeDivisionSearchOptions
        {
            Language = language,
            MaxResults = maxResults
        });

    return Results.Ok(results.Select(r => new
    {
        r.Code,
        type = r.Type.ToString(),
        english = r.Name.English,
        sinhala = r.Name.Sinhala,
        tamil = r.Name.Tamil,
        r.ProvinceCode,
        r.DistrictCode,
        r.DivisionalSecretariatCode
    }));
});

app.Run();
