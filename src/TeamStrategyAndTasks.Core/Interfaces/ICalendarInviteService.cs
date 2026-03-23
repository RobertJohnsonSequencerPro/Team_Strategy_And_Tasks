using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Core.Interfaces;

/// <summary>Generates RFC 5545 iCalendar (.ics) content for task step due dates.</summary>
public interface ICalendarInviteService
{
    /// <summary>
    /// Generates an ICS file string for the given task step.
    /// </summary>
    /// <param name="step">The task step to create the event for.</param>
    /// <param name="taskTitle">Title of the parent task (used in the event summary).</param>
    /// <param name="assigneeEmail">Email address of the assignee (used as ATTENDEE).</param>
    /// <param name="assigneeDisplayName">Display name of the assignee.</param>
    /// <returns>RFC 5545 iCalendar text content.</returns>
    string GenerateIcs(TaskStep step, string taskTitle, string? assigneeEmail, string? assigneeDisplayName);
}
