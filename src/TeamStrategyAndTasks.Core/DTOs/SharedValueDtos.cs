namespace TeamStrategyAndTasks.Core.DTOs;

public record CreateSharedValueRequest(string Name, string Definition);

public record UpdateSharedValueRequest(string Name, string Definition);
