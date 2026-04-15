using MarcusPrado.Platform.Security.Pii;
using Serilog.Core;
using Serilog.Events;

namespace MarcusPrado.Platform.Security.Tests;

internal sealed class CustomerDto
{
    public string Name { get; set; } = "";

    [PiiData(Type = PiiType.Email)]
    public string Email { get; set; } = "";

    [PiiData(Type = PiiType.Phone)]
    public string Phone { get; set; } = "";
    public int Age { get; set; }
}

public sealed class PiiTests
{
    // 1. MaskEmail happy path
    [Fact]
    public void MaskEmail_ShouldMaskLocalPart_WhenValidEmail()
    {
        var result = PiiRedactor.MaskEmail("john.doe@example.com");
        result.Should().StartWith("jo***@");
        result.Should().Contain("example.com");
    }

    // 2. MaskEmail handles empty string
    [Fact]
    public void MaskEmail_ShouldReturnStars_WhenEmpty()
    {
        PiiRedactor.MaskEmail("").Should().Be("***");
    }

    // 3. MaskPhone
    [Fact]
    public void MaskPhone_ShouldKeepLastFourDigits()
    {
        var result = PiiRedactor.MaskPhone("(11) 91234-5678");
        result.Should().Be("****-5678");
    }

    // 4. MaskCpf
    [Fact]
    public void MaskCpf_ShouldHideFirstAndLastDigits()
    {
        var result = PiiRedactor.MaskCpf("123.456.789-00");
        result.Should().Contain("***");
        result.Should().NotContain("123");
        result.Should().NotContain("-00");
    }

    // 5. Mask generic
    [Fact]
    public void Mask_ShouldStartWithFirstTwoCharsAndContainAsterisks()
    {
        var result = PiiRedactor.Mask("HelloWorld");
        result.Should().StartWith("He");
        result.Should().Contain("*");
    }

    // 6. PiiClassifier.GetPiiProperties
    [Fact]
    public void GetPiiProperties_ShouldReturnAnnotatedProperties()
    {
        var props = PiiClassifier.GetPiiProperties(typeof(CustomerDto));
        props.Should().ContainKey("Email");
        props["Email"].Should().Be(PiiType.Email);
        props.Should().ContainKey("Phone");
        props["Phone"].Should().Be(PiiType.Phone);
        props.Should().NotContainKey("Name");
        props.Should().NotContainKey("Age");
    }

    // 7. PiiClassifier.Redact
    [Fact]
    public void Redact_ShouldMaskPiiFields_AndPreserveNonPii()
    {
        var dto = new CustomerDto
        {
            Name = "Alice",
            Email = "alice@example.com",
            Phone = "(21) 99887-6543",
            Age = 30,
        };

        var result = PiiClassifier.Redact(dto);

        result["Name"].Should().Be("Alice");
        result["Age"].Should().Be(30);

        var maskedEmail = result["Email"] as string;
        maskedEmail.Should().NotBe("alice@example.com");
        maskedEmail.Should().Contain("@");

        var maskedPhone = result["Phone"] as string;
        maskedPhone.Should().NotBe("(21) 99887-6543");
        maskedPhone.Should().StartWith("****-");
    }

    // 8. GdprComplianceReport.Scan
    [Fact]
    public void GdprComplianceReport_Scan_ShouldFindPiiAnnotatedTypes()
    {
        var report = GdprComplianceReport.Scan(typeof(CustomerDto).Assembly);
        report.Should().Contain(r => r.PropertyName == "Email" && r.PiiType == PiiType.Email);
        report.Should().Contain(r => r.PropertyName == "Phone" && r.PiiType == PiiType.Phone);
    }

    // 9. SerilogPiiDestructuringPolicy
    [Fact]
    public void SerilogPiiDestructuringPolicy_ShouldReturnStructureValue_WhenTypeHasPii()
    {
        var policy = new SerilogPiiDestructuringPolicy();
        var dto = new CustomerDto
        {
            Name = "Bob",
            Email = "bob@test.com",
            Phone = "11912345678",
            Age = 25,
        };

        var factory = new ScalarValueFactory();
        var success = policy.TryDestructure(dto, factory, out var logValue);

        success.Should().BeTrue();
        logValue.Should().BeOfType<StructureValue>();
    }
}

// Minimal ILogEventPropertyValueFactory for testing
internal sealed class ScalarValueFactory : ILogEventPropertyValueFactory
{
    public LogEventPropertyValue CreatePropertyValue(object? value, bool destructureObjects = false) =>
        new ScalarValue(value);
}
