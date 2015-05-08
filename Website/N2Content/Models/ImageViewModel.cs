using System;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models
{
    [Serializable]
	public class ImageViewModel
	{
        public ImageViewModel(string fileName, string filePath, string fileType)
        {
            FileName = fileName;
            FilePath = filePath;
            FileType = fileType;
        }

        public string FileName { get; set; }
		public string FilePath { get; set; }
        public string FileType { get; set; }
        public string ImageUrl { get; set; }
        public DateTime FileDate { get; set; }
        public string FileDateStr { get; set; }
	}
}
