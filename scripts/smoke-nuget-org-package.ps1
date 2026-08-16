<#
.SYNOPSIS
  Smoke-tests LankaLens.AdministrativeDivisions installed from nuget.org only
  (no ProjectReference, no local package source).
#>
param(
  [string]$PackageVersion = "0.1.0-preview.1"
)

$ErrorActionPreference = "Stop"

$work = Join-Path ([System.IO.Path]::GetTempPath()) ("lankalens-nugetorg-smoke-" + [guid]::NewGuid().ToString("n"))
New-Item -ItemType Directory -Path $work | Out-Null

try {
  $nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@
  Set-Content -Path (Join-Path $work "nuget.config") -Value $nugetConfig -Encoding UTF8

  Push-Location $work
  dotnet new console -n SmokeNugetOrg --force | Out-Null
  Set-Location (Join-Path $work "SmokeNugetOrg")

  Write-Host "Installing LankaLens.AdministrativeDivisions $PackageVersion from nuget.org..."
  dotnet add package LankaLens.AdministrativeDivisions --version $PackageVersion | Out-Null

  $csproj = Get-Content "SmokeNugetOrg.csproj" -Raw
  if ($csproj -match "ProjectReference") {
    throw "Smoke consumer must not use ProjectReference."
  }
  if ($csproj -notmatch "PackageReference.*LankaLens\.AdministrativeDivisions") {
    throw "Expected PackageReference to LankaLens.AdministrativeDivisions."
  }

  $program = @'
using LankaLens.AdministrativeDivisions;

var sriLanka = AdministrativeDivisions.Default
    ?? throw new Exception("AdministrativeDivisions.Default is null");

var provinces = sriLanka.GetProvinces();
var districts = sriLanka.GetDistricts();
var dsAll = sriLanka.GetDivisionalSecretariats();
var gnAll = sriLanka.GetGramaNiladhariDivisions();

if (provinces.Count != 9) throw new Exception($"Provinces={provinces.Count}");
if (districts.Count != 25) throw new Exception($"Districts={districts.Count}");
if (dsAll.Count != 340) throw new Exception($"DS={dsAll.Count}");
if (gnAll.Count != 14008) throw new Exception($"GN={gnAll.Count}");

var western = sriLanka.GetProvinceByCode("1")
    ?? throw new Exception("English lookup failed for province 1");
if (string.IsNullOrWhiteSpace(western.Name.English))
    throw new Exception("Western English name missing");

var english = sriLanka.Search("Colombo", new AdministrativeDivisionSearchOptions
{
    Language = Language.English,
    Type = AdministrativeDivisionType.District,
    MaxResults = 5
});
if (!english.Any(r => r.Code == "11"))
    throw new Exception("English search missed Colombo district");

if (western.Name.Sinhala is null || western.Name.Tamil is null)
    throw new Exception("Western missing Sinhala/Tamil");

var sinhala = sriLanka.Search(western.Name.Sinhala, new AdministrativeDivisionSearchOptions
{
    Language = Language.Sinhala,
    Type = AdministrativeDivisionType.Province,
    MaxResults = 5
});
if (!sinhala.Any(r => r.Code == "1"))
    throw new Exception("Sinhala search missed Western");

var tamil = sriLanka.Search(western.Name.Tamil, new AdministrativeDivisionSearchOptions
{
    Language = Language.Tamil,
    Type = AdministrativeDivisionType.Province,
    MaxResults = 5
});
if (!tamil.Any(r => r.Code == "1"))
    throw new Exception("Tamil search missed Western");

var unresolved = gnAll.Count(g => g.Name.Sinhala is null || g.Name.Tamil is null);
foreach (var gn in gnAll.Where(g => g.Name.Sinhala is null || g.Name.Tamil is null))
{
    if (gn.Name.Sinhala is not null || gn.Name.Tamil is not null)
        throw new Exception($"Unresolved GN {gn.Code} must keep both Sinhala and Tamil null");
}
if (unresolved != 285)
    throw new Exception($"Expected 285 unresolved GN localized pairs, got {unresolved}");

Console.WriteLine("SMOKE_OK");
Console.WriteLine($"Provinces={provinces.Count}");
Console.WriteLine($"Districts={districts.Count}");
Console.WriteLine($"DS={dsAll.Count}");
Console.WriteLine($"GN={gnAll.Count}");
Console.WriteLine($"UnresolvedGnLocalized={unresolved}");
Console.WriteLine("Search=English+Sinhala+Tamil OK");
Console.WriteLine("MultilingualLookup=OK");
'@
  Set-Content -Path "Program.cs" -Value $program -Encoding UTF8

  & dotnet restore | Out-Null
  $output = & dotnet run -c Release 2>&1
  $text = ($output | Out-String)
  Write-Host $text
  if ($LASTEXITCODE -ne 0 -or $text -notmatch "SMOKE_OK") {
    throw "NuGet.org smoke consumer failed."
  }

  Write-Host "NuGet.org package consumer smoke test succeeded."
}
finally {
  Pop-Location -ErrorAction SilentlyContinue
  Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}
