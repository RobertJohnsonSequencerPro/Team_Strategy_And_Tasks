using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Web.Helpers;
using System.Text;

namespace TeamStrategyAndTasks.Web.Helpers;

/// <summary>Builds CSV strings from strategy hierarchy data for browser download.</summary>
public static class CsvBuilder
{
    // ── Public entry points ────────────────────────────────────────────────

    /// <summary>
    /// Flat CSV of the full strategy hierarchy: Objectives → Processes → Initiatives → Tasks.
    /// Each row represents one node; M:M parents are listed as a semicolon-separated string.
    /// </summary>
    public static string BuildHierarchyCsv(IReadOnlyList<Objective> objectives)
    {
        var sb = new StringBuilder();
        AppendRow(sb,
            "Level", "Title", "Status", "Progress %",
            "Target Date", "Success Metric", "Target Value",
            "Team", "Linked To");

        foreach (var obj in objectives.OrderBy(o => o.Title))
        {
            var objPct = ProgressHelper.ObjectiveProgress(obj);
            AppendRow(sb,
                "Objective",
                obj.Title,
                obj.Status.ToString(),
                objPct.HasValue ? $"{objPct}%" : "",
                obj.TargetDate?.ToString("yyyy-MM-dd") ?? "",
                obj.SuccessMetric ?? "",
                obj.TargetValue ?? "",
                obj.Team?.Name ?? "",
                "");

            var procs = obj.ObjectiveProcesses
                .Where(op => op.Process is not null && !op.Process.IsArchived)
                .Select(op => op.Process!)
                .OrderBy(p => p.Title);

            foreach (var proc in procs)
            {
                var procPct = ProgressHelper.ProcessProgress(proc);
                var procParents = proc.ObjectiveProcesses
                    .Where(op => op.Objective is not null)
                    .Select(op => op.Objective!.Title)
                    .Order();
                AppendRow(sb,
                    "Process",
                    proc.Title,
                    proc.Status.ToString(),
                    procPct.HasValue ? $"{procPct}%" : "",
                    proc.TargetDate?.ToString("yyyy-MM-dd") ?? "",
                    proc.SuccessMetric ?? "",
                    proc.TargetValue ?? "",
                    proc.Team?.Name ?? "",
                    string.Join("; ", procParents));

                var inits = proc.ProcessInitiatives
                    .Where(pi => pi.Initiative is not null && !pi.Initiative.IsArchived)
                    .Select(pi => pi.Initiative!)
                    .OrderBy(i => i.Title);

                foreach (var init in inits)
                {
                    var initPct = ProgressHelper.InitiativeProgress(init);
                    var initParents = init.ProcessInitiatives
                        .Where(pi => pi.Process is not null)
                        .Select(pi => pi.Process!.Title)
                        .Order();
                    AppendRow(sb,
                        "Initiative",
                        init.Title,
                        init.Status.ToString(),
                        initPct.HasValue ? $"{initPct}%" : "",
                        init.TargetDate?.ToString("yyyy-MM-dd") ?? "",
                        "",
                        "",
                        init.Team?.Name ?? "",
                        string.Join("; ", initParents));

                    var tasks = init.InitiativeWorkTasks
                        .Where(iwt => iwt.WorkTask is not null && !iwt.WorkTask.IsArchived)
                        .Select(iwt => iwt.WorkTask!)
                        .OrderBy(t => t.Title);

                    foreach (var task in tasks)
                    {
                        var taskParents = task.InitiativeWorkTasks
                            .Where(iwt => iwt.Initiative is not null)
                            .Select(iwt => iwt.Initiative!.Title)
                            .Order();
                        AppendRow(sb,
                            "Task",
                            task.Title,
                            task.Status.ToString(),
                            ProgressHelper.IsTaskDone(task) ? "100%" : "0%",
                            task.TargetDate?.ToString("yyyy-MM-dd") ?? "",
                            "",
                            "",
                            task.Team?.Name ?? "",
                            string.Join("; ", taskParents));
                    }
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Flat CSV of all tasks with effort, dates, and initiative links.
    /// </summary>
    public static string BuildTasksCsv(IEnumerable<WorkTask> tasks)
    {
        var sb = new StringBuilder();
        AppendRow(sb,
            "Title", "Description", "Status",
            "Target Date", "Completion Date",
            "Estimated Effort", "Actual Effort",
            "Team", "Initiatives");

        foreach (var t in tasks.OrderBy(t => t.Title))
        {
            var initiatives = t.InitiativeWorkTasks
                .Where(iwt => iwt.Initiative is not null)
                .Select(iwt => iwt.Initiative!.Title)
                .Order();
            AppendRow(sb,
                t.Title,
                t.Description ?? "",
                t.Status.ToString(),
                t.TargetDate?.ToString("yyyy-MM-dd") ?? "",
                t.CompletionDate?.ToString("yyyy-MM-dd") ?? "",
                t.EstimatedEffort?.ToString() ?? "",
                t.ActualEffort?.ToString() ?? "",
                t.Team?.Name ?? "",
                string.Join("; ", initiatives));
        }

        return sb.ToString();
    }

    // ── CSV utilities ──────────────────────────────────────────────────────

    private static void AppendRow(StringBuilder sb, params string[] values)
    {
        sb.AppendLine(string.Join(",", values.Select(EscapeCsvField)));
    }

    private static string EscapeCsvField(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
