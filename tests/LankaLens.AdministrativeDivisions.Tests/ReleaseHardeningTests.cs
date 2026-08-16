using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;

namespace LankaLens.AdministrativeDivisions.Tests;

public sealed class ImmutabilityTests
{
    private readonly IAdministrativeDivisionProvider _provider = AdministrativeDivisions.Default;

    [Fact]
    public void GetProvinces_Is_ReadOnly_And_Rejects_Mutation()
    {
        AssertReadOnly(_provider.GetProvinces());
    }

    [Fact]
    public void GetDistricts_Is_ReadOnly_And_Rejects_Mutation()
    {
        AssertReadOnly(_provider.GetDistricts());
    }

    [Fact]
    public void GetDivisionalSecretariats_Is_ReadOnly_And_Rejects_Mutation()
    {
        AssertReadOnly(_provider.GetDivisionalSecretariats());
    }

    [Fact]
    public void GetGramaNiladhariDivisions_Is_ReadOnly_And_Rejects_Mutation()
    {
        AssertReadOnly(_provider.GetGramaNiladhariDivisions());
    }

    [Fact]
    public void Hierarchy_Filters_Are_ReadOnly()
    {
        AssertReadOnly(_provider.GetDistrictsByProvince("1"));
        AssertReadOnly(_provider.GetDivisionalSecretariatsByDistrict("11"));
        AssertReadOnly(_provider.GetGramaNiladhariDivisionsByDivisionalSecretariat("1103"));
    }

    [Fact]
    public void Search_Results_Are_ReadOnly()
    {
        AssertReadOnly(_provider.Search("Colombo", new AdministrativeDivisionSearchOptions { MaxResults = 5 }));
    }

    [Fact]
    public void Returned_Collections_Are_Not_Mutable_Lists_Or_Arrays()
    {
        var provinces = _provider.GetProvinces();
        Assert.IsNotType<List<Province>>(provinces);
        Assert.IsNotType<Province[]>(provinces);
        Assert.IsAssignableFrom<ReadOnlyCollection<Province>>(provinces);
    }

    private static void AssertReadOnly<T>(IReadOnlyList<T> items)
    {
        Assert.NotNull(items);
        var list = Assert.IsAssignableFrom<IList<T>>(items);
        Assert.True(list.IsReadOnly);
        Assert.ThrowsAny<NotSupportedException>(() => list.Clear());
        if (items.Count > 0)
        {
            Assert.ThrowsAny<NotSupportedException>(() => list[0] = default!);
        }
    }
}

