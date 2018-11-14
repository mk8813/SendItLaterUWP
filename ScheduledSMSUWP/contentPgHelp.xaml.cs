using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ScheduledSMSUWP
{
    public sealed partial class contentPgHelp : ContentDialog
    {

       
        public contentPgHelp()
        {
            this.InitializeComponent();
           
            lblVersion.Text = "Version " + GetAppVersion();
            lblCopyright.Text = "Copyright © " + DateTime.Now.Year.ToString() + " Mehdi Kheirandish";
        }


        public string GetAppVersion()
        {

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);

        }


   

        private async void btnRate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?PFN={Package.Current.Id.FamilyName}"));
            }
            catch (Exception)
            {


            }
        }

        private async void btnFeedback_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();

                emailMessage.Subject = "Sendenarium Feedback";
                var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient("mk8813@hotmail.com");
                emailMessage.To.Add(emailRecipient);
                await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);

            }
            catch (Exception)
            {


            }
        }



        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
           
            
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void txtMoreApps_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
          

        }

        private void txtMoreApps_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);

        }

        private async void txtMoreApps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"ms-windows-store://publisher/?name=Mehdi Kheirandish"));
            }
            catch (Exception)
            {


            }
        }
    }
}
