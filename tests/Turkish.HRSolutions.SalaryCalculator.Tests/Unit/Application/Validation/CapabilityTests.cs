using Turkish.HRSolutions.SalaryCalculator.Application.Validation;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Application.Validation;

/// <summary>
/// Tests for <see cref="Capability"/> flags enum and <see cref="CapabilityInfo"/>.
/// </summary>
public sealed class CapabilityTests
{
    [Test]
    public async Task Capability_None_Should_BeZero()
    {
        var none = GetCapability(Capability.None);
        await Assert.That((int)none).IsEqualTo(0);
    }

    private static Capability GetCapability(Capability c) => c;

    [Test]
    public async Task Capability_Should_SupportCombiningFlags()
    {
        const Capability combined = Capability.AgiSelection | Capability.EducationType;

        await Assert.That(combined.HasFlag(Capability.AgiSelection)).IsTrue();
        await Assert.That(combined.HasFlag(Capability.EducationType)).IsTrue();
        await Assert.That(combined.HasFlag(Capability.MinWageExemption)).IsFalse();
    }

    [Test]
    public async Task Capability_All_Should_CombineAllCapabilities()
    {
        await Assert.That(Capability.All.HasFlag(Capability.AgiSelection)).IsTrue();
        await Assert.That(Capability.All.HasFlag(Capability.EducationType)).IsTrue();
        await Assert.That(Capability.All.HasFlag(Capability.MinWageExemption)).IsTrue();
        await Assert.That(Capability.All.HasFlag(Capability.AgiCalculation)).IsTrue();
        await Assert.That(Capability.All.HasFlag(Capability.Discount5746)).IsTrue();
        await Assert.That(Capability.All.HasFlag(Capability.RnDDaysInput)).IsTrue();
        await Assert.That(Capability.All.HasFlag(Capability.AgiIncludedInNet)).IsTrue();
    }

    [Test]
    public async Task CapabilityInfo_IsAvailable_Should_ReturnTrue_When_CapabilityNotDisabled()
    {
        var info = new CapabilityInfo(Capability.AgiSelection); // Only AGI disabled

        await Assert.That(info.IsAvailable(Capability.EducationType)).IsTrue();
        await Assert.That(info.IsAvailable(Capability.MinWageExemption)).IsTrue();
    }

    [Test]
    public async Task CapabilityInfo_IsAvailable_Should_ReturnFalse_When_CapabilityIsDisabled()
    {
        var info = new CapabilityInfo(Capability.AgiSelection | Capability.EducationType);

        await Assert.That(info.IsAvailable(Capability.AgiSelection)).IsFalse();
        await Assert.That(info.IsAvailable(Capability.EducationType)).IsFalse();
    }

    [Test]
    public async Task CapabilityInfo_DisabledCapabilities_Should_ReturnSetFlags()
    {
        const Capability disabled = Capability.AgiSelection | Capability.Discount5746;
        var info = new CapabilityInfo(disabled);

        await Assert.That(info.DisabledCapabilities).IsEqualTo(disabled);
    }

    [Test]
    public async Task CapabilityInfo_AvailableCapabilities_Should_ReturnInverseOfDisabled()
    {
        var info = new CapabilityInfo(Capability.AgiSelection);

        await Assert.That(info.AvailableCapabilities.HasFlag(Capability.AgiSelection)).IsFalse();
        await Assert.That(info.AvailableCapabilities.HasFlag(Capability.EducationType)).IsTrue();
        await Assert.That(info.AvailableCapabilities.HasFlag(Capability.MinWageExemption)).IsTrue();
    }

    [Test]
    public async Task CapabilityInfo_WithNoneDisabled_Should_HaveAllAvailable()
    {
        var info = new CapabilityInfo(Capability.None);

        await Assert.That(info.AvailableCapabilities).IsEqualTo(Capability.All);
    }

    [Test]
    public async Task CapabilityInfo_WithAllDisabled_Should_HaveNoneAvailable()
    {
        var info = new CapabilityInfo(Capability.All);

        await Assert.That(info.AvailableCapabilities).IsEqualTo(Capability.None);
    }
}
