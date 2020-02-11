using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.DependencyServices;
using App7.iOS.DependencyServices;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(IosSelectMultipleImages))]
namespace App7.iOS.DependencyServices
{
    public class IosSelectMultipleImages : ISelectMultipleImages
    {
        public async Task OpenGallery()
        {
            try
            {
                var picker = ELCImagePickerViewController.Instance;
                picker.MaximumImagesCount = 15;
                nfloat compressionQuality = 0.5f;
                var topController = UIApplication.SharedApplication.KeyWindow.RootViewController;
                while (topController.PresentedViewController != null)
                {
                    topController = topController.PresentedViewController;
                }
                topController.PresentViewController(picker, true, null);

                await picker.Completion.ContinueWith(t =>
                {
                    picker.BeginInvokeOnMainThread(() =>
                    {
                        picker.DismissViewController(true, null);

                        if (!t.IsCanceled && t.Exception == null)
                        {
                            Dictionary<string, object> dataObj = new Dictionary<string, object>();
                            var items = t.Result as List<AssetResult>;
                            items.ForEach(item =>
                            {
                                var path = Save(item.Image, item.Name);

                                using (NSData imageData = item.Image.AsJPEG(compressionQuality))
                                {
                                    byte[] myByteArray = imageData.ToArray();
                                    dataObj.Add(path, myByteArray);
                                }
                            });

                            MessagingCenter.Send((App)Xamarin.Forms.Application.Current, "ImagesSelected", dataObj);
                        }
                    });
                });
            }
            catch (Exception)
            {

            }
        }

        string Save(UIImage image, string name)
        {
            try
            {
                var documentsDirectory = Environment.GetFolderPath
                                  (Environment.SpecialFolder.Personal);
                string jpgFilename = System.IO.Path.Combine(documentsDirectory, name);
                NSData imgData = image.AsJPEG();
                NSError err = null;
                if (imgData.Save(jpgFilename, false, out err))
                {
                    return jpgFilename;
                }
                else
                {
                    Console.WriteLine("NOT saved as " + jpgFilename + " because" + err.LocalizedDescription);
                    return null;
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
}