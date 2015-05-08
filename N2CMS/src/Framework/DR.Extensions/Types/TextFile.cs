using System.Globalization;
using System.IO;
using System.Text;
using N2.Azure;
using N2.Edit.FileSystem;
using N2.Resources;

namespace N2.Types
{
    public class TextFile 
    {
        public static IFileSystem GetFileSystem(FileSystemNamespace fileSystemNamespace)
        {
            var fileSystemFactory = Context.Current.Resolve<IFileSystemFactory>();
            return fileSystemFactory.Create(fileSystemNamespace);
        }

        public static string GenerateTextFilePath(string pagePath, int pageVersion, string partIdentifier, string extension)
        {
            // the return path is for the form:
            //     <PageFilePath>/<PageVersion><PartIdentifier>.css
            string filename = string.Format("{0}{1}.{2}", pageVersion.ToString(CultureInfo.InvariantCulture), partIdentifier, extension);
            string filePath = Path.Combine(pagePath, filename);
            return filePath;
        }

        public static string GetTextFileContents(IFileSystem fs, string filePath)
        {
            string text = null;

            if (string.IsNullOrEmpty(filePath))
                return string.Empty; // new part - no path, no text

            // load the file if it exists
            if (fs.FileExists(filePath))
            {
                using (var stream = new MemoryStream())
                {
                    fs.ReadFileContents(filePath, stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var streamReader = new StreamReader(stream))
                    {
                        text = streamReader.ReadToEnd();
                    }
                }
            }
            return text;
        }

        public static string SaveTextFileContents(IFileSystem fs, string filePath, string text)
        {
            // write the content to the file
            fs.WriteFile(filePath, new MemoryStream(Encoding.UTF8.GetBytes(text)));

            // return the public URL
            string publicUrl = null;
            var webAccessible = fs as IWebAccessible;
            if (webAccessible != null)
                publicUrl = webAccessible.GetPublicURL(filePath);
            return publicUrl;
        }
    }
}
