using App7.DependencyServices;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App7.ViewModels
{
    public class MyCameraViewModel : NotifyViewModel
    {
        private readonly ICommand callCommand, mailCommand;
        private readonly Page page;

        #region Properties
        private string completePath;
        public string CompletePath
        {
            get => completePath;
            set
            {
                completePath = value;
                NotifyPropertyChanged();
            }
        }

        private string originalBytes;
        public string OriginalBytes
        {
            get => originalBytes;
            set
            {
                originalBytes = value;
                NotifyPropertyChanged();
            }
        }

        private string compressedBytes;
        public string CompressedBytes
        {
            get => compressedBytes;
            set
            {
                compressedBytes = value;
                NotifyPropertyChanged();
            }
        }
        #endregion Properties

        #region Constructor
        public MyCameraViewModel(Page page)
        {
            this.page = page;
            callCommand = new Command(OnMakeCall);
            mailCommand = new Command<object>(async (x) => await OnSendMail(x));
            SubscribeToImagesSelected();
        }
        #endregion Constructor

        #region Command
        public ICommand CallCommand
        {
            get { return callCommand; }
        }
        public ICommand MailCommand
        {
            get { return mailCommand; }
        }
        public Command TakePicture
        {
            get
            {
                return new Command(async () =>
                {
                    try
                    {
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            await GetPermissionAndOpenCameraForIos();
                        }
                        else
                        {
                            await ClickPhotoAndSave();
                        }
                    }
                    catch (MediaPermissionException)
                    {
                        await HandleMediaPermissionException();
                    }
                    catch (Exception)
                    {
                    }
                });
            }
        }
        public Command SelectVideo
        {
            get
            {
                return new Command(async () =>
                {
                    try
                    {
                        if (!CrossMedia.Current.IsPickVideoSupported)
                        {
                            await page.DisplayAlert("Not Supported", "Video not supported", "Ok");
                            return;
                        }

                        var file = await CrossMedia.Current.PickVideoAsync();

                        if (file == null)
                            return;

                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            long lengthOfVideo = 0;
                            var returnedObject = DependencyService.Get<ICompressMediaFile>().CompressVideoBytes(file.Path);
                            var newPath = returnedObject as string;

                            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            var files = Directory.GetFiles(documents);
                            var filenameToSearch = newPath.Replace("file://", "");
                           

                            if (files != null && files.Count() > 0)
                            {
                                var mediaFile = files.FirstOrDefault(x => x.Contains(filenameToSearch));
                                if (mediaFile != null)
                                {
                                    FileInfo fileInfo = new FileInfo(mediaFile); //desired file
                                    lengthOfVideo = fileInfo.Length;
                                }
                            }

                            OriginalBytes = new FileInfo(file.Path).Length.ToString();
                            CompressedBytes = lengthOfVideo.ToString();
                        }
                        else
                        {

                            OriginalBytes = new FileInfo(file.Path).Length.ToString();
                            CompressedBytes = "no compression applied";
                        }
                        CompletePath = file.Path;
                    }
                    catch (MediaPermissionException)
                    {
                        await HandleMediaPermissionException();
                    }
                    catch (Exception)
                    {

                    }
                });
            }
        }
        public Command RecordVideo
        {
            get
            {
                return new Command(async () =>
                {
                    try
                    {
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            await GetPermissionAndOpenVideoCameraForIos();
                        }
                        else
                        {
                            await TakeVideoAndSave();
                        }
                    }
                    catch (MediaPermissionException)
                    {
                        await HandleMediaPermissionException();
                    }
                    catch (Exception)
                    {
                    }
                });
            }
        }
        public Command SelectPicture
        {
            get
            {
                return new Command(async () =>
                {
                    try
                    {
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            await GetPermissionAndOpenGalleryForIos();
                        }
                        else
                        {
                            await DependencyService.Get<ISelectMultipleImages>().OpenGallery();
                            await HandleMediaPermissionException();
                        }
                    }
                    catch (Exception)
                    {
                    }
                });
            }
        }
        #endregion Command      

        #region Private Methods
        private void OnMakeCall(object obj)
        {
            string phoneNo = obj as string;
            DependencyService.Get<IPhoneCall>().OpenCallAction(phoneNo);
        }
        private async Task OnSendMail(object obj)
        {
            string mailID = obj as string;
            var mailIdList = new List<string> { mailID };
            await SendEmail(mailIdList);
        }
        private async Task SendEmail(List<string> recipients)
        {
            try
            {
                var message = new EmailMessage
                {
                    To = recipients,
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                Console.WriteLine("FeatureNotSupportedException");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private async Task GetPermissionAndOpenCameraForIos()
        {
            try
            {
                var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                var photosStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos);

                if (cameraStatus != PermissionStatus.Granted || photosStatus != PermissionStatus.Granted)
                {
                    if (cameraStatus == PermissionStatus.Unknown || photosStatus == PermissionStatus.Unknown)
                    {
                        var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Photos });
                        cameraStatus = results[Permission.Camera];
                        photosStatus = results[Permission.Photos];
                    }
                    else
                    {
                        await page.DisplayAlert("Permission Needed", "Please allow Photos and Camera permission from settings", "Ok");
                        CrossPermissions.Current.OpenAppSettings();
                    }
                }
                else if (cameraStatus == PermissionStatus.Granted && photosStatus == PermissionStatus.Granted)
                {
                    await ClickPhotoAndSave();
                }
            }
            catch (Exception ex)
            {

               
            }

          
        }
        private async Task ClickPhotoAndSave()
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await page.DisplayAlert("Not Supported", "Camera not supported", "Ok");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Name = "clickedimage.jpg",
                CompressionQuality = 50 //compressing image
            });

            if (file == null)
                return;

            CompletePath = file.Path;
            OriginalBytes = "Image clicked with compression, so no original bytes";
            FileInfo fileInfo = new FileInfo(file.Path);
            CompressedBytes = fileInfo.Length.ToString();
        }
        private async Task GetPermissionAndOpenGalleryForIos()
        {
            var photosStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos);
            if (photosStatus != PermissionStatus.Granted)
            {
                if (photosStatus == PermissionStatus.Unknown)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Photos });
                    photosStatus = results[Permission.Photos];
                }
                else
                {
                    await page.DisplayAlert("Permission Needed", "Please allow Photos permission from settings", "Ok");
                    CrossPermissions.Current.OpenAppSettings();
                }
            }
            else if (photosStatus == PermissionStatus.Granted)
            {
                await DependencyService.Get<ISelectMultipleImages>().OpenGallery();
            }
        }
        private async Task TakeVideoAndSave()
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakeVideoSupported)
            {
                await page.DisplayAlert("Not Supported", "Camera not supported", "Ok");
                return;
            }

            var file = await CrossMedia.Current.TakeVideoAsync(new StoreVideoOptions
            {
                Name = "capturedvideo.mp4",
                Quality = VideoQuality.Low //compressed
            });

            if (file == null)
                return;

            CompletePath = file.Path;
            OriginalBytes = "Video recorded with compression, so no original bytes";
            FileInfo fileInfo = new FileInfo(file.Path);
            CompressedBytes = fileInfo.Length.ToString();
        }
        private async Task GetPermissionAndOpenVideoCameraForIos()
        {
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            var photosStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos);
            var microphoneStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Microphone);

            if (cameraStatus != PermissionStatus.Granted || photosStatus != PermissionStatus.Granted || microphoneStatus != PermissionStatus.Granted)
            {
                if (cameraStatus == PermissionStatus.Unknown || photosStatus == PermissionStatus.Unknown || microphoneStatus == PermissionStatus.Unknown)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Photos, Permission.Microphone });
                    cameraStatus = results[Permission.Camera];
                    photosStatus = results[Permission.Photos];
                    microphoneStatus = results[Permission.Microphone];
                }
                else
                {
                    await page.DisplayAlert("Permission Needed", "Please allow Photos, Camera and Microphone permission from settings", "Ok");
                    CrossPermissions.Current.OpenAppSettings();
                }
            }
            else if (cameraStatus == PermissionStatus.Granted && photosStatus == PermissionStatus.Granted && microphoneStatus == PermissionStatus.Granted)
            {
                await TakeVideoAndSave();
            }
        }
        private async Task HandleMediaPermissionException()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            var showPermissionRationale = await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage);
            if (status != PermissionStatus.Granted && !showPermissionRationale)
            {
                bool result = await page.DisplayAlert("Permission Needed", "Please enable storage permission from app settings", "Ok", "Cancel");
                if (result)
                    CrossPermissions.Current.OpenAppSettings();
            }
        }
        private void SubscribeToImagesSelected()
        {
            try
            {
                MessagingCenter.Subscribe<App, Dictionary<string, object>>((App)Xamarin.Forms.Application.Current, "ImagesSelected", (s, images) =>
                {
                    var listOfImagesSelected = images;

                    foreach (var item in listOfImagesSelected)
                    {
                        CompletePath = item.Key + Environment.NewLine + CompletePath;
                        OriginalBytes = "returned compressed";
                        byte[] bb = (byte[])item.Value;
                        CompressedBytes = bb.Length + Environment.NewLine + CompressedBytes;

                    }
                });
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

    }
    public class NotifyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected NotifyViewModel()
        {

        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _executeAction;
        private readonly Func<object, bool> _canExecuteAction;

        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public void Execute(object parameter) => _executeAction(parameter);
        public bool CanExecute(object parameter) => _canExecuteAction?.Invoke(parameter) ?? true;

        public event EventHandler CanExecuteChanged;
        public void InvokeCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
