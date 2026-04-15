namespace MarcusPrado.Platform.Application.Tests;

public sealed class AppExceptionTests
{
    [Fact]
    public void NotFoundException_HasNotFoundCategory()
    {
        var ex = new NotFoundException("ORDER.NOT_FOUND", "Order not found");
        Assert.Equal(ErrorCategory.NotFound, ex.Error.Category);
        Assert.Equal("ORDER.NOT_FOUND", ex.Error.Code);
        Assert.Equal("Order not found", ex.Message);
    }

    [Fact]
    public void ConflictException_HasConflictCategory()
    {
        var ex = new ConflictException("ORDER.CONFLICT", "Order already exists");
        Assert.Equal(ErrorCategory.Conflict, ex.Error.Category);
    }

    [Fact]
    public void UnauthorizedException_HasUnauthorizedCategory()
    {
        var ex = new UnauthorizedException("AUTH.REQUIRED", "Not authenticated");
        Assert.Equal(ErrorCategory.Unauthorized, ex.Error.Category);
    }

    [Fact]
    public void ForbiddenException_HasForbiddenCategory()
    {
        var ex = new ForbiddenException("AUTH.FORBIDDEN", "Not allowed");
        Assert.Equal(ErrorCategory.Forbidden, ex.Error.Category);
    }

    [Fact]
    public void ValidationException_CarriesAllErrors()
    {
        var errors = new[] { Error.Validation("V1", "First"), Error.Validation("V2", "Second") };
        var ex = new ValidationException(errors);
        Assert.Equal(2, ex.Errors.Count);
        Assert.Equal(ErrorCategory.Validation, ex.Error.Category);
    }

    [Fact]
    public void AppException_IsAnException()
    {
        var ex = new NotFoundException("X", "X");
        Assert.IsAssignableFrom<Exception>(ex);
        Assert.IsAssignableFrom<AppException>(ex);
    }
}
