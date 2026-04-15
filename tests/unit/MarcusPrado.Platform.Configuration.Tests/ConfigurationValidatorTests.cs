using FluentAssertions;
using MarcusPrado.Platform.Configuration;
using Xunit;

namespace MarcusPrado.Platform.Configuration.Tests;

public sealed class ConfigurationValidatorTests
{
    private sealed class MyOptions
    {
        public string? Name { get; set; }
        public int Port { get; set; }
    }

    [Fact]
    public void Validate_WithNoValidators_ShouldNotThrow()
    {
        var validator = new ConfigurationValidator<MyOptions>();
        var options = new MyOptions { Name = "test", Port = 8080 };

        var act = () => validator.Validate(options);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithPassingValidators_ShouldNotThrow()
    {
        var validator = new ConfigurationValidator<MyOptions>().AddValidator(opts =>
        {
            if (opts.Port <= 0)
                throw new InvalidOperationException("Port must be positive");
        });

        var options = new MyOptions { Name = "test", Port = 8080 };

        var act = () => validator.Validate(options);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithFailingValidator_ShouldThrowOptionsValidationException()
    {
        var validator = new ConfigurationValidator<MyOptions>().AddValidator(opts =>
        {
            if (string.IsNullOrEmpty(opts.Name))
                throw new InvalidOperationException("Name is required");
        });

        var options = new MyOptions { Name = null, Port = 8080 };

        var act = () => validator.Validate(options);

        act.Should().Throw<OptionsValidationException>().Which.OptionsTypeName.Should().Be(nameof(MyOptions));
    }

    [Fact]
    public void Validate_WithMultipleValidators_FirstFailureShouldThrow()
    {
        int callCount = 0;
        var validator = new ConfigurationValidator<MyOptions>()
            .AddValidator(_ =>
            {
                callCount++;
                throw new InvalidOperationException("First fails");
            })
            .AddValidator(_ =>
            {
                callCount++;
            });

        var options = new MyOptions { Name = "test", Port = 8080 };

        var act = () => validator.Validate(options);

        act.Should().Throw<OptionsValidationException>();
        callCount.Should().Be(1);
    }

    [Fact]
    public void Validate_WithNullOptions_ShouldThrowArgumentNullException()
    {
        var validator = new ConfigurationValidator<MyOptions>();

        var act = () => validator.Validate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddValidator_ShouldReturnSameInstance_ForChaining()
    {
        var validator = new ConfigurationValidator<MyOptions>();

        var result = validator.AddValidator(_ => { });

        result.Should().BeSameAs(validator);
    }
}
