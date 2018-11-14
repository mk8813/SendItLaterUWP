using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ScheduledSMSUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
        string curBgImg = "";
        private int bgCounter = 1;
        ThreadPoolTimer _clockTimer = null;
      
        public MainPage()
        {
            try
            {
                //AppSettings.Values["HeaderBg"] = "1";
                this.InitializeComponent();
                this.Loaded += MainPage_Loaded;
                
                _clockTimer = ThreadPoolTimer.CreatePeriodicTimer(_clockTimer_Tick, TimeSpan.FromMilliseconds(1000));
                imgHeader.ManipulationMode = ManipulationModes.All;
                if (AppSettings.Values["HeaderBg"]==null)
                {
                    AppSettings.Values["HeaderBg"] = "1";
                }
                curBgImg = AppSettings.Values["HeaderBg"].ToString();

                imgHeader.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/" + curBgImg + ".png", UriKind.Absolute));
               
                bgCounter = int.Parse(curBgImg);

                ///////////////////////
              

            }
            catch (Exception)
            {

                throw;
            }
           

        }

        private async void _clockTimer_Tick(ThreadPoolTimer timer)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtCurrentTime.Text = DateTime.Now.ToString(@"F");

            });

        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //////////////////////////////populate list

            PopulateScheduledList();
            ////////////////////
            if (AppSettings.Values["Warning"] == null)
            {
                ContentDialog warningDialog = new ContentDialog()
                {
                    Title="Important notice",
                    Content = "Due to Universal Windows Platform limitation (UWP Security reason) apps can't send SMS, WhatsApp, Skype and Email messages AUTOMATICALLY. Therefore, final confirmation and sending of messages should be done manually.",
                    PrimaryButtonText = "Got it!",

                };

                ContentDialogResult result = await warningDialog.ShowAsync();
                if (result== ContentDialogResult.Primary)
                {
                    AppSettings.Values["Warning"] = "1";
                }
              
            }

        }

        private void imgHeader_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
              //  imgHeader.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/bg/" + new Random().Next(1,7).ToString()+".png", UriKind.Absolute));
            }
            catch (Exception)
            {

              
            }
        }

        private Point initialpoint;
        private void imgHeader_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            initialpoint = e.Position;

          
        }


        private  void imgHeader_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            if (e.IsInertial)
            {
              
                Point currentpoint = e.Position;
                if (currentpoint.X - initialpoint.X >= 300)//right
                {
               
                    bgCounter--;
                    if (bgCounter<=0)
                    {
                        bgCounter = 7;
                    }
                }
                else if (currentpoint.X - initialpoint.X <= 300)//left
                {

                    bgCounter++;
                    if (bgCounter>=8)
                    {
                        bgCounter =1;
                    }
                }

             
              
                if (bgCounter >=1 && bgCounter <= 7)
                {
                    imgHeader.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/" + bgCounter.ToString() + ".png", UriKind.Absolute));

                }
               
                AppSettings.Values["HeaderBg"] = bgCounter.ToString();
                e.Complete();



            }
        }

        private async  void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                popupAddJob mypopup = new popupAddJob();
                if (await mypopup.ShowAsync()==ContentDialogResult.Primary)
                {
                    PopulateScheduledList();
                }
                pivotMainLists.SelectedIndex = 0;
            }
            catch (Exception)
            {

            }
        }

        protected override  void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadingContentControl.ContentTemplate = Resources["ProgressBarTemplate"] as DataTemplate;
            //await LoadingContentControl.Blur(2, 100).StartAsync();

        }
        private  void PopulateScheduledList()
        {
            try
            {
                 ShowLoadingDialog();

                using (dbHelperConnection db = new dbHelperConnection())
                {
                    var taskslist = db.tbl_ScheduledTasks.Where(t => t.IsScheduled == 1 && t.IsSent == 0).OrderBy(t => t.TargetDate).ThenBy(t => t.TargetTime).Select(t => t);
                    if (taskslist.Count() > 0)
                    {

                        txtNothing.Visibility = Visibility.Collapsed;
                        lstScheduledTasks.ItemsSource = taskslist.ToArray();

                    }
                    else
                    {
                        txtNothing.Visibility = Visibility.Visible;
                        lstScheduledTasks.ItemsSource = null;

                    }

                }

                 HideLoadingDialog();
              
            }
            catch (Exception)
            {


            }
        }

        private void ShowLoadingDialog()
        {
            //await LoadingContentControl.Blur(2, 100).StartAsync();
            LoadingControl.IsLoading = true;



        }

        private void HideLoadingDialog()
        {
            // await LoadingContentControl.Blur(0, 0).StartAsync();
            LoadingControl.IsLoading = false;

        }
        private void btnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var SelectedRecord = ((FrameworkElement)e.OriginalSource).DataContext as tbl_ScheduledTasks;
                using (dbHelperConnection db = new dbHelperConnection())
                {
                    db.tbl_ScheduledTasks.Remove(SelectedRecord);
                    if (db.SaveChanges() > 0)
                    {
                        AlarmHelper.RemoveAlarms(SelectedRecord.Id);
                        if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
                        {
                            Windows.Phone.Devices.Notification.VibrationDevice v = Windows.Phone.Devices.Notification.VibrationDevice.GetDefault();
                            v.Vibrate(TimeSpan.FromMilliseconds(25));
                        }

                        PopulateScheduledList();

                    }

                }
            }
            catch (Exception)
            {

             
            }
        }

        private void btnSort_Click(object sender, RoutedEventArgs e)
        {
        
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (pivotMainLists.SelectedIndex==0)
            {
                PopulateScheduledList();
            }
            else if(pivotMainLists.SelectedIndex==1)
            {
                populateDoneList();
            }
        }

        private static bool IsDoneListAlreadyLoaded = false;

        private void pivotMainLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivotMainLists.SelectedIndex==1)
            {
                if (!IsDoneListAlreadyLoaded)
                {
                    populateDoneList();

                }
            }
        }

        private void populateDoneList()
        {
            try
            {
                ShowLoadingDialog();

                using (dbHelperConnection db = new dbHelperConnection())
                {
                    var taskslist = db.tbl_ScheduledTasks.Where(t =>  t.IsSent == 1 || t.IsSent==2).OrderByDescending(t => t.TargetDate).ThenByDescending(t => t.TargetTime).Select(t => t);
                    if (taskslist.Count() > 0)
                    {

                        txtNothingDoneList.Visibility = Visibility.Collapsed;
                        lstDoneScheduledTasks.ItemsSource = taskslist.ToArray();
                        IsDoneListAlreadyLoaded = true;
                    }
                    else
                    {
                        txtNothingDoneList.Visibility = Visibility.Visible;
                        lstDoneScheduledTasks.ItemsSource = null;
                        IsDoneListAlreadyLoaded = false;

                    }

                }

                HideLoadingDialog();

            }
            catch (Exception)
            {


            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            //clear listbox item
            try
            {
                var item = ((FrameworkElement)sender).DataContext as tbl_ScheduledTasks;
                if (item!=null)
                {
                    using (dbHelperConnection db=new dbHelperConnection())
                    {
                        db.tbl_ScheduledTasks.Remove(item);
                        if (db.SaveChanges()>0)
                        {
                            populateDoneList();
                        }
                      
                    }
                }

            }
            catch (Exception)
            {

              
            }
        }

        private void grdListitemTemplate_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext;
            if (item!=null)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }

        }

        private async void btnHelp_Click(object sender, RoutedEventArgs e)
        {
          await  new contentPgHelp().ShowAsync();
        }

        private void imgHeader_Loading(FrameworkElement sender, object args)
        {
            var anim = imgHeader.Blur(0.9f);
            anim.SetDurationForAll(2500);
            anim.SetDelay(250);

            anim.Start();

            /////////////////////////
        }

        private void imgHeader_Loaded(object sender, RoutedEventArgs e)
        {
            var anim = imgHeader.Blur(0.5f);
            anim.SetDurationForAll(2500);
            anim.SetDelay(250);

            anim.Start();

            ///////////////////////////
        }

        private void imgHeader_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
          
        }

        private  void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                 Donate();
              // await new donateDialog().ShowAsync();

            }
            catch (Exception)
            {
               
            }
        }
        private async void Donate()
        {
            try
            {
                ///////////////////// begin read setting
               // ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
              //  object donated = AppSettings.Values["IsDonated"];
              
                        ContentDialog donateDialog = new ContentDialog()
                        {
                            Title = "Donate",
                            Content = "Please cheer me up to develop more free Apps!",
                            PrimaryButtonText = "Donate 😊",
                            SecondaryButtonText ="Now now! 🙁"
                        };

                       
                        ContentDialogResult result = await donateDialog.ShowAsync();


                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        //AppSettings.Values["IsDonated"] = "1";
                        //   await new contentDonateList().ShowAsync();
                        await Windows.System.Launcher.LaunchUriAsync(new Uri(@"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SR6U393C95FT6"));

                    }
                    catch
                    {

                    }

                }
                //else
                //{
                //    AppSettings.Values["IsDonated"] = "0";
                //}


            }
            catch (Exception)
            {


            }
        }


        private void CommandBar_Closing(object sender, object e)
        {
            try
            {
                btnDonate.Label = "";
            }
            catch (Exception)
            {


            }
        }

        private void CommandBar_Opening(object sender, object e)
        {
            try
            {
                btnDonate.Label = "Donate";
            }
            catch (Exception)
            {


            }
        }

        Storyboard sb=null;
        private void lstScheduledTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sb!=null)
                {
                    sb.Stop();
                }
                var parentContainer = lstScheduledTasks.ContainerFromIndex(lstScheduledTasks.SelectedIndex);
                StackPanel st = FindChildControl<StackPanel>(parentContainer, "stackItemToTime") as StackPanel;
                sb = ((StackPanel)st).Resources["marquee"] as Storyboard;

                sb.Begin();
            }
            catch (Exception)
            {

               
            }
        }

        private void stackItemToTime_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //Storyboard sb = ((StackPanel)sender).Resources["marquee"] as Storyboard;
           
            //sb.Begin();
        }

        private void stackItemToTime_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            //Storyboard sb = ((StackPanel)sender).Resources["marquee"] as Storyboard;
            //sb.Stop();
        }
        private DependencyObject FindChildControl<T>(DependencyObject control, string ctrlName)
        {
            int childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (int i = 0; i < childNumber; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, i);
                FrameworkElement fe = child as FrameworkElement;
                // Not a framework element or is null
                if (fe == null) return null;

                if (child is T && fe.Name == ctrlName)
                {
                    // Found the control so return
                    return child;
                }
                else
                {
                    // Not found it - search children
                    DependencyObject nextLevel = FindChildControl<T>(child, ctrlName);
                    if (nextLevel != null)
                        return nextLevel;
                }
            }
            return null;
        }

        private void lstScheduledTasks_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
           
        }
    }
}
