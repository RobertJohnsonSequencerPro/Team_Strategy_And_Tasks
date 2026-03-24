namespace TeamStrategyAndTasks.Core.DTOs;

public record CreateTeamRequest(string Name, string Mandate);

public record UpdateTeamRequest(string Name, string Mandate);
