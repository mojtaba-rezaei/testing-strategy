namespace OrderProcessor.FunctionApp.Models;

public class ProcessResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public static ValidationResult Valid() => new() { IsValid = true };
    
    public static ValidationResult Invalid(params string[] errors) => new() 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };
}
