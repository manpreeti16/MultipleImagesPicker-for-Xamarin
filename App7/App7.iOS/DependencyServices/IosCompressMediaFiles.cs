using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.DependencyServices;
using App7.iOS.DependencyServices;
using AVFoundation;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(IosCompressMediaFiles))]
namespace App7.iOS.DependencyServices
{
    public class IosCompressMediaFiles : ICompressMediaFile
    {
        public object CompressVideoBytes(object sourceFilePath)
        {
            try
            {
                var inputSourceFilePath = sourceFilePath as string;
                var lastindexOfSlash = inputSourceFilePath.LastIndexOf('/');
                var lastIndexOfDot = inputSourceFilePath.LastIndexOf('.');
                var newnameWithoutExtension = inputSourceFilePath.Substring(0, lastIndexOfDot);
                var newname = newnameWithoutExtension.Substring(lastindexOfSlash + 1);
                string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string downloadFilePath = Path.Combine(downloadPath, newname + "_compressedvideo.mp4");

                var asset = AVAsset.FromUrl(NSUrl.FromFilename(inputSourceFilePath));

                AVAssetExportSession export = new AVAssetExportSession(asset, AVAssetExportSession.PresetLowQuality);

                export.OutputUrl = NSUrl.FromFilename(downloadFilePath);
                export.OutputFileType = AVFileType.Mpeg4;
                export.ShouldOptimizeForNetworkUse = true;

                export.ExportAsynchronously(() =>
                {
                    if (export.Error != null)
                        System.Diagnostics.Debug.WriteLine(export.Error.LocalizedDescription);
                });
                return export.OutputUrl.AbsoluteString;
            }
            catch (Exception )
            {
            }
            return string.Empty;
        }

        public byte[] GetCompressedBytes(FileStream fs)
        {
            byte[] compresseBytes = null;
            return compresseBytes;
        }
        
    }
}