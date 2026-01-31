namespace Turkish.HRSolutions.SalaryCalculator.Application.Validation;

/// <summary>
/// Contains capability information returned from validation.
/// Used as the value type in <see cref="Common.Results.Result{T}"/>.
/// </summary>
/// <param name="DisabledCapabilities">Capabilities that are disabled for the current configuration.</param>
/// <remarks>
/// <para>
/// Validation returns <c>Result&lt;CapabilityInfo&gt;</c> where:
/// </para>
/// <list type="bullet">
/// <item><description><c>Result.Errors</c> → blocking validation failures</description></item>
/// <item><description><c>Result.Warnings</c> → non-blocking issues (settings will be ignored)</description></item>
/// <item><description><c>Result.Value.DisabledCapabilities</c> → which capabilities are unavailable</description></item>
/// </list>
/// </remarks>
public sealed record CapabilityInfo(Capability DisabledCapabilities)
{
    /// <summary>
    /// Gets a value indicating whether a specific capability is available.
    /// </summary>
    /// <param name="capability">The capability to check.</param>
    /// <returns>True if the capability is available; false if disabled.</returns>
    public bool IsAvailable(Capability capability) => !DisabledCapabilities.HasFlag(capability);

    /// <summary>
    /// Gets a value indicating whether all capabilities are available.
    /// </summary>
    public bool AllCapabilitiesAvailable => DisabledCapabilities == Capability.None;

    /// <summary>
    /// Gets the capabilities that are available (inverse of <see cref="DisabledCapabilities"/>).
    /// </summary>
    public Capability AvailableCapabilities => Capability.All & ~DisabledCapabilities;
}
