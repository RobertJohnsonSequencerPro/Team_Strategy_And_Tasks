using Microsoft.Extensions.Logging;

namespace TeamStrategyAndTasks.Infrastructure.Jobs;

// Hangfire will resolve this via DI
public class EmailDigestJob(ILogger<EmailDigestJob> logger)
{
    public async Task SendDailyDigestAsync()
    {
        logger.LogInformation("EmailDigestJob: sending daily digest at {Time}", DateTimeOffset.UtcNow);
        // TODO(Phase 3): query overdue tasks, build per-user digest email, send via IEmailSender
        await Task.CompletedTask;
    }
}
