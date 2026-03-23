using FluentAssertions;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Tests.Entities;

public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_WhenCreated_HasNonEmptyGuid()
    {
        var objective = new Objective();
        objective.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void BaseEntity_WhenCreated_SetsCreatedAtToUtcNow()
    {
        var before = DateTimeOffset.UtcNow;
        var objective = new Objective();
        var after = DateTimeOffset.UtcNow;

        objective.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void BaseEntity_WhenCreated_SetsUpdatedAtToUtcNow()
    {
        var before = DateTimeOffset.UtcNow;
        var objective = new Objective();
        var after = DateTimeOffset.UtcNow;

        objective.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void BaseEntity_EachInstance_HasUniqueId()
    {
        var obj1 = new Objective();
        var obj2 = new Objective();

        obj1.Id.Should().NotBe(obj2.Id);
    }
}

public class ObjectiveEntityTests
{
    [Fact]
    public void Objective_WhenCreated_HasDefaultNotStartedStatus()
    {
        var objective = new Objective();
        objective.Status.Should().Be(NodeStatus.NotStarted);
    }

    [Fact]
    public void Objective_WhenCreated_TitleIsEmptyString()
    {
        var objective = new Objective();
        objective.Title.Should().BeEmpty();
    }

    [Fact]
    public void Objective_WhenCreated_IsNotArchived()
    {
        var objective = new Objective();
        objective.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Objective_WhenCreated_HasEmptyObjectiveProcesses()
    {
        var objective = new Objective();
        objective.ObjectiveProcesses.Should().BeEmpty();
    }

    [Fact]
    public void Objective_CanSetTitle()
    {
        var objective = new Objective { Title = "Test Objective" };
        objective.Title.Should().Be("Test Objective");
    }

    [Fact]
    public void Objective_CanSetStatus()
    {
        var objective = new Objective { Status = NodeStatus.InProgress };
        objective.Status.Should().Be(NodeStatus.InProgress);
    }
}

public class BusinessProcessEntityTests
{
    [Fact]
    public void BusinessProcess_WhenCreated_HasDefaultNotStartedStatus()
    {
        var process = new BusinessProcess();
        process.Status.Should().Be(NodeStatus.NotStarted);
    }

    [Fact]
    public void BusinessProcess_WhenCreated_IsNotArchived()
    {
        var process = new BusinessProcess();
        process.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void BusinessProcess_WhenCreated_HasEmptyCollections()
    {
        var process = new BusinessProcess();
        process.ObjectiveProcesses.Should().BeEmpty();
        process.ProcessInitiatives.Should().BeEmpty();
    }
}

public class InitiativeEntityTests
{
    [Fact]
    public void Initiative_WhenCreated_HasDefaultNotStartedStatus()
    {
        var initiative = new Initiative();
        initiative.Status.Should().Be(NodeStatus.NotStarted);
    }

    [Fact]
    public void Initiative_WhenCreated_IsNotArchived()
    {
        var initiative = new Initiative();
        initiative.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Initiative_WhenCreated_HasEmptyCollections()
    {
        var initiative = new Initiative();
        initiative.ProcessInitiatives.Should().BeEmpty();
        initiative.InitiativeWorkTasks.Should().BeEmpty();
    }
}

public class WorkTaskEntityTests
{
    [Fact]
    public void WorkTask_WhenCreated_HasDefaultNotStartedStatus()
    {
        var task = new WorkTask();
        task.Status.Should().Be(NodeStatus.NotStarted);
    }

    [Fact]
    public void WorkTask_WhenCreated_IsNotArchived()
    {
        var task = new WorkTask();
        task.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void WorkTask_WhenCreated_AssigneeIdIsNull()
    {
        var task = new WorkTask();
        task.AssigneeId.Should().BeNull();
    }

    [Fact]
    public void WorkTask_WhenCreated_CompletionDateIsNull()
    {
        var task = new WorkTask();
        task.CompletionDate.Should().BeNull();
    }

    [Fact]
    public void WorkTask_WhenCreated_EstimatedAndActualEffortAreNull()
    {
        var task = new WorkTask();
        task.EstimatedEffort.Should().BeNull();
        task.ActualEffort.Should().BeNull();
    }
}

public class JoinEntityTests
{
    [Fact]
    public void ObjectiveProcess_LinkedAt_IsSetOnCreation()
    {
        var before = DateTimeOffset.UtcNow;
        var link = new ObjectiveProcess();
        var after = DateTimeOffset.UtcNow;

        link.LinkedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void ProcessInitiative_LinkedAt_IsSetOnCreation()
    {
        var before = DateTimeOffset.UtcNow;
        var link = new ProcessInitiative();
        var after = DateTimeOffset.UtcNow;

        link.LinkedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void InitiativeWorkTask_LinkedAt_IsSetOnCreation()
    {
        var before = DateTimeOffset.UtcNow;
        var link = new InitiativeWorkTask();
        var after = DateTimeOffset.UtcNow;

        link.LinkedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void ObjectiveProcess_DisplayOrder_DefaultsToZero()
    {
        var link = new ObjectiveProcess();
        link.DisplayOrder.Should().Be(0);
    }
}
