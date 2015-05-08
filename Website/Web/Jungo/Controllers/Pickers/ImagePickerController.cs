using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Models;
using N2.Edit.FileSystem;
using N2.Interfaces;
using N2.Web.Drawing;
using DependencyResolver = Jungo.Infrastructure.DependencyResolver;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Pickers
{
    public class ImagePickerController : Controller
    {
        [HttpGet]
        public ActionResult Index(string id)
        {
            ImagePickerViewModel viewModel = GetFilesInternal(id, "/upload");
            SortImages(SortOrder.DateDesc, viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public JsonResult GetFiles(string id, FormCollection collection)
        {
            string virtualPath = collection["path"];
            ImagePickerViewModel viewModel = GetFilesInternal(id, virtualPath);
            SortImages(GetSortOrder(collection), viewModel);
            return Json(viewModel);
        }

        [HttpPost]
        public JsonResult Search(string id, FormCollection collection)
        {
            string virtualPath = collection["virtualPath"];
            string searchCriteria = collection["searchCriteria"];
            searchCriteria = searchCriteria.ToLower(Thread.CurrentThread.CurrentCulture);
            var viewModel = new ImagePickerViewModel(id, virtualPath);
            RecursiveSearch(N2.Context.Current.Resolve<IFileSystem>(), searchCriteria, virtualPath, viewModel);
            SortImages(GetSortOrder(collection), viewModel);
            return Json(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            string virtualPath = HttpContext.Request.Form["virtualPath"];
            string filePath = virtualPath.TrimEnd('/') + "/" + file.FileName;
            N2.Context.Current.Resolve<IFileSystem>().WriteFile(filePath, file.InputStream);
            ImagePickerViewModel viewModel = GetFilesInternal("0", virtualPath);
            SortImages(SortOrder.DateDesc, viewModel);
            return View("Index", viewModel);
        }

        #region implementation

        private void RecursiveSearch(IFileSystem fs, string searchCriteria, string virtualPath, ImagePickerViewModel viewModel)
        {
            foreach (var file in fs.GetFiles(virtualPath))
                if (file.Name.ToLower(Thread.CurrentThread.CurrentCulture).Contains(searchCriteria))
                    viewModel.Images.Add(FileToImageViewModel(file));
            foreach (var dir in fs.GetDirectories(virtualPath))
                if (dir.VirtualPath != virtualPath)
                    RecursiveSearch(fs, searchCriteria, dir.VirtualPath, viewModel);
        }

        private ImageViewModel FileToImageViewModel(FileData file)
        {
            string url = null;
            var extension = VirtualPathUtility.GetExtension(file.Name);
            string path = file.VirtualPath;
            if (!String.IsNullOrEmpty(path))
            {
                if (path.Substring(0, 1) != "/")
                    path = "/" + path;
                path = "~" + path;
            }
            if (ImagesUtility.GetExtensionGroup(extension) == ImagesUtility.ExtensionGroups.Images)
                url = DependencyResolver.Current.Get<IExternalWebLinkResolver>().GetPublicUrl(path);
            return new ImageViewModel(file.Name, path, "f")
                { ImageUrl = url, FileDate = file.Created, FileDateStr = file.Created.ToShortDateString() };
        }

        private ImageViewModel DirectoryToImageViewModel(DirectoryData directory)
        {
            return new ImageViewModel(directory.Name, directory.VirtualPath, "d");
        }

        private ImagePickerViewModel GetFilesInternal(string id, string virtualPath)
        {
            var viewModel = new ImagePickerViewModel(id, virtualPath);
            var fs = N2.Context.Current.Resolve<IFileSystem>();

            foreach (DirectoryData directory in fs.GetDirectories(virtualPath))
                viewModel.Images.Add(DirectoryToImageViewModel(directory));
            foreach (FileData file in fs.GetFiles(virtualPath))
                viewModel.Images.Add(FileToImageViewModel(file));
            return viewModel;
        }

        private enum SortOrder
        {
            None,
            DateAsc,
            DateDesc
        }

        private SortOrder GetSortOrder(FormCollection collection)
        {
            var sortOrder = SortOrder.None;
            int sortOrderInt;
            if (Int32.TryParse(collection["sortOrder"], out sortOrderInt))
                sortOrder = (SortOrder)sortOrderInt;
            return sortOrder;
        }

        private class DateAscComparer : Comparer<ImageViewModel>
        {
            public override int Compare(ImageViewModel x, ImageViewModel y)
            {
                return x.FileDate.CompareTo(y.FileDate);
            }

        }

        private class DateDescComparer : Comparer<ImageViewModel>
        {
            public override int Compare(ImageViewModel x, ImageViewModel y)
            {
                return y.FileDate.CompareTo(x.FileDate);
            }

        }

        private void SortImages(SortOrder so, ImagePickerViewModel viewModel)
        {
            switch (so)
            {
                case SortOrder.DateAsc:
                    viewModel.Images.Sort(new DateAscComparer());
                    break;
                case SortOrder.DateDesc:
                    viewModel.Images.Sort(new DateDescComparer());
                    break;
            }
        }

        #endregion
    }
}