public sealed class ConcurrencyTests
{
    [Fact]
    public void Parallel_Lookups_Hierarchy_And_Search_Are_Stable()
    {
        var provider = AdministrativeDivisions.Default;
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        Parallel.For(0, 64, i =>
        {
            try
            {
                Assert.Equal(9, provider.GetProvinces().Count);
                Assert.Equal(25, provider.GetDistricts().Count);
                Assert.NotNull(provider.GetProvinceByCode("1"));
                Assert.NotNull(provider.GetDistrictByCode("11"));
                Assert.NotNull(provider.GetDivisionalSecretariatByCode("1103"));
                Assert.NotNull(provider.GetGramaNiladhariDivisionByCode("1103005"));
                Assert.NotEmpty(provider.GetDistrictsByProvince("1"));
                Assert.Contains(
                    provider.Search("Colombo", new AdministrativeDivisionSearchOptions { MaxResults = 10 }),
                    r => r.Code == "11");
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.True(exceptions.IsEmpty, string.Join(Environment.NewLine, exceptions.Select(e => e.ToString())));
        Assert.Same(provider, AdministrativeDivisions.Default);
    }
}

public sealed class CultureIndependenceTests
{
    [Theory]
    [InlineData("en-US")]
    [InlineData("si-LK")]
    [InlineData("ta-LK")]
    [InlineData("tr-TR")]
    public void Lookup_And_Search_Are_Culture_Independent(string cultureName)
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            var provider = AdministrativeDivisions.Default;
            Assert.Equal("Western", provider.GetProvinceByCode("1")!.Name.English);
            Assert.Equal("Colombo", provider.GetDistrictByCode("11")!.Name.English);

            var results = provider.Search(
                "Colombo",
                new AdministrativeDivisionSearchOptions
                {
                    Language = Language.English,
                    Type = AdministrativeDivisionType.District,
                    MaxResults = 5
                });
            Assert.Contains(results, r => r.Code == "11");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }
}

public sealed class UnicodeIntegrityTests
{
    private readonly IAdministrativeDivisionProvider _provider = AdministrativeDivisions.Default;

    [Fact]
    public void Representative_Sinhala_And_Tamil_Survive_Runtime_Load()
    {
        var western = _provider.GetProvinceByCode("1");
        Assert.NotNull(western);
        Assert.Equal("බස්නාහිර", western.Name.Sinhala);
        Assert.Equal("மேற்கு", western.Name.Tamil);

        var colombo = _provider.GetDistrictByCode("11");
        Assert.NotNull(colombo);
        Assert.Equal("කොළඹ", colombo.Name.Sinhala);
        Assert.False(string.IsNullOrWhiteSpace(colombo.Name.Tamil));
        Assert.DoesNotContain('\uFFFD', colombo.Name.Tamil!);

        var jaffna = _provider.GetDistrictByCode("41");
        Assert.NotNull(jaffna);
        Assert.False(string.IsNullOrWhiteSpace(jaffna.Name.Sinhala));
        Assert.False(string.IsNullOrWhiteSpace(jaffna.Name.Tamil));
        Assert.DoesNotContain('\uFFFD', jaffna.Name.Sinhala!);
        Assert.DoesNotContain('\uFFFD', jaffna.Name.Tamil!);
    }

    [Fact]
    public void Embedded_Dataset_Contains_No_Unicode_Replacement_Characters()
    {
        foreach (var province in _provider.GetProvinces())
        {
            AssertNoReplacement(province.Name.English);
            AssertNoReplacement(province.Name.Sinhala);
            AssertNoReplacement(province.Name.Tamil);
        }

        foreach (var district in _provider.GetDistricts())
        {
            AssertNoReplacement(district.Name.English);
            AssertNoReplacement(district.Name.Sinhala);
            AssertNoReplacement(district.Name.Tamil);
        }
    }

    [Fact]
    public void Canonical_Dataset_Sha256_Matches_Snapshot_Expectation()
    {
        var expectationsPath = FindRepoFile(Path.Combine("data", "source", "snapshot-expectations.json"));
        using var expectationsDoc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(expectationsPath));
        var expected = expectationsDoc.RootElement.GetProperty("canonicalDatasetSha256").GetString();
        Assert.False(string.IsNullOrWhiteSpace(expected));

        var datasetPath = FindRepoFile(Path.Combine(
            "src",
            "LankaLens.AdministrativeDivisions",
            "Data",
            "administrative-divisions.json"));
        var bytes = File.ReadAllBytes(datasetPath);
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        Assert.Equal(expected, hash);

        using var stream = typeof(AdministrativeDivisions).Assembly.GetManifestResourceStream(
            Internal.EmbeddedAdministrativeDivisionLoader.ResourceName);
        Assert.NotNull(stream);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var embeddedHash = Convert.ToHexString(SHA256.HashData(ms.ToArray())).ToLowerInvariant();
        Assert.Equal(expected, embeddedHash);
    }

    private static void AssertNoReplacement(string? value)
    {
        if (value is null)
        {
            return;
        }

        Assert.DoesNotContain('\uFFFD', value);
    }

    private static string FindRepoFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException($"Could not locate '{relativePath}' from '{AppContext.BaseDirectory}'.");
    }
}
