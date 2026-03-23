using FluentAssertions;
using Xunit;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Web.Components.Tree;

namespace TeamStrategyAndTasks.Web.Tests.Tree;

public class TreeBuilderTests
{
    private static Objective CreateObjective(string title, NodeStatus status = NodeStatus.NotStarted)
        => new() { Title = title, Status = status };

    private static BusinessProcess CreateProcess(string title, NodeStatus status = NodeStatus.NotStarted)
        => new() { Title = title, Status = status };

    private static Initiative CreateInitiative(string title, NodeStatus status = NodeStatus.NotStarted)
        => new() { Title = title, Status = status };

    private static WorkTask CreateTask(string title, NodeStatus status = NodeStatus.NotStarted)
        => new() { Title = title, Status = status };

    [Fact]
    public void Build_WithEmptyList_ReturnsEmptyTree()
    {
        var result = TreeBuilder.Build([]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Build_WithSingleObjective_ReturnsOneNode()
    {
        var objective = CreateObjective("My Objective");

        var result = TreeBuilder.Build([objective]);

        result.Should().ContainSingle();
        result[0].Title.Should().Be("My Objective");
        result[0].Level.Should().Be(NodeType.Objective);
    }

    [Fact]
    public void Build_ObjectiveNode_HasCorrectDetailUrl()
    {
        var objective = CreateObjective("Obj");

        var result = TreeBuilder.Build([objective]);

        result[0].DetailUrl.Should().Be($"/objectives/{objective.Id}");
    }

    [Fact]
    public void Build_ObjectiveNode_IsNotShared()
    {
        var objective = CreateObjective("Obj");

        var result = TreeBuilder.Build([objective]);

        result[0].IsShared.Should().BeFalse();
    }

    [Fact]
    public void Build_ObjectiveWithNoProcesses_HasZeroProgress()
    {
        var objective = CreateObjective("Obj");

        var result = TreeBuilder.Build([objective]);

        result[0].ProgressPct.Should().Be(0.0);
        result[0].Children.Should().BeEmpty();
    }

    [Fact]
    public void Build_MultipleObjectives_ReturnsSortedByTitle()
    {
        var objectives = new[]
        {
            CreateObjective("Zebra"),
            CreateObjective("Alpha"),
            CreateObjective("Middle")
        };

        var result = TreeBuilder.Build(objectives);

        result[0].Title.Should().Be("Alpha");
        result[1].Title.Should().Be("Middle");
        result[2].Title.Should().Be("Zebra");
    }

    [Fact]
    public void Build_ObjectiveWithProcess_ProcessIsChildNode()
    {
        var objective = CreateObjective("Objective");
        var process = CreateProcess("Process");
        objective.ObjectiveProcesses.Add(new ObjectiveProcess
        {
            ObjectiveId = objective.Id,
            ProcessId = process.Id,
            Process = process,
            Objective = objective
        });

        var result = TreeBuilder.Build([objective]);

        result[0].Children.Should().ContainSingle();
        var processNode = result[0].Children[0];
        processNode.Title.Should().Be("Process");
        processNode.Level.Should().Be(NodeType.Process);
        processNode.DetailUrl.Should().Be($"/processes/{process.Id}");
    }

    [Fact]
    public void Build_ProcessWithInitiative_InitiativeIsGrandchildNode()
    {
        var objective = CreateObjective("Objective");
        var process = CreateProcess("Process");
        var initiative = CreateInitiative("Initiative");

        process.ProcessInitiatives.Add(new ProcessInitiative
        {
            ProcessId = process.Id,
            InitiativeId = initiative.Id,
            Process = process,
            Initiative = initiative
        });
        objective.ObjectiveProcesses.Add(new ObjectiveProcess
        {
            ObjectiveId = objective.Id,
            ProcessId = process.Id,
            Process = process,
            Objective = objective
        });

        var result = TreeBuilder.Build([objective]);

        var initiativeNode = result[0].Children[0].Children[0];
        initiativeNode.Title.Should().Be("Initiative");
        initiativeNode.Level.Should().Be(NodeType.Initiative);
        initiativeNode.DetailUrl.Should().Be($"/initiatives/{initiative.Id}");
    }

    [Fact]
    public void Build_InitiativeWithTask_TaskIsDeepChildNode()
    {
        var objective = CreateObjective("Objective");
        var process = CreateProcess("Process");
        var initiative = CreateInitiative("Initiative");
        var task = CreateTask("Task");

        initiative.InitiativeWorkTasks.Add(new InitiativeWorkTask
        {
            InitiativeId = initiative.Id,
            WorkTaskId = task.Id,
            Initiative = initiative,
            WorkTask = task
        });
        process.ProcessInitiatives.Add(new ProcessInitiative
        {
            ProcessId = process.Id,
            InitiativeId = initiative.Id,
            Process = process,
            Initiative = initiative
        });
        objective.ObjectiveProcesses.Add(new ObjectiveProcess
        {
            ObjectiveId = objective.Id,
            ProcessId = process.Id,
            Process = process,
            Objective = objective
        });

        var result = TreeBuilder.Build([objective]);

        var taskNode = result[0].Children[0].Children[0].Children[0];
        taskNode.Title.Should().Be("Task");
        taskNode.Level.Should().Be(NodeType.Task);
        taskNode.DetailUrl.Should().Be($"/tasks/{task.Id}");
        taskNode.Children.Should().BeEmpty();
    }

    [Fact]
    public void Build_ArchivedProcess_IsExcludedFromChildren()
    {
        var objective = CreateObjective("Objective");
        var activeProcess = CreateProcess("Active Process");
        var archivedProcess = CreateProcess("Archived Process");
        archivedProcess.IsArchived = true;

        objective.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = activeProcess.Id, Process = activeProcess, Objective = objective });
        objective.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = archivedProcess.Id, Process = archivedProcess, Objective = objective });

        var result = TreeBuilder.Build([objective]);

        result[0].Children.Should().ContainSingle();
        result[0].Children[0].Title.Should().Be("Active Process");
    }

    [Fact]
    public void Build_ArchivedInitiative_IsExcludedFromChildren()
    {
        var objective = CreateObjective("Objective");
        var process = CreateProcess("Process");
        var activeInitiative = CreateInitiative("Active Initiative");
        var archivedInitiative = CreateInitiative("Archived Initiative");
        archivedInitiative.IsArchived = true;

        process.ProcessInitiatives.Add(new ProcessInitiative { InitiativeId = activeInitiative.Id, Initiative = activeInitiative, Process = process });
        process.ProcessInitiatives.Add(new ProcessInitiative { InitiativeId = archivedInitiative.Id, Initiative = archivedInitiative, Process = process });
        objective.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = process.Id, Process = process, Objective = objective });

        var result = TreeBuilder.Build([objective]);

        result[0].Children[0].Children.Should().ContainSingle();
        result[0].Children[0].Children[0].Title.Should().Be("Active Initiative");
    }

    [Fact]
    public void Build_ArchivedTask_IsExcludedFromChildren()
    {
        var objective = CreateObjective("Objective");
        var process = CreateProcess("Process");
        var initiative = CreateInitiative("Initiative");
        var activeTask = CreateTask("Active Task");
        var archivedTask = CreateTask("Archived Task");
        archivedTask.IsArchived = true;

        initiative.InitiativeWorkTasks.Add(new InitiativeWorkTask { WorkTaskId = activeTask.Id, WorkTask = activeTask, Initiative = initiative });
        initiative.InitiativeWorkTasks.Add(new InitiativeWorkTask { WorkTaskId = archivedTask.Id, WorkTask = archivedTask, Initiative = initiative });
        process.ProcessInitiatives.Add(new ProcessInitiative { InitiativeId = initiative.Id, Initiative = initiative, Process = process });
        objective.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = process.Id, Process = process, Objective = objective });

        var result = TreeBuilder.Build([objective]);

        var initiativeNode = result[0].Children[0].Children[0];
        initiativeNode.Children.Should().ContainSingle();
        initiativeNode.Children[0].Title.Should().Be("Active Task");
    }

    [Fact]
    public void Build_DoneTask_HasOneHundredPercentProgress()
    {
        var objective = CreateObjective("Objective");
        var process = CreateProcess("Process");
        var initiative = CreateInitiative("Initiative");
        var task = CreateTask("Done Task", NodeStatus.Done);

        initiative.InitiativeWorkTasks.Add(new InitiativeWorkTask { WorkTaskId = task.Id, WorkTask = task, Initiative = initiative });
        process.ProcessInitiatives.Add(new ProcessInitiative { InitiativeId = initiative.Id, Initiative = initiative, Process = process });
        objective.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = process.Id, Process = process, Objective = objective });

        var result = TreeBuilder.Build([objective]);

        var taskNode = result[0].Children[0].Children[0].Children[0];
        taskNode.ProgressPct.Should().Be(100.0);
    }

    [Fact]
    public void Build_NotDoneTask_HasZeroProgress()
    {
        var objective = CreateObjective("Objective");
        var process = CreateProcess("Process");
        var initiative = CreateInitiative("Initiative");
        var task = CreateTask("In Progress Task", NodeStatus.InProgress);

        initiative.InitiativeWorkTasks.Add(new InitiativeWorkTask { WorkTaskId = task.Id, WorkTask = task, Initiative = initiative });
        process.ProcessInitiatives.Add(new ProcessInitiative { InitiativeId = initiative.Id, Initiative = initiative, Process = process });
        objective.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = process.Id, Process = process, Objective = objective });

        var result = TreeBuilder.Build([objective]);

        var taskNode = result[0].Children[0].Children[0].Children[0];
        taskNode.ProgressPct.Should().Be(0.0);
    }

    [Fact]
    public void Build_InitiativeWithMixedTasks_ProgressIsAverageOfChildren()
    {
        var objective = CreateObjective("Objective");
        var process = CreateProcess("Process");
        var initiative = CreateInitiative("Initiative");
        var doneTask = CreateTask("Done", NodeStatus.Done);
        var notDoneTask = CreateTask("NotDone", NodeStatus.NotStarted);

        initiative.InitiativeWorkTasks.Add(new InitiativeWorkTask { WorkTaskId = doneTask.Id, WorkTask = doneTask, Initiative = initiative });
        initiative.InitiativeWorkTasks.Add(new InitiativeWorkTask { WorkTaskId = notDoneTask.Id, WorkTask = notDoneTask, Initiative = initiative });
        process.ProcessInitiatives.Add(new ProcessInitiative { InitiativeId = initiative.Id, Initiative = initiative, Process = process });
        objective.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = process.Id, Process = process, Objective = objective });

        var result = TreeBuilder.Build([objective]);

        var initiativeNode = result[0].Children[0].Children[0];
        initiativeNode.ProgressPct.Should().Be(50.0); // Average of 100% and 0%
    }

    [Fact]
    public void Build_ProcessSharedAcrossObjectives_IsMarkedAsShared()
    {
        var obj1 = CreateObjective("Objective 1");
        var obj2 = CreateObjective("Objective 2");
        var sharedProcess = CreateProcess("Shared Process");

        obj1.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = sharedProcess.Id, Process = sharedProcess, Objective = obj1 });
        obj2.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = sharedProcess.Id, Process = sharedProcess, Objective = obj2 });

        var result = TreeBuilder.Build([obj1, obj2]);

        var processInObj1 = result.First(o => o.Title == "Objective 1").Children[0];
        var processInObj2 = result.First(o => o.Title == "Objective 2").Children[0];
        processInObj1.IsShared.Should().BeTrue();
        processInObj2.IsShared.Should().BeTrue();
    }

    [Fact]
    public void Build_ProcessNotShared_IsNotMarkedAsShared()
    {
        var obj1 = CreateObjective("Objective 1");
        var obj2 = CreateObjective("Objective 2");
        var uniqueProcess1 = CreateProcess("Process 1");
        var uniqueProcess2 = CreateProcess("Process 2");

        obj1.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = uniqueProcess1.Id, Process = uniqueProcess1, Objective = obj1 });
        obj2.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = uniqueProcess2.Id, Process = uniqueProcess2, Objective = obj2 });

        var result = TreeBuilder.Build([obj1, obj2]);

        result[0].Children[0].IsShared.Should().BeFalse();
        result[1].Children[0].IsShared.Should().BeFalse();
    }

    [Fact]
    public void Build_ObjectiveStatus_IsPreservedInNode()
    {
        var objective = CreateObjective("Objective", NodeStatus.AtRisk);

        var result = TreeBuilder.Build([objective]);

        result[0].Status.Should().Be(NodeStatus.AtRisk);
    }

    [Fact]
    public void Build_TaskNode_HasNoChildren()
    {
        var objective = CreateObjective("Objective");
        var process = CreateProcess("Process");
        var initiative = CreateInitiative("Initiative");
        var task = CreateTask("Task");

        initiative.InitiativeWorkTasks.Add(new InitiativeWorkTask { WorkTaskId = task.Id, WorkTask = task, Initiative = initiative });
        process.ProcessInitiatives.Add(new ProcessInitiative { InitiativeId = initiative.Id, Initiative = initiative, Process = process });
        objective.ObjectiveProcesses.Add(new ObjectiveProcess { ProcessId = process.Id, Process = process, Objective = objective });

        var result = TreeBuilder.Build([objective]);

        var taskNode = result[0].Children[0].Children[0].Children[0];
        taskNode.Children.Should().BeEmpty();
    }
}
