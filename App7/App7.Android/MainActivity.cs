using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;
using Plugin.Media;
using Android.Database;
using Android.Provider;
using System.IO;
using Android.Graphics;
using Android.Content;
using System.Collections.Generic;
using Xamarin.Forms;

namespace App7.Droid
{
    [Activity(Label = "App7", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public const int OPENGALLERYCODE = 100;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

         
            try
            {
                base.OnCreate(savedInstanceState);
                CrossCurrentActivity.Current.Init(this, savedInstanceState);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);
                //  await CrossMedia.Current.Initialize();
            }
            catch (Exception ex)
            {

                throw;
            }
            
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            try
            {
                if (requestCode == OPENGALLERYCODE && resultCode == Result.Ok)
                {
                    Dictionary<string, object> dataObj = new Dictionary<string, object>();
                    if (data != null)
                    {
                        ClipData clipData = data.ClipData;
                        dataObj = clipData != null ? ConvertClipDataToBytes(dataObj, clipData) : ConvertDataToBytes(data, dataObj);
                        MessagingCenter.Send<App, Dictionary<string, object>>((App)Xamarin.Forms.Application.Current, "ImagesSelected", dataObj);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private Dictionary<string, object> ConvertDataToBytes(Intent data, Dictionary<string, object> dataObj)
        {
            Android.Net.Uri uri = data?.Data;
            if (uri == null)
            {
                Toast.MakeText(Xamarin.Forms.Forms.Context, "Unable to get path of image from this location. Please try some other folder.", ToastLength.Long).Show();
                return dataObj;
            }
            var path = GetRealPathFromURI(uri);
            var bytes = ConvertImageToByte(uri);
            if (path != null)
            {
                dataObj.Add(path, bytes);
            }
            return dataObj;
        }

        private Dictionary<string, object> ConvertClipDataToBytes(Dictionary<string, object> dataObj, ClipData clipData)
        {
            for (int i = 0; i < clipData.ItemCount; i++)
            {
                ClipData.Item item = clipData.GetItemAt(i);
                Android.Net.Uri uri = item?.Uri;
                if (uri == null)
                {
                    Toast.MakeText(Xamarin.Forms.Forms.Context, "Unable to get path of image from this location. Please try some other folder.", ToastLength.Long).Show();
                    return dataObj;
                }
                var path = GetRealPathFromURI(uri);
                var bytes = ConvertImageToByte(uri);
                if (path != null)
                {
                    dataObj.Add(path, bytes);
                }
            }
            return dataObj;
        }

        private byte[] ConvertImageToByte(Android.Net.Uri uri)
        {
            Bitmap bitmap = null;
            byte[] byteArray, byteArrayCompressed = null;
            try
            {
                Stream stream = ContentResolver.OpenInputStream(uri);
                using (var memoryStream = new MemoryStream())
                using (var toMemoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    byteArray = memoryStream.ToArray();
                    bitmap = BitmapFactory.DecodeByteArray(byteArray, 0, byteArray.Length);
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 50, toMemoryStream);
                    byteArrayCompressed = toMemoryStream.ToArray();
                }
                stream.Flush();
                stream.Close();
                return byteArrayCompressed;
            }
            catch (Exception ex)
            {
             }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Recycle();
                    bitmap = null;
                }
            }
            return byteArrayCompressed;
        }

        private String GetRealPathFromURI(Android.Net.Uri contentURI)
        {
            try
            {
                ICursor imageCursor = null;
                string fullPathToImage = "";

                imageCursor = ContentResolver.Query(contentURI, null, null, null, null);
                imageCursor.MoveToFirst();
                int idx = imageCursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);

                if (idx != -1)
                {
                    fullPathToImage = imageCursor.GetString(idx);
                }
                else
                {
                    ICursor cursor = null;
                    var docID = DocumentsContract.GetDocumentId(contentURI);
                    var id = docID.Split(':')[1];
                    var whereSelect = MediaStore.Images.ImageColumns.Id + "=?";
                    var projections = new string[] { MediaStore.Images.ImageColumns.Data };

                    cursor = ContentResolver.Query(MediaStore.Images.Media.InternalContentUri, projections, whereSelect, new string[] { id }, null);
                    if (cursor.Count == 0)
                    {
                        cursor = ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, projections, whereSelect, new string[] { id }, null);
                    }
                    var colData = cursor.GetColumnIndexOrThrow(MediaStore.Images.ImageColumns.Data);
                    cursor.MoveToFirst();
                    fullPathToImage = cursor.GetString(colData);
                }
                return fullPathToImage;
            }
            catch (Exception ex)
            {
                Toast.MakeText(Xamarin.Forms.Forms.Context, "Unable to get path of image from this location. Please try some other folder.", ToastLength.Long).Show();
               
                //to handle in exception handling
            }
            return null;
        }
    }
}