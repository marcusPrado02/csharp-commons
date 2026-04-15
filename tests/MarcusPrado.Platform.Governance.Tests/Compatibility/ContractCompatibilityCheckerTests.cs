namespace MarcusPrado.Platform.Governance.Tests.Compatibility;

public sealed class ContractCompatibilityCheckerTests
{
    [Fact]
    public void Check_ReturnsCompatible_WhenSchemasAreIdentical()
    {
        const string schema = """{"orderId":"String","amount":"Number"}""";

        var report = ContractCompatibilityChecker.Check(schema, schema);

        report.IsCompatible.Should().BeTrue();
        report.Violations.Should().BeEmpty();
    }

    [Fact]
    public void Check_DetectsFieldRemoval()
    {
        const string previous = """{"orderId":"String","amount":"Number"}""";
        const string current = """{"orderId":"String"}""";

        var report = ContractCompatibilityChecker.Check(previous, current);

        report.IsCompatible.Should().BeFalse();
        report.Violations.Should().ContainSingle(v =>
            v.Type == ViolationType.FieldRemoved && v.FieldPath == "amount");
    }

    [Fact]
    public void Check_DetectsTypeChange()
    {
        const string previous = """{"amount":"Number"}""";
        const string current = """{"amount":"String"}""";

        var report = ContractCompatibilityChecker.Check(previous, current);

        report.IsCompatible.Should().BeFalse();
        report.Violations.Should().ContainSingle(v =>
            v.Type == ViolationType.TypeChanged && v.FieldPath == "amount");
    }

    [Fact]
    public void Check_AllowsAddingNewFields()
    {
        const string previous = """{"orderId":"String"}""";
        const string current = """{"orderId":"String","newField":"String"}""";

        var report = ContractCompatibilityChecker.Check(previous, current);

        report.IsCompatible.Should().BeTrue();
    }
}
