using Microsoft.AspNetCore.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
