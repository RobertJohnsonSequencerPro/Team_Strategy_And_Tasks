using System.Text;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class CalendarInviteService : ICalendarInviteService
{
    public string GenerateIcs(TaskStep step, string taskTitle, string? assigneeEmail, string? assigneeDisplayName)
    {
        var dueDate = step.DueDate ?? DateTimeOffset.UtcNow.AddDays(1);
        var uid = $"{step.Id}@teamstrategyandtasks";
        var now = DateTimeOffset.UtcNow;

        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//TeamStrategyAndTasks//TaskPlanner//EN");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine("METHOD:REQUEST");
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine($"UID:{uid}");
        sb.AppendLine($"DTSTAMP:{now:yyyyMMddTHHmmssZ}");
        sb.AppendLine($"DTSTART;VALUE=DATE:{dueDate:yyyyMMdd}");
        sb.AppendLine($"DTEND;VALUE=DATE:{dueDate.AddDays(1):yyyyMMdd}");
        sb.AppendLine($"SUMMARY:Step: {EscapeIcsText(step.Title)} ({EscapeIcsText(taskTitle)})");
        if (!string.IsNullOrWhiteSpace(step.Description))
            sb.AppendLine($"DESCRIPTION:{EscapeIcsText(step.Description)}");
        if (!string.IsNullOrWhiteSpace(assigneeEmail))
            sb.AppendLine($"ATTENDEE;CN={assigneeDisplayName ?? assigneeEmail}:mailto:{assigneeEmail}");
        sb.AppendLine($"STATUS:{(step.Status == NodeStatus.Done ? "COMPLETED" : "CONFIRMED")}");
        sb.AppendLine("END:VEVENT");
        sb.AppendLine("END:VCALENDAR");

        return sb.ToString();
    }

    private static string EscapeIcsText(string text) =>
        text.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");
}
