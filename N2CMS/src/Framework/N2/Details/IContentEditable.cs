namespace N2.Details
{
    /// <summary>
    /// marker interface to indicate support for HTML5 contenteditable 
    /// </summary>
    public interface IContentEditable
    {
        string GetEditorCssClass();
    }
}