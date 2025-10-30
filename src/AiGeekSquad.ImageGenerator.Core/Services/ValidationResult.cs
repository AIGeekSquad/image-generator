namespace AiGeekSquad.ImageGenerator.Core.Services;

/// <summary>
/// Validation result for parsed arguments
/// </summary>
public class ValidationResult
{
    /// <summary>Gets whether the validation passed (no errors)</summary>
    public bool IsValid => Errors.Count == 0;
    /// <summary>Gets or sets the list of validation errors</summary>
    public List<string> Errors { get; set; } = new();
    /// <summary>Gets or sets the list of validation warnings</summary>
    public List<string> Warnings { get; set; } = new();
}
