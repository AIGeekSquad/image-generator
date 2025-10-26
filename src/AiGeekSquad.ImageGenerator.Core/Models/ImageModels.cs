namespace AiGeekSquad.ImageGenerator.Core.Models;

/// <summary>
/// Supported image generation models across providers
/// </summary>
public static class ImageModels
{
    /// <summary>
    /// OpenAI image generation models
    /// </summary>
    public static class OpenAI
    {
        /// <summary>
        /// DALL-E 3 - Latest high-quality image generation
        /// </summary>
        public const string DallE3 = "dall-e-3";

        /// <summary>
        /// DALL-E 2 - Previous generation
        /// </summary>
        public const string DallE2 = "dall-e-2";

        /// <summary>
        /// GPT Image 1 - New GPT-based image generation model
        /// </summary>
        public const string GPTImage1 = "gpt-image-1";

        /// <summary>
        /// GPT-5 Image Generation - Next generation model (if available)
        /// Note: This is a placeholder for future GPT-5 image capabilities
        /// </summary>
        public const string GPT5Image = "gpt-5-image";

        /// <summary>
        /// Default model
        /// </summary>
        public const string Default = DallE3;
    }

    /// <summary>
    /// Google Gemini/Imagen models
    /// </summary>
    public static class Google
    {
        /// <summary>
        /// Imagen 3 - Latest image generation model
        /// </summary>
        public const string Imagen3 = "imagen-3.0-generate-001";

        /// <summary>
        /// Imagen 2 - Previous generation
        /// </summary>
        public const string Imagen2 = "imagegeneration@006";

        /// <summary>
        /// Imagen Fast - Optimized for speed
        /// </summary>
        public const string ImagenFast = "imagen-3.0-fast-generate-001";

        /// <summary>
        /// Default model
        /// </summary>
        public const string Default = Imagen3;
    }

    /// <summary>
    /// Standard image sizes
    /// </summary>
    public static class Sizes
    {
        public const string Square1024 = "1024x1024";
        public const string Square512 = "512x512";
        public const string Square256 = "256x256";
        public const string Wide1792x1024 = "1792x1024";
        public const string Tall1024x1792 = "1024x1792";
    }

    /// <summary>
    /// Image quality options
    /// </summary>
    public static class Quality
    {
        public const string Standard = "standard";
        public const string HD = "hd";
    }

    /// <summary>
    /// Image style options
    /// </summary>
    public static class Style
    {
        public const string Vivid = "vivid";
        public const string Natural = "natural";
    }
}
