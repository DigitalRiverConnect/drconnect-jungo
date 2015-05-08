using System;
using System.Collections.Generic;
using System.Linq;
using N2.Edit.FileSystem.Items;
using N2.Engine;
using N2.Edit.FileSystem;
using N2.Edit;
using N2.Persistence;
using N2.Collections;
using N2.Security;

namespace N2.Management.Files
{
	/// <summary>
	/// A custom <see cref="INodeProvider" /> that enumerates the FileSystem as special ContentItem instances
	/// </summary>
	[Service]
	public class FolderNodeProvider : INodeProvider
	{
		private readonly IFileSystem _fs;
		private readonly IRepository<ContentItem> _repository;
		private readonly IDependencyInjector _dependencyInjector;

		internal FolderReference[] UploadFolderPaths { get; set; }

		public FolderNodeProvider(IFileSystem fs, IRepository<ContentItem> repository, IDependencyInjector dependencyInjector)
		{
            UploadFolderPaths = new FolderReference[0];
			this._fs = fs;
			this._repository = repository;
			this._dependencyInjector = dependencyInjector;
		}


		#region INodeProvider Members

		public ContentItem Get(string path)
		{
		    return (from pair in UploadFolderPaths where path.StartsWith(pair.Path, StringComparison.InvariantCultureIgnoreCase) 
                    let dir = CreateDirectory(pair) 
                    let remaining = path.Substring(pair.Path.Length) 
                    select string.IsNullOrEmpty(remaining) ? dir : dir.GetChild(remaining)).FirstOrDefault();
		}

	    public IEnumerable<ContentItem> GetChildren(string path)
		{
			foreach (var pair in UploadFolderPaths)
			{
				if (pair.ParentPath.Equals(path, StringComparison.InvariantCultureIgnoreCase))
				{
					yield return CreateDirectory(pair);
				}
                else if (path.StartsWith(pair.Path, StringComparison.InvariantCultureIgnoreCase))
				{
					ContentItem dir = CreateDirectory(pair);

                    var remaining = path.Substring(pair.Path.Length);
                    if (!string.IsNullOrEmpty(remaining))
					{
                        dir = dir.GetChild(remaining);
					}

                    if (dir != null)
                    {
						foreach (var child in dir.GetChildren(new NullFilter()))
						{
							yield return child;
						}
                    }
				}
			}
		}

        public bool HasChildren(string path)
        {
            foreach (var pair in UploadFolderPaths)
            {
                if (pair.ParentPath.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                    return true;

                if (pair.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                    return true;

                if (path.StartsWith(pair.Path, StringComparison.InvariantCultureIgnoreCase))
                {
                    ContentItem dir = CreateDirectory(pair);

                    var remaining = path.Substring(pair.Path.Length);
                    if (!string.IsNullOrEmpty(remaining))
                    {
                        dir = dir.GetChild(remaining);
                    }

                    if (dir != null)
                    {
                        return dir.GetChildren().Any();
                    } 
                } 
            }
            return false;
        }

        #endregion

        #region Private & Internal Members

        private Directory CreateDirectory(FolderReference pair)
		{
			return CreateDirectory(pair.Folder, _fs, _repository, _dependencyInjector);
		}

		internal static Directory CreateDirectory(FileSystemRoot folder, IFileSystem fs, IRepository<ContentItem> persister, IDependencyInjector dependencyInjector)
		{
			var dd = fs.GetDirectoryOrVirtual(folder.Path);
			var parent = persister.Get(folder.GetParentID());

			var dir = Directory.New(dd, parent, dependencyInjector);
			dir.Name = folder.GetName();
			dir.Title = folder.Title ?? dir.Name;
			dir.UrlPrefix = folder.UrlPrefix;

			Apply(folder.Readers, dir);
			Apply(folder.Writers, dir);

			return dir;
		}

		private static void Apply(PermissionMap map, Directory dir)
		{
			if (map.IsAltered)
				DynamicPermissionMap.SetRoles(dir, map.Permissions, map.Roles);
			else
				DynamicPermissionMap.SetAllRoles(dir, map.Permissions);
		}

		#endregion
    }
}
