namespace AiGeekSquad.ImageGenerator.Core.Services;

/// <summary>
/// Parsed image size dimensions
/// </summary>
public class ImageSize
{
    /// <summary>Gets or sets the image width in pixels</summary>
    public int Width { get; set; }
    /// <summary>Gets or sets the image height in pixels</summary>
    public int Height { get; set; }
    
    /// <summary>Returns a string representation of the image size</summary>
    public override string ToString() => $"{Width}x{Height}";
}
