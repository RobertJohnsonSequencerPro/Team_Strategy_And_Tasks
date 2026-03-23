using FluentAssertions;
using TeamStrategyAndTasks.Core.Exceptions;

namespace TeamStrategyAndTasks.Core.Tests.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithEntityNameAndId_FormatsMessageCorrectly()
    {
        var id = Guid.NewGuid();
        var ex = new NotFoundException("Objective", id);

        ex.Message.Should().Contain("Objective");
        ex.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void Constructor_WithCustomMessage_UsesProvidedMessage()
    {
        const string message = "Custom not found message";
        var ex = new NotFoundException(message);

        ex.Message.Should().Be(message);
    }

    [Fact]
    public void NotFoundException_IsException()
    {
        var ex = new NotFoundException("Entity", Guid.NewGuid());
        ex.Should().BeAssignableTo<Exception>();
    }

    [Theory]
    [InlineData("Objective")]
    [InlineData("BusinessProcess")]
    [InlineData("Initiative")]
    [InlineData("WorkTask")]
    public void Constructor_WithDifferentEntityNames_IncludesEntityNameInMessage(string entityName)
    {
        var id = Guid.NewGuid();
        var ex = new NotFoundException(entityName, id);

        ex.Message.Should().Contain(entityName);
    }
}

public class ForbiddenExceptionTests
{
    [Fact]
    public void DefaultConstructor_HasDefaultMessage()
    {
        var ex = new ForbiddenException();

        ex.Message.Should().NotBeNullOrEmpty();
        ex.Message.Should().Contain("permission");
    }

    [Fact]
    public void Constructor_WithCustomMessage_UsesProvidedMessage()
    {
        const string message = "Access denied to this resource";
        var ex = new ForbiddenException(message);

        ex.Message.Should().Be(message);
    }

    [Fact]
    public void ForbiddenException_IsException()
    {
        var ex = new ForbiddenException();
        ex.Should().BeAssignableTo<Exception>();
    }
}

public class AppValidationExceptionTests
{
    [Fact]
    public void Constructor_WithDictionary_StoresErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Title"] = ["Title is required"],
            ["Status"] = ["Invalid status value"]
        };

        var ex = new AppValidationException(errors);

        ex.Errors.Should().ContainKey("Title");
        ex.Errors["Title"].Should().Contain("Title is required");
        ex.Errors.Should().ContainKey("Status");
    }

    [Fact]
    public void Constructor_WithFieldAndMessage_CreatesErrorsForField()
    {
        var ex = new AppValidationException("Title", "Title is required");

        ex.Errors.Should().ContainKey("Title");
        ex.Errors["Title"].Should().Contain("Title is required");
    }

    [Fact]
    public void Constructor_WithDictionary_SetsGenericMessage()
    {
        var ex = new AppValidationException("Field", "Error");

        ex.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AppValidationException_IsException()
    {
        var ex = new AppValidationException("Field", "Error");
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Errors_IsReadOnly()
    {
        var ex = new AppValidationException("Field", "Error");
        ex.Errors.Should().BeAssignableTo<IReadOnlyDictionary<string, string[]>>();
    }
}
