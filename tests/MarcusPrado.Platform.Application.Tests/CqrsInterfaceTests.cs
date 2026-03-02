namespace MarcusPrado.Platform.Application.Tests;

public sealed class CqrsInterfaceTests
{
    [Fact]
    public void ICommandT_ImplementsICommand()
    {
        // ICommand<TResult> extends ICommand
        var cmd = new SimpleCommand();
        Assert.IsAssignableFrom<ICommand>(cmd);
    }

    [Fact]
    public void VoidCommand_ImplementsNonGenericICommand()
    {
        var cmd = new VoidCommand();
        Assert.IsAssignableFrom<ICommand>(cmd);
    }

    [Fact]
    public void IQueryT_IsMarkerInterface()
    {
        var query = new SimpleQuery();
        Assert.IsAssignableFrom<IQuery<string>>(query);
    }

    [Fact]
    public void IdempotentCommand_HasAttribute()
    {
        var attr = typeof(IdempotentCommand)
            .GetCustomAttributes(typeof(IdempotentAttribute), false)
            .OfType<IdempotentAttribute>()
            .SingleOrDefault();

        Assert.NotNull(attr);
        Assert.Equal(60, attr.TimeToLiveSeconds);
    }

    [Fact]
    public void TransactionalCommand_HasAttribute()
    {
        var attr = typeof(TransactionalCommand)
            .GetCustomAttributes(typeof(TransactionalAttribute), false)
            .FirstOrDefault();

        Assert.NotNull(attr);
    }
}
