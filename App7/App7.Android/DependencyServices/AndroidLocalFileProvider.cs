using System;
using System.IO;
using System.Linq;
using App7.DependencyServices;
using App7.Droid.DependencyServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidLocalFileProvider))]
namespace App7.Droid.DependencyServices
{
    public class AndroidLocalFileProvider : ILocalFileProvider
    {
        public void DeleteMediaFiles()
        {
            try
            {
                var filesPath = Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;
                DirectoryInfo filesDirectory = new DirectoryInfo(filesPath);
                string[] extensions = new string[] { ".3gp", ".mp4", ".mkv", ".webm", ".bmp", ".gif", ".jpg", ".png", ".webp", ".heif" };
                foreach (var folders in filesDirectory.GetDirectories())
                {
                    DeleteFilesFromFolder(extensions, folders);

                    var getFolder = folders.GetDirectories();
                    if (getFolder.Count() > 0)
                    {
                        foreach (var item in getFolder)
                        {
                            DeleteFilesFromFolder(extensions, item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void DeleteFilesFromFolder(string[] extensions, DirectoryInfo folders)
        {
            foreach (var file in folders.GetFiles())
            {
                if (extensions.Contains(file.Extension.ToLower()))
                    file.Delete();
            }
        }

    }
}