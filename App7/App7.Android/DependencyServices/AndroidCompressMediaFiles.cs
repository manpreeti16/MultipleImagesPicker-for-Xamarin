using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using App7.DependencyServices;
using App7.Droid.DependencyServices;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidCompressMediaFiles))]
namespace App7.Droid.DependencyServices
{
    public class AndroidCompressMediaFiles : ICompressMediaFile
    {
        public object CompressVideoBytes(object sourceFilePath)
        {
            return sourceFilePath;
        }

        public byte[] GetCompressedBytes(FileStream fs)
        {
            Bitmap bitmap = null;
            byte[] bitmapData = null;

            try
            {
                byte[] byteArray = new byte[fs.Length];
                fs.Read(byteArray, 0, (int)fs.Length);
                using (var stream = new MemoryStream())
                {
                    bitmap = BitmapFactory.DecodeByteArray(byteArray, 0, byteArray.Length);
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 50, stream);
                    bitmapData = stream.ToArray();
                }
                return bitmapData;
            }
            catch (Exception)
            {
               return bitmapData;
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Recycle();
                    bitmap = null;
                }
            }
        }

      
    }
}