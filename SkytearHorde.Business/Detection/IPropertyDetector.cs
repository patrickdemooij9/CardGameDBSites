namespace SkytearHorde.Business.Detection
{
    /// <summary>
    /// Detects a card property value from a cropped region of the card image,
    /// without using AI. Implement this for properties that can be reliably
    /// identified by visual characteristics such as color or shape.
    /// </summary>
    public interface IPropertyDetector
    {
        /// <summary>
        /// Detect the property value from a base64-encoded cropped card image.
        /// Returns null if detection is inconclusive.
        /// </summary>
        string? Detect(string base64CroppedImage);
    }
}
