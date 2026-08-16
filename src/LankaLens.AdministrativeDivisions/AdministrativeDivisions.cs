using LankaLens.AdministrativeDivisions.Internal;

namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// Entry point for Sri Lanka administrative division data.
/// Use <see cref="Default"/> for the process-wide provider backed by the bundled dataset.
/// </summary>
public static class AdministrativeDivisions
{
    private static readonly Lazy<IAdministrativeDivisionProvider> DefaultProvider =
        new(
            static () => new AdministrativeDivisionProvider(EmbeddedAdministrativeDivisionLoader.Load()),
            LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Shared read-only provider for the bundled production administrative dataset.
    /// The same instance is returned for the lifetime of the process.
    /// Loaded once from the embedded assembly resource on first access.
    /// </summary>
    /// <remarks>
    /// Throws <see cref="InvalidOperationException"/> if the embedded package data cannot be loaded
    /// or violates required runtime invariants. This indicates a broken package, not normal user input.
    /// </remarks>
    public static IAdministrativeDivisionProvider Default => DefaultProvider.Value;
}
