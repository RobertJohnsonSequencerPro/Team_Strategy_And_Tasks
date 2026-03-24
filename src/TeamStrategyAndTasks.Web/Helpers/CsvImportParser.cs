using System.Globalization;
using TeamStrategyAndTasks.Core.DTOs;

namespace TeamStrategyAndTasks.Web.Helpers;

/// <summary>One parsed row from an import CSV.</summary>
public sealed record TaskImportRow(
    int RowNumber,
    string Title,
    string? Description,
    DateTimeOffset? TargetDate,
    decimal? EstimatedEffort,
    string? Error   // null = valid
)
{
    public bool IsValid => Error is null;

    public CreateTaskRequest ToRequest() => new(
        Title,
        string.IsNullOrWhiteSpace(Description) ? null : Description,
        null,               // AssigneeId — not set via CSV
        EstimatedEffort,
        TargetDate);
}

/// <summary>Parses CSV text into <see cref="TaskImportRow"/> records for preview and bulk import.</summary>
public static class CsvImportParser
{
    // Expected column names (case-insensitive). Only Title is mandatory.
    private const string ColTitle    = "title";
    private const string ColDesc     = "description";
    private const string ColDate     = "target date";
    private const string ColEffort   = "estimated effort";

    /// <summary>
    /// Parses <paramref name="csvText"/> and returns one row per non-blank data line.
    /// The first non-blank line must be a header row.
    /// </summary>
    public static IReadOnlyList<TaskImportRow> Parse(string csvText)
    {
        var lines = csvText
            .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count == 0)
            return [];

        // --- header ---
        var headers = SplitCsvLine(lines[0])
            .Select(h => h.Trim().ToLowerInvariant())
            .ToArray();

        int Idx(string name) =>
            Array.IndexOf(headers, name);

        var titleIdx  = Idx(ColTitle);
        var descIdx   = Idx(ColDesc);
        var dateIdx   = Idx(ColDate);
        var effortIdx = Idx(ColEffort);

        if (titleIdx < 0)
            return [ new TaskImportRow(0, "", null, null, null, "Header row missing required \"Title\" column.") ];

        var rows = new List<TaskImportRow>();
        for (int i = 1; i < lines.Count; i++)
        {
            int rowNum = i;
            var fields = SplitCsvLine(lines[i]);

            string Get(int idx) =>
                idx >= 0 && idx < fields.Length ? fields[idx].Trim() : string.Empty;

            var title = Get(titleIdx);
            if (string.IsNullOrWhiteSpace(title))
            {
                rows.Add(new TaskImportRow(rowNum, "(blank)", null, null, null, "Title is required."));
                continue;
            }

            // TargetDate
            DateTimeOffset? targetDate = null;
            var dateStr = Get(dateIdx);
            if (!string.IsNullOrWhiteSpace(dateStr))
            {
                if (DateTimeOffset.TryParseExact(dateStr, "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
                    targetDate = parsed;
                else if (DateTimeOffset.TryParse(dateStr, CultureInfo.InvariantCulture,
                             DateTimeStyles.AssumeUniversal, out var parsed2))
                    targetDate = parsed2;
                else
                {
                    rows.Add(new TaskImportRow(rowNum, title, Get(descIdx), null, null,
                        $"Invalid date \"{dateStr}\" — expected yyyy-MM-dd."));
                    continue;
                }
            }

            // EstimatedEffort
            decimal? effort = null;
            var effortStr = Get(effortIdx);
            if (!string.IsNullOrWhiteSpace(effortStr))
            {
                if (decimal.TryParse(effortStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var e))
                    effort = e;
                else
                {
                    rows.Add(new TaskImportRow(rowNum, title, Get(descIdx), targetDate, null,
                        $"Invalid effort \"{effortStr}\" — expected a number."));
                    continue;
                }
            }

            rows.Add(new TaskImportRow(rowNum, title, Get(descIdx), targetDate, effort, null));
        }

        return rows;
    }

    // ---- CSV line splitter (handles quoted fields) -------------------------
    private static string[] SplitCsvLine(string line)
    {
        var fields = new List<string>();
        int pos = 0;
        while (pos <= line.Length)
        {
            if (pos == line.Length) { fields.Add(""); break; }
            if (line[pos] == '"')
            {
                pos++;
                var sb = new System.Text.StringBuilder();
                while (pos < line.Length)
                {
                    if (line[pos] == '"')
                    {
                        pos++;
                        if (pos < line.Length && line[pos] == '"') { sb.Append('"'); pos++; }
                        else break;
                    }
                    else { sb.Append(line[pos++]); }
                }
                fields.Add(sb.ToString());
                if (pos < line.Length && line[pos] == ',') pos++;
            }
            else
            {
                int comma = line.IndexOf(',', pos);
                if (comma < 0) { fields.Add(line[pos..]); break; }
                fields.Add(line[pos..comma]);
                pos = comma + 1;
            }
        }
        return fields.ToArray();
    }
}
