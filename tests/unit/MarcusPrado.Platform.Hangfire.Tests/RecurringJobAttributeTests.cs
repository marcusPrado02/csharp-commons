using FluentAssertions;
using MarcusPrado.Platform.Hangfire.Attributes;
using Xunit;

namespace MarcusPrado.Platform.Hangfire.Tests;

public sealed class RecurringJobAttributeTests
{
    [Fact]
    public void Attribute_StoresCronExpression()
    {
        var attr = new RecurringJobAttribute("*/5 * * * *");

        attr.CronExpression.Should().Be("*/5 * * * *");
    }

    [Fact]
    public void Attribute_DefaultQueueIsDefault()
    {
        var attr = new RecurringJobAttribute("0 * * * *");

        attr.Queue.Should().Be("default");
    }

    [Fact]
    public void Attribute_DefaultTimeZoneIsUtc()
    {
        var attr = new RecurringJobAttribute("0 * * * *");

        attr.TimeZoneId.Should().Be("UTC");
    }

    [Fact]
    public void Attribute_CustomQueueIsStored()
    {
        var attr = new RecurringJobAttribute("0 * * * *") { Queue = "critical" };

        attr.Queue.Should().Be("critical");
    }

    [Fact]
    public void Attribute_EmptyCronExpression_ThrowsArgumentException()
    {
        var act = () => new RecurringJobAttribute(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Attribute_WhitespaceCronExpression_ThrowsArgumentException()
    {
        var act = () => new RecurringJobAttribute("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Attribute_TargetOnClass_IsApplicable()
    {
        var usage = typeof(RecurringJobAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        usage.ValidOn.Should().HaveFlag(AttributeTargets.Class);
    }

    [Fact]
    public void Attribute_AllowMultiple_IsFalse()
    {
        var usage = typeof(RecurringJobAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        usage.AllowMultiple.Should().BeFalse();
    }
}
