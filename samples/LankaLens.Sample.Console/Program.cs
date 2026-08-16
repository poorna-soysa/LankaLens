using LankaLens.AdministrativeDivisions;

var sriLanka = AdministrativeDivisions.Default;

Console.WriteLine($"Provinces: {sriLanka.GetProvinces().Count}");
Console.WriteLine($"Districts: {sriLanka.GetDistricts().Count}");
Console.WriteLine($"Dataset: {sriLanka.DatasetMetadata.SourceName}");
Console.WriteLine();

var western = sriLanka.GetProvinceByCode("1");
if (western is not null)
{
    Console.WriteLine($"Province [1] {western.Name.English}");
    Console.WriteLine($"  Sinhala: {western.Name.Sinhala ?? "(not available)"}");
    Console.WriteLine($"  Tamil:   {western.Name.Tamil ?? "(not available)"}");

    var districts = sriLanka.GetDistrictsByProvince(western.Code);
    Console.WriteLine($"  Districts under Western: {districts.Count}");
}

Console.WriteLine();
var results = sriLanka.Search(
    "Colombo",
    new AdministrativeDivisionSearchOptions
    {
        Language = Language.English,
        MaxResults = 5
    });

Console.WriteLine("Search 'Colombo' (English):");
foreach (var hit in results)
{
    Console.WriteLine($"  [{hit.Type}] {hit.Code} — {hit.Name.English}");
}
