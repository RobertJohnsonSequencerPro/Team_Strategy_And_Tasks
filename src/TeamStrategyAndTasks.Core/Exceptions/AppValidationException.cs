namespace TeamStrategyAndTasks.Core.Exceptions;

public class AppValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public AppValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.AsReadOnly();
    }

    public AppValidationException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = [message] }) { }
}
