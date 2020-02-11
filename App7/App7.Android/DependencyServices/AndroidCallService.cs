using System;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Support.V4.App;
using App7.DependencyServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(App7.Droid.DependencyServices.AndroidCallService))]
namespace App7.Droid.DependencyServices
{
   public class AndroidCallService: IPhoneCall
    {
        public void OpenCallAction(string phoneNumber)
        {
            try
            {
                var phonenumbers = phoneNumber.Split(',');
                var uri = Android.Net.Uri.Parse(String.Format("tel:{0}", phonenumbers[0]));
                var intent = new Intent(Intent.ActionCall, uri);
                if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(Android.App.Application.Context, Manifest.Permission.CallPhone) != (int)Permission.Granted)
                {
                    ShowExplanation("Permission Needed", "Allow app to make phone calls.");
                }
                else
                {
                    Forms.Context.StartActivity(intent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ShowExplanation(string title, string message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder((Activity)Forms.Context);
            builder.SetTitle(title).SetMessage(message).SetCancelable(false).
                SetPositiveButton("OK", MyDialogHandlerForPermission).Show();
        }

        private void MyDialogHandlerForPermission(object sender, DialogClickEventArgs e)
        {
            //Ask permission
            ActivityCompat.RequestPermissions((Activity)Forms.Context, new string[] { Manifest.Permission.CallPhone }, 0);
        }
    }
}