using FluentAssertions;
using TeamStrategyAndTasks.Core.Entities.Suggestions;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Infrastructure.Services;
using TeamStrategyAndTasks.Integration.Tests.Helpers;

namespace TeamStrategyAndTasks.Integration.Tests.Services;

public class SuggestionServiceTests
{
    private static SuggestionService CreateService(out Infrastructure.Data.AppDbContext db)
    {
        db = InMemoryDbHelper.CreateContext();
        return new SuggestionService(db);
    }

    [Fact]
    public async Task GetSuggestedObjectivesAsync_ReturnsOnlyActiveObjectives()
    {
        var service = CreateService(out var db);
        db.SuggestionObjectives.AddRange(
            new SuggestionObjective { Title = "Active 1", IsActive = true },
            new SuggestionObjective { Title = "Active 2", IsActive = true },
            new SuggestionObjective { Title = "Inactive", IsActive = false }
        );
        await db.SaveChangesAsync();

        var results = await service.GetSuggestedObjectivesAsync();

        results.Should().HaveCount(2);
        results.Should().OnlyContain(s => s.IsActive);
        results.Should().NotContain(s => s.Title == "Inactive");
    }

    [Fact]
    public async Task GetSuggestedObjectivesAsync_ReturnsObjectivesOrderedByTitle()
    {
        var service = CreateService(out var db);
        db.SuggestionObjectives.AddRange(
            new SuggestionObjective { Title = "Zebra Objective", IsActive = true },
            new SuggestionObjective { Title = "Alpha Objective", IsActive = true }
        );
        await db.SaveChangesAsync();

        var results = await service.GetSuggestedObjectivesAsync();

        results[0].Title.Should().Be("Alpha Objective");
        results[1].Title.Should().Be("Zebra Objective");
    }

    [Fact]
    public async Task CreateSuggestionObjectiveAsync_CreatesAndReturnsNewObjective()
    {
        var service = CreateService(out var db);

        var result = await service.CreateSuggestionObjectiveAsync("New Suggestion", "A description");

        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("New Suggestion");
        result.Description.Should().Be("A description");
        result.IsActive.Should().BeTrue();

        var inDb = await db.SuggestionObjectives.FindAsync(result.Id);
        inDb.Should().NotBeNull();
    }

    [Fact]
    public async Task SetSuggestionActiveAsync_ForObjective_SetsActiveFlag()
    {
        var service = CreateService(out var db);
        var obj = new SuggestionObjective { Title = "Test", IsActive = true };
        db.SuggestionObjectives.Add(obj);
        await db.SaveChangesAsync();

        await service.SetSuggestionActiveAsync("objective", obj.Id, false);

        var inDb = await db.SuggestionObjectives.FindAsync(obj.Id);
        inDb!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SetSuggestionActiveAsync_ForProcess_SetsActiveFlag()
    {
        var service = CreateService(out var db);
        var proc = new SuggestionProcess { Title = "Test Process", IsActive = true };
        db.SuggestionProcesses.Add(proc);
        await db.SaveChangesAsync();

        await service.SetSuggestionActiveAsync("process", proc.Id, false);

        var inDb = await db.SuggestionProcesses.FindAsync(proc.Id);
        inDb!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SetSuggestionActiveAsync_ForInitiative_SetsActiveFlag()
    {
        var service = CreateService(out var db);
        var init = new SuggestionInitiative { Title = "Test Initiative", IsActive = true };
        db.SuggestionInitiatives.Add(init);
        await db.SaveChangesAsync();

        await service.SetSuggestionActiveAsync("initiative", init.Id, false);

        var inDb = await db.SuggestionInitiatives.FindAsync(init.Id);
        inDb!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SetSuggestionActiveAsync_ForTask_SetsActiveFlag()
    {
        var service = CreateService(out var db);
        var task = new SuggestionTask { Title = "Test Task", IsActive = true };
        db.SuggestionTasks.Add(task);
        await db.SaveChangesAsync();

        await service.SetSuggestionActiveAsync("task", task.Id, false);

        var inDb = await db.SuggestionTasks.FindAsync(task.Id);
        inDb!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SetSuggestionActiveAsync_IsCaseInsensitive()
    {
        var service = CreateService(out var db);
        var obj = new SuggestionObjective { Title = "Test", IsActive = false };
        db.SuggestionObjectives.Add(obj);
        await db.SaveChangesAsync();

        await service.SetSuggestionActiveAsync("OBJECTIVE", obj.Id, true);

        var inDb = await db.SuggestionObjectives.FindAsync(obj.Id);
        inDb!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SetSuggestionActiveAsync_WithUnknownNodeType_ThrowsArgumentException()
    {
        var service = CreateService(out _);

        var act = () => service.SetSuggestionActiveAsync("unknown", Guid.NewGuid(), true);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*unknown*");
    }

    [Fact]
    public async Task SetSuggestionActiveAsync_WithNonExistingObjective_ThrowsNotFoundException()
    {
        var service = CreateService(out _);

        var act = () => service.SetSuggestionActiveAsync("objective", Guid.NewGuid(), true);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetSuggestedProcessesForObjectiveAsync_ReturnsOnlyActiveLinkedProcesses()
    {
        var service = CreateService(out var db);
        var suggObjective = new SuggestionObjective { Title = "Objective" };
        var activeProcess = new SuggestionProcess { Title = "Active Process", IsActive = true };
        var inactiveProcess = new SuggestionProcess { Title = "Inactive Process", IsActive = false };
        db.SuggestionObjectives.Add(suggObjective);
        db.SuggestionProcesses.AddRange(activeProcess, inactiveProcess);
        db.SuggestionObjectiveProcesses.AddRange(
            new SuggestionObjectiveProcess { SuggestionObjectiveId = suggObjective.Id, SuggestionProcessId = activeProcess.Id, SuggestionObjective = suggObjective, SuggestionProcess = activeProcess },
            new SuggestionObjectiveProcess { SuggestionObjectiveId = suggObjective.Id, SuggestionProcessId = inactiveProcess.Id, SuggestionObjective = suggObjective, SuggestionProcess = inactiveProcess }
        );
        await db.SaveChangesAsync();

        var results = await service.GetSuggestedProcessesForObjectiveAsync(suggObjective.Id);

        results.Should().ContainSingle();
        results[0].Title.Should().Be("Active Process");
    }

    [Fact]
    public async Task GetSuggestedTasksForInitiativeAsync_ReturnsOnlyActiveLinkedTasks()
    {
        var service = CreateService(out var db);
        var suggInitiative = new SuggestionInitiative { Title = "Initiative" };
        var activeTask = new SuggestionTask { Title = "Active Task", IsActive = true };
        var inactiveTask = new SuggestionTask { Title = "Inactive Task", IsActive = false };
        db.SuggestionInitiatives.Add(suggInitiative);
        db.SuggestionTasks.AddRange(activeTask, inactiveTask);
        db.SuggestionInitiativeTasks.AddRange(
            new SuggestionInitiativeTask { SuggestionInitiativeId = suggInitiative.Id, SuggestionTaskId = activeTask.Id, SuggestionInitiative = suggInitiative, SuggestionTask = activeTask },
            new SuggestionInitiativeTask { SuggestionInitiativeId = suggInitiative.Id, SuggestionTaskId = inactiveTask.Id, SuggestionInitiative = suggInitiative, SuggestionTask = inactiveTask }
        );
        await db.SaveChangesAsync();

        var results = await service.GetSuggestedTasksForInitiativeAsync(suggInitiative.Id);

        results.Should().ContainSingle();
        results[0].Title.Should().Be("Active Task");
    }
}
