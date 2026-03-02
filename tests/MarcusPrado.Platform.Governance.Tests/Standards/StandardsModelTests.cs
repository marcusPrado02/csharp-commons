namespace MarcusPrado.Platform.Governance.Tests.Standards;

public sealed class StandardsModelTests
{
    [Fact]
    public void PlatformStandard_EqualityIsValueBased()
    {
        var a = new PlatformStandard("STD-001", "Use HTTPS", "All APIs must use HTTPS.", "Security");
        var b = new PlatformStandard("STD-001", "Use HTTPS", "All APIs must use HTTPS.", "Security");

        a.Should().Be(b);
    }

    [Fact]
    public void PlatformStandard_IsMandatoryByDefault()
    {
        var standard = new PlatformStandard("STD-003", "Tracing", "All services must emit traces.", "Observability");

        standard.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void StandardViolation_RecordsDetails()
    {
        var std = new PlatformStandard("STD-002", "SemVer", "All packages must use SemVer.", "Versioning");
        var now = DateTimeOffset.UtcNow;
        var violation = new StandardViolation(std, "payments-service", "Uses CalVer instead.", now);

        violation.Standard.Should().Be(std);
        violation.Service.Should().Be("payments-service");
        violation.Details.Should().Contain("CalVer");
    }
}
