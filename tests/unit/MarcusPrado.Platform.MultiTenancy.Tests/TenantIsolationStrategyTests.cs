using MarcusPrado.Platform.MultiTenancy.Strategy;

namespace MarcusPrado.Platform.MultiTenancy.Tests;

public sealed class TenantIsolationStrategyTests
{
    [Fact]
    public void SchemaPerTenantStrategy_Name_IsSchemaPerTenant()
    {
        new SchemaPerTenantStrategy().StrategyName.Should().Be("SchemaPerTenant");
    }

    [Fact]
    public void SchemaPerTenantStrategy_GetSchemaName_LowercasesAndReplacesHyphens()
    {
        var schema = SchemaPerTenantStrategy.GetSchemaName("Tenant-ABC");
        schema.Should().Be("tenant_abc");
    }

    [Fact]
    public void SchemaPerTenantStrategy_BuildSearchPath_ContainsSchema()
    {
        var sql = SchemaPerTenantStrategy.BuildSearchPathSql("tenant-1");
        sql.Should().Contain("tenant_1").And.Contain("SET search_path");
    }

    [Fact]
    public void DatabasePerTenantStrategy_Name_IsDatabasePerTenant()
    {
        new DatabasePerTenantStrategy().StrategyName.Should().Be("DatabasePerTenant");
    }

    [Fact]
    public void DatabasePerTenantStrategy_RegisterAndGet_Roundtrips()
    {
        var sut = new DatabasePerTenantStrategy();
        sut.RegisterTenant("t1", "Host=localhost;Database=t1");
        sut.GetConnectionString("t1").Should().Contain("t1");
    }

    [Fact]
    public void DatabasePerTenantStrategy_UnknownTenant_ReturnsNull()
    {
        new DatabasePerTenantStrategy().GetConnectionString("unknown").Should().BeNull();
    }

    [Fact]
    public void DiscriminatorStrategy_Name_IsDiscriminator()
    {
        new DiscriminatorStrategy().StrategyName.Should().Be("Discriminator");
    }

    [Fact]
    public void DiscriminatorStrategy_DefaultColumn_IsTenantId()
    {
        new DiscriminatorStrategy().DiscriminatorColumn.Should().Be("TenantId");
    }

    [Fact]
    public void AllStrategies_ImplementInterface()
    {
        ((ITenantIsolationStrategy)new SchemaPerTenantStrategy()).StrategyName.Should().NotBeEmpty();
        ((ITenantIsolationStrategy)new DatabasePerTenantStrategy()).StrategyName.Should().NotBeEmpty();
        ((ITenantIsolationStrategy)new DiscriminatorStrategy()).StrategyName.Should().NotBeEmpty();
    }
}
