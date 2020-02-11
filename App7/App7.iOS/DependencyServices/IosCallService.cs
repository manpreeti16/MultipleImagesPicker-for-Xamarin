using App7.DependencyServices;
using App7.iOS.DependencyServices;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(IosCallService))]
namespace App7.iOS.DependencyServices
{
    public class IosCallService: IPhoneCall
    {
        public void OpenCallAction(string phoneNumber)
        {
            var phonenumbers = phoneNumber.Split(',');
            var url = new NSUrl("tel:" + phonenumbers[0]);
            if (!UIApplication.SharedApplication.OpenUrl(url))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var av = new UIAlertView("Not supported",
                             "Scheme 'tel:' is not supported on this device",
                             null,
                             "OK",
                             null);
#pragma warning restore CS0618 // Type or member is obsolete
                av.Show();
            };
        }
    }
}