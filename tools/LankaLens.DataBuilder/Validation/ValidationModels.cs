namespace LankaLens.DataBuilder.Validation;

internal enum ValidationSeverity
{
    Warning,
    Error
}

internal sealed record ValidationIssue(
    ValidationSeverity Severity,
    string Code,
    string Message,
    string? EntityType = null,
    string? EntityCode = null);

internal sealed class ValidationReport
{
    private readonly List<ValidationIssue> _issues = [];

    public IReadOnlyList<ValidationIssue> Issues => _issues;

    public int ErrorCount => _issues.Count(i => i.Severity == ValidationSeverity.Error);

    public int WarningCount => _issues.Count(i => i.Severity == ValidationSeverity.Warning);

    public bool HasErrors => ErrorCount > 0;

    public bool Passed => !HasErrors;

    public int ProvinceCount { get; set; }

    public int DistrictCount { get; set; }

    public int DivisionalSecretariatCount { get; set; }

    public int GramaNiladhariDivisionCount { get; set; }

    public int MissingEnglish { get; set; }

    public int MissingSinhala { get; set; }

    public int MissingTamil { get; set; }

    public int MissingEnglishProvinces { get; set; }

    public int MissingEnglishDistricts { get; set; }

    public int MissingEnglishDivisionalSecretariats { get; set; }

    public int MissingEnglishGramaNiladhariDivisions { get; set; }

    public int MissingSinhalaProvinces { get; set; }

    public int MissingSinhalaDistricts { get; set; }

    public int MissingSinhalaDivisionalSecretariats { get; set; }

    public int MissingSinhalaGramaNiladhariDivisions { get; set; }

    public int MissingTamilProvinces { get; set; }

    public int MissingTamilDistricts { get; set; }

    public int MissingTamilDivisionalSecretariats { get; set; }

    public int MissingTamilGramaNiladhariDivisions { get; set; }

    public IReadOnlyList<string> DatasetSources { get; set; } = [];

    public void Add(ValidationIssue issue) => _issues.Add(issue);

    public void AddRange(IEnumerable<ValidationIssue> issues) => _issues.AddRange(issues);
}
