<#
.SYNOPSIS
  Smoke-tests the locally packed LankaLens.AdministrativeDivisions NuGet package
  from a temporary console app (PackageReference only — no ProjectReference).
#>
param(
  [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
  [string]$PackageVersion = "0.1.0-preview.1"
)

$ErrorActionPreference = "Stop"

$packagesDir = Join-Path $RepoRoot "artifacts/packages"
$nupkg = Join-Path $packagesDir "LankaLens.AdministrativeDivisions.$PackageVersion.nupkg"
if (-not (Test-Path $nupkg)) {
  throw "Package not found: $nupkg. Run 'dotnet pack' first."
}

$work = Join-Path ([System.IO.Path]::GetTempPath()) ("lankalens-smoke-" + [guid]::NewGuid().ToString("n"))
New-Item -ItemType Directory -Path $work | Out-Null

try {
  $nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-lankalens" value="$packagesDir" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@
  Set-Content -Path (Join-Path $work "nuget.config") -Value $nugetConfig -Encoding UTF8

  Push-Location $work
  dotnet new console -n SmokeConsumer --force | Out-Null
  Set-Location (Join-Path $work "SmokeConsumer")

  # Ensure no ProjectReference path exists — package only
  dotnet add package LankaLens.AdministrativeDivisions --version $PackageVersion --source $packagesDir | Out-Null

  $program = @'
using LankaLens.AdministrativeDivisions;

var sriLanka = AdministrativeDivisions.Default;

if (sriLanka.GetProvinces().Count != 9) throw new Exception($"Provinces={sriLanka.GetProvinces().Count}");
if (sriLanka.GetDistricts().Count != 25) throw new Exception($"Districts={sriLanka.GetDistricts().Count}");

var ds = sriLanka.GetDivisionalSecretariatByCode("1103")
    ?? throw new Exception("DS 1103 missing");
var gn = sriLanka.GetGramaNiladhariDivisionByCode("1103005")
    ?? throw new Exception("GN 1103005 missing");

var english = sriLanka.Search("Colombo", new AdministrativeDivisionSearchOptions
{
    Language = Language.English,
    Type = AdministrativeDivisionType.District,
    MaxResults = 5
});
if (!english.Any(r => r.Code == "11")) throw new Exception("English search missed Colombo district");

var western = sriLanka.GetProvinceByCode("1") ?? throw new Exception("Province 1 missing");
if (western.Name.Sinhala is null || western.Name.Tamil is null)
    throw new Exception("Western missing Sinhala/Tamil");

var sinhala = sriLanka.Search(western.Name.Sinhala, new AdministrativeDivisionSearchOptions
{
    Language = Language.Sinhala,
    Type = AdministrativeDivisionType.Province,
    MaxResults = 5
});
if (!sinhala.Any(r => r.Code == "1")) throw new Exception("Sinhala search missed Western");

var tamil = sriLanka.Search(western.Name.Tamil, new AdministrativeDivisionSearchOptions
{
    Language = Language.Tamil,
    Type = AdministrativeDivisionType.Province,
    MaxResults = 5
});
if (!tamil.Any(r => r.Code == "1")) throw new Exception("Tamil search missed Western");

Console.WriteLine("SMOKE_OK");
Console.WriteLine($"Provinces={sriLanka.GetProvinces().Count}");
Console.WriteLine($"Districts={sriLanka.GetDistricts().Count}");
Console.WriteLine($"DS={ds.Code}:{ds.Name.English}");
Console.WriteLine($"GN={gn.Code}:{gn.Name.English}");
Console.WriteLine($"WesternSi={western.Name.Sinhala}");
Console.WriteLine($"WesternTa={western.Name.Tamil}");
'@
  Set-Content -Path "Program.cs" -Value $program -Encoding UTF8

  $output = & dotnet run -c Release --no-restore 2>&1
  if ($LASTEXITCODE -ne 0) {
    # restore may be needed if add package didn't restore fully
    & dotnet restore | Out-Null
    $output = & dotnet run -c Release 2>&1
  }

  $text = ($output | Out-String)
  Write-Host $text
  if ($LASTEXITCODE -ne 0 -or $text -notmatch "SMOKE_OK") {
    throw "Smoke consumer failed."
  }

  Write-Host "Local package consumer smoke test succeeded."
}
finally {
  Pop-Location -ErrorAction SilentlyContinue
  Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}
