using Xunit;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Infrastructure.Services;
using FluentAssertions;

namespace TeamStrategyAndTasks.Integration.Tests;

public class CalendarInviteServiceTests
{
    private readonly CalendarInviteService _sut = new();

    private static TaskStep MakeStep(string title, DateTimeOffset? dueDate = null,
        Guid? assigneeId = null, NodeStatus status = NodeStatus.NotStarted) =>
        new()
        {
            Title = title,
            WorkTaskId = Guid.NewGuid(),
            DueDate = dueDate,
            AssigneeId = assigneeId,
            Status = status
        };

    [Fact]
    public void GenerateIcs_ReturnsVCalendarContent()
    {
        var step = MakeStep("Do something", DateTimeOffset.UtcNow.AddDays(3));

        var ics = _sut.GenerateIcs(step, "Parent Task", null, null);

        ics.Should().Contain("BEGIN:VCALENDAR");
        ics.Should().Contain("END:VCALENDAR");
        ics.Should().Contain("BEGIN:VEVENT");
        ics.Should().Contain("END:VEVENT");
    }

    [Fact]
    public void GenerateIcs_ContainsDueDate()
    {
        var dueDate = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var step = MakeStep("Ship it", dueDate);

        var ics = _sut.GenerateIcs(step, "Release", null, null);

        ics.Should().Contain("DTSTART;VALUE=DATE:20260615");
        ics.Should().Contain("DTEND;VALUE=DATE:20260616");
    }

    [Fact]
    public void GenerateIcs_ContainsStepTitleAndTaskTitle()
    {
        var step = MakeStep("Write tests", DateTimeOffset.UtcNow.AddDays(1));

        var ics = _sut.GenerateIcs(step, "Build Feature", null, null);

        ics.Should().Contain("SUMMARY:Step: Write tests (Build Feature)");
    }

    [Fact]
    public void GenerateIcs_ContainsAttendeeWhenEmailProvided()
    {
        var step = MakeStep("Review code", DateTimeOffset.UtcNow.AddDays(2), Guid.NewGuid());

        var ics = _sut.GenerateIcs(step, "Code Review", "alice@example.com", "Alice");

        ics.Should().Contain("ATTENDEE;CN=Alice:mailto:alice@example.com");
    }

    [Fact]
    public void GenerateIcs_ContainsUidWithStepId()
    {
        var step = MakeStep("Deploy", DateTimeOffset.UtcNow.AddDays(1));

        var ics = _sut.GenerateIcs(step, "Release", null, null);

        ics.Should().Contain($"UID:{step.Id}@teamstrategyandtasks");
    }

    [Fact]
    public void GenerateIcs_StatusIsCompletedWhenStepIsDone()
    {
        var step = MakeStep("Done step", DateTimeOffset.UtcNow, status: NodeStatus.Done);

        var ics = _sut.GenerateIcs(step, "Task", null, null);

        ics.Should().Contain("STATUS:COMPLETED");
    }

    [Fact]
    public void GenerateIcs_StatusIsConfirmedWhenStepIsNotDone()
    {
        var step = MakeStep("In progress step", DateTimeOffset.UtcNow, status: NodeStatus.InProgress);

        var ics = _sut.GenerateIcs(step, "Task", null, null);

        ics.Should().Contain("STATUS:CONFIRMED");
    }

    [Fact]
    public void GenerateIcs_EscapesSpecialCharactersInTitle()
    {
        var step = MakeStep("Fix bug, urgent; critical", DateTimeOffset.UtcNow.AddDays(1));

        var ics = _sut.GenerateIcs(step, "Product", null, null);

        ics.Should().Contain(@"Fix bug\, urgent\; critical");
    }

    [Fact]
    public void GenerateIcs_UsesFallbackDueDateWhenNoneSet()
    {
        var step = MakeStep("No date step", dueDate: null);

        var ics = _sut.GenerateIcs(step, "Task", null, null);

        // Should not throw and should contain a DTSTART line
        ics.Should().Contain("DTSTART;VALUE=DATE:");
    }
}
