namespace N2.Resources
{
    /// <summary>
    /// Implemented by file systems that expose their files via a publically accessible URL
    /// </summary>
    public interface IWebAccessible
    {
        string GetPublicURL(string filePath);
    }
}
