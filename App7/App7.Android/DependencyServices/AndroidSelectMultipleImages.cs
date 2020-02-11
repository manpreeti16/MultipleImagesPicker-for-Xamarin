using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Widget;
using App7.DependencyServices;
using App7.Droid.DependencyServices;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidSelectMultipleImages))]
namespace App7.Droid.DependencyServices
{
    public class AndroidSelectMultipleImages : ISelectMultipleImages
    {
        public async Task OpenGallery()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                if (status != PermissionStatus.Granted)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Plugin.Permissions.Abstractions.Permission.Storage });
                    status = results[Plugin.Permissions.Abstractions.Permission.Storage];
                }
                if (status == PermissionStatus.Granted)
                {
                    var imageIntent = new Intent(
                        Intent.ActionPick);
                    imageIntent.SetType("image/*");
                    imageIntent.PutExtra(Intent.ExtraAllowMultiple, true);
                    imageIntent.SetAction(Intent.ActionGetContent);
                    ((Activity)Forms.Context).StartActivityForResult(
                        Intent.CreateChooser(imageIntent, "Select photo"), MainActivity.OPENGALLERYCODE);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Toast.MakeText(Xamarin.Forms.Forms.Context, "Error. Can not continue, try again.", ToastLength.Long).Show();
            }
        }
    }
}