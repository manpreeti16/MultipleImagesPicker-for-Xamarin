using System;
using System.IO;
using System.Linq;
using App7.DependencyServices;
using App7.iOS.DependencyServices;
using Xamarin.Forms;

[assembly:Dependency(typeof(IosLocalFileProvider))]
namespace App7.iOS.DependencyServices
{
    public class IosLocalFileProvider : ILocalFileProvider
    {
        public void DeleteMediaFiles()
        {
            try
            {
                string[] extensions = new string[] {"mpeg", "3gp","x-emf","x-wmf", "x-jg", "x-xbitmap", "avi",
                "x-png", "pjpeg", "tga","jpeg","mov","mp4", "mkv","heic",
                "webm","psd","sgi","tiff","bmp","gif","jpg","png","webp","heif" };

                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var alldirectories = Directory.GetDirectories(documents);
                foreach (var item in alldirectories)
                {
                    if (item.Contains("/temp"))
                    {
                        DeleteFilesFromFolder(extensions, item);
                    }
                }

                DeleteFilesFromFolder(extensions, documents);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void DeleteFilesFromFolder(string[] extensions, string item)
        {
            var tempfiles = Directory.EnumerateFiles(item);
            foreach (var file in tempfiles)
            {
                if (extensions.Contains(file.Split('.').Last().ToLower()))
                    File.Delete(file);
            }
        }
    }
}