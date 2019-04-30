﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using SlideShow_1._1;
using Windows.UI.ViewManagement;

namespace SlideShow_1._0
{
    /// <summary>
    /// The main page ( Shows the picturs ).
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Time to wait between pictures in seconds
        private int WaitTime = 20;

        StorageFile[] pictures;
        int index = -1;

        bool pasue = false;


        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            // load a setting that is local to the device
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            bool? Value1 = localSettings.Values["ShowWelocme"] as bool?;
            if (Value1 == null)
            {
                Welcome dialog = new Welcome();
                dialog.DialogClosed += Dialog_DialogClosed1;
                dialog.ShowAsync();            
            }

            // load a setting that is local to the device
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            int? Value = localSettings.Values["interval"] as int?;
            if (Value != null)
                WaitTime = (int)Value;
            else
            {
                // Save a setting locally on the device
                localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["interval"] = WaitTime;
            }

        }

        private void Dialog_DialogClosed1(object sender, EventArgs e)
        {
            Welcome welcome = sender as Welcome;
            if (welcome.ShowNext)
            {
                // Save a setting locally on the device
                ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["ShowWelocme"] = true ;
            }
        }



        /// <summary>
        /// When Page is Loaded then load pictures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //get pictures folder
                StorageFolder folder = KnownFolders.PicturesLibrary;
                if (folder != null)
                {
                    //Get all the images (jpg , png) in the pictures folder
                    var query = CommonFileQuery.OrderByDate;
                    var queryOptions = new QueryOptions(query, new[] { ".png", ".jpg" });
                    queryOptions.FolderDepth = FolderDepth.Deep;
                    var queryResult = folder.CreateFileQueryWithOptions(queryOptions);
                    IReadOnlyList<StorageFile> files = await queryResult.GetFilesAsync();
                    pictures = files.ToArray();
                }
                this.Focus(FocusState.Keyboard);
                //main loop
                while (true)
                {
                    if(!pasue)
                        ShowNextPicture();
                    await Task.Delay(WaitTime * 1000);
                }
            }
            catch (Exception ex)
            {
                //Show exception message
                image.Visibility = Visibility.Collapsed;
                errorM.Visibility = Visibility.Visible;
                errorM.Text = ex.Message;
            }
        }

        private StorageFile GetNextPicture()
        {
            if(index == pictures.Length-1)
                index = -1;
            return pictures[++index];
    
        }

        private StorageFile GetPrewPicture()
        {
            if (index == 0)
                index = pictures.Length;
            return pictures[--index];

        }


        private async void ShowNextPicture()
        {
            using (IRandomAccessStream fileStream = await GetNextPicture().OpenAsync(FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap 
                BitmapImage bitmapImage = new BitmapImage();
                //Optional set width and height
                bitmapImage.DecodePixelWidth = 1920;
              //  bitmapImage.DecodePixelHeight = 1080;
                await bitmapImage.SetSourceAsync(fileStream);
                image.Source = bitmapImage;
            }
        }

        private async void ShowPrewPicture()
        {
            using (IRandomAccessStream fileStream = await GetPrewPicture().OpenAsync(FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap 
                BitmapImage bitmapImage = new BitmapImage();
                //Optional set width and height
                bitmapImage.DecodePixelWidth = 1920;
                //  bitmapImage.DecodePixelHeight = 1080;
                await bitmapImage.SetSourceAsync(fileStream);
                image.Source = bitmapImage;
            }
        }


        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Right)
            {
                ShowNextPicture();
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Left)
            {
                ShowPrewPicture();
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.F1)
            {
                ChangeTime dialog = new ChangeTime(WaitTime);
                dialog.DialogClosed += Dialog_DialogClosed;
                dialog.ShowAsync();
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.F11)
            {
                var view = ApplicationView.GetForCurrentView();
                if (view.IsFullScreenMode)
                {
                    view.ExitFullScreenMode();
                }
                else
                {
                    var succeeded = view.TryEnterFullScreenMode();
                }
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Space)
            {
                if (pasue == false)
                    pasue = true;
                else
                {
                    pasue = false;
                }
            }

        }

        private void Dialog_DialogClosed(object sender, EventArgs e)
        {
            ChangeTime dialog = sender as ChangeTime;
            if (dialog.Update)
                WaitTime = dialog.IntervalNum;

            // Save a setting locally on the device
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["interval"] = WaitTime;
        }
    }


}
