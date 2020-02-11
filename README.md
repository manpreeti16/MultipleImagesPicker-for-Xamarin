# MultipleImagesPicker-for-Xamarin
Multiple images picker for Xamarin, capture video or images, select video from gallery, make phone call or send email.

![alt text](https://github.com/manpreeti16/MultipleImagesPicker-for-Xamarin/blob/master/multipleimagesselected.jpeg)

# Purpose
App can be used to select multiple images from a phone for Android and iOS. Select a video from phone. Capture a photo or video. 
All these will be compressed, so that it is easy to send them over the network as original bytes may be very large.

# Features
Below are the details of features in the app:
1. Background worker: To delete last stored photos/videos files from app storage, so that app size does not increases as we continue to capture more photos and videos.
2. Files stored within app can be found at this location(file manager/Internal Storage/Android/data/com.companyname.App7/files/Movies or Pictures)
3. Images clicked are compressed when stored, so it can be used to send over the network using multipart.
4. Images/Videos picked from gallery are compressed, so it can be used to send over the network using multipart.
5. Videos captured in iOS is compressed.
6. Name of the selected image in iOS will also be displayed, as usually iOS never shows the image name to user.
7. All selected iOS images will be converted and saved to Jpeg while compressing. 
8. *Replace phone number and email in MyCamera.xaml to make phone call and send emails.
9. Permission handling, if user denies the permission it will be asked again and again everytime they use any camera feature.
10. If user says, "Don't ask me again", they will then be redirected to app settings page, where they have to manually give the permission for Storage in Android, Camera, Photos& Microphone in iOS.
11. Clone the code and build it and it is ready to be used in Emulator or devices

# External references used
https://stackoverflow.com/questions/33750101/how-to-select-multiple-images-from-gallery-for-android-and-ios-device-using-xama/34141450#34141450

https://github.com/jamesmontemagno/MediaPlugin
Follow steps mentioned in this to setup camera and video recording in app.

# Screenshots

Permission popup for Phone call
![alt text](https://github.com/manpreeti16/MultipleImagesPicker-for-Xamarin/blob/master/1.jpeg)

Making a phone call
![alt text](https://github.com/manpreeti16/MultipleImagesPicker-for-Xamarin/blob/master/7.jpeg)

Popup if "Don't ask me again" is selected and before redirecting to app settings page
![alt text](https://github.com/manpreeti16/MultipleImagesPicker-for-Xamarin/blob/master/6.jpeg)

Redirected to APP Settings if "Don't ask me again" is selected
![alt text](https://github.com/manpreeti16/MultipleImagesPicker-for-Xamarin/blob/master/2.jpeg)

Multiple Images Picked
![alt text](https://github.com/manpreeti16/MultipleImagesPicker-for-Xamarin/blob/master/4.jpeg)




