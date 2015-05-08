using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.UI.WebControls;

namespace N2.Details
{
	/// <summary>
	/// Allows the selection of views.
	/// 
	/// inspired by EditableThemeSelectionAttribute.cs, author sweber@digitalriver.com
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class EditableViewSelectionAttribute : EditableListControlAttribute
	{
		public string ViewDirectory
		{
			get;
			set;  
		}

		public EditableViewSelectionAttribute()
			: this("View Template", 14, "~/Views/Shared")
		{
		}

		public EditableViewSelectionAttribute(string title, int sortOrder, string viewDirectory)
			: base(title, sortOrder)
		{
			ViewDirectory = viewDirectory;
		}

		protected override ListControl CreateEditor()
		{
			return new DropDownList();
		}

		protected override ListItem[] GetListItems()
		{
			var path = GetViewDirectory();

		    return (from filePath in Directory.GetFiles(path)
                    select Path.GetFileNameWithoutExtension(filePath) into fileName
                    where !fileName.StartsWith(".") select new ListItem(fileName)).ToArray();
		}

		private string GetViewDirectory()
		{
			return HostingEnvironment.MapPath(ViewDirectory);
		}

#if false
		private IEnumerable<string> GetDirectories(string path)
		{
			if (Directory.Exists(path))
			{
				foreach (string directoryPath in Directory.GetDirectories(path))
				{
					string directoryName = Path.GetFileName(directoryPath);
					if (!directoryName.StartsWith("."))
						yield return directoryPath;
				}
			}
		}
#endif
	}
}
