using Microsoft.QueryStringDotNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SenderPlatformHandler;

namespace ScheduledSMSUWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public  App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
          
        }

        private async void ShowStatus()
        {
            try
            {
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationViewTitleBar"))
                {
                    Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                    ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                    titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
                    titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;


                }
                //Mobile customization
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {

                    var statusBar =  StatusBar.GetForCurrentView();
                    if (statusBar != null)
                    {
                        //statusBar.BackgroundOpacity = 100;
                        //statusBar.BackgroundColor = (Windows.UI.Color)(Current.Resources["ApplicationPageBackgroundThemeBrush"] as Windows.UI.Xaml.Media.SolidColorBrush).Color; //(Windows.UI.Color)Current.Resources["SystemAccentColor"];//Windows.UI.Colors.Transparent; //Windows.UI.Colors.Black;
                        //statusBar.ForegroundColor = (Windows.UI.Color)(Current.Resources["ApplicationForegroundThemeBrush"] as Windows.UI.Xaml.Media.SolidColorBrush).Color;
                        await statusBar.HideAsync();

                       

                    }
                }
            }
            catch (Exception ex)
            {
                MessageDialog msg = new MessageDialog(ex.Message);
                await msg.ShowAsync();

            }


        }


        private async Task CheckBackgroundTask()
        {
            try
            {
                var isAlreadyRegistered = BackgroundTaskRegistration.AllTasks.Any(t => t.Value?.Name == "bgTaskshandler");
                if (isAlreadyRegistered)
                {
                    foreach (var tsk in BackgroundTaskRegistration.AllTasks)
                    {
                        if (tsk.Value.Name == "bgTaskshandler")
                        {
                            tsk.Value.Unregister(true);
                            break;
                        }
                    }
                }
                BackgroundExecutionManager.RemoveAccess();
                var hasAccess = await BackgroundExecutionManager.RequestAccessAsync();
                if (hasAccess == BackgroundAccessStatus.DeniedBySystemPolicy || hasAccess == BackgroundAccessStatus.DeniedByUser || hasAccess == BackgroundAccessStatus.Unspecified)
                {
                    await new MessageDialog("Failed to initialize background task, Access denied.").ShowAsync();
                    return;
                }


                var task = new BackgroundTaskBuilder
                {
                    Name = "bgTaskshandler",
                    TaskEntryPoint = typeof(ScheduledTaskBgHandler.bgTaskshandler).ToString()
                };

                ToastNotificationActionTrigger actiontrigger = new ToastNotificationActionTrigger();
          
                task.SetTrigger(actiontrigger);
                //var condition = new SystemCondition(SystemConditionType.SessionConnected);
                //task.AddCondition(condition);//condition
                BackgroundTaskRegistration registration = task.Register();
                //if (registration!=null)
                //{
                //    await new MessageDialog("task started").ShowAsync();
                  
                //}
                ///////////////////////////begin task check
            }
            catch (Exception)
            {

              
            }
          
          
        }
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                var res = Task.Run(async () => { return await CopyDatabase(); }).Result; 
                if (!res)
                {
                    MessageDialog msg = new MessageDialog("Failed to load user data");
                    await msg.ShowAsync();
                }

                ///bg task check
                ///
                await CheckBackgroundTask();
                ////////////////////////////////////////////
                // Ensure the current window is active
                Window.Current.Activate();
                ShowStatus();

                
            }
        }

        protected override async void OnActivated(IActivatedEventArgs e)
        {
            try
            {
                #region getNotifiArgs
                if (e.Kind == ActivationKind.ToastNotification)
                {
           

                    if (e is ToastNotificationActivatedEventArgs)
                    {
                        var toastActivationArgs = e as ToastNotificationActivatedEventArgs;

                        QueryString args = QueryString.Parse(toastActivationArgs.Argument);
                        switch (args["action"])
                        {
                            case "send":
                                string messageId = args["id"].ToString();
                                using (dbHelperConnection db = new dbHelperConnection())
                                {
                                    var task = db.tbl_ScheduledTasks.Where(t => t.Id == messageId).FirstOrDefault();
                                    if (task != null)
                                    {
                                        ShowNotification("Processing request...", true);

                                        switch (task.AppType)
                                        {

                                            case "CALL":
                                                SenderHandler.MakePhoneCall(task.ToReceiver, task.DisplayName);
                                                break;
                                            case "SMS":
                                              
                                                await SenderHandler.SendSms(task.ToReceiver, task.Message);
                                                

                                                break;
                                            case "WHATSAPP":

                                               
                                                await SenderHandler.SendToWhatsapp(task.Message);
                                              
                                                break;
                                            case "TGMSG":
                                               
                                                await SenderHandler.SendToTelegram(task.ToReceiver, task.Message);
                                               
                                                break;
                                            case "FBMSG":
                                               
                                                await SenderHandler.SendToFbMessenger(task.Message);
                                             
                                                break;
                                            case "TWITTER":
                                              
                                                await SenderHandler.SendToTwitter(task.Message);
                                              
                                                break;
                                            case "EMAIL":
                                               
                                                await SenderHandler.SendEmail(task.ToReceiver, task.Subject, task.Message);
                                              
                                                break;
                                            case "SKYPE":
                                               
                                                await SenderHandler.SendToSkype(task.ToReceiver, task.Message);
                                                
                                                break;
                                        }
                                    }
                                    if (SenderHandler.Result=="0")
                                    {
                                        task.IsSent = 1;
                                    }
                                    else
                                    {
                                        task.IsSent = 2;
                                        task.IsScheduled = 0;
                                        ShowNotification(SenderHandler.Result);
                                    }
                                    db.tbl_ScheduledTasks.Attach(task);
                                    await db.SaveChangesAsync();
                                }
                                break;
                        }
                    }

# endregion getNotifiArgs
                    Frame rootFrame = Window.Current.Content as Frame;

                    // Do not repeat app initialization when the Window already has content,
                    // just ensure that the window is active
                    if (rootFrame == null)
                    {
                        // Create a Frame to act as the navigation context and navigate to the first page
                        rootFrame = new Frame();

                        rootFrame.NavigationFailed += OnNavigationFailed;

                        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                        {
                            //TODO: Load state from previously suspended application
                        }

                        // Place the frame in the current Window
                        Window.Current.Content = rootFrame;

                        rootFrame.Navigate(typeof(MainPage));

                    }
                    await CheckBackgroundTask();
                    Window.Current.Activate();
                    ShowStatus();
                }






            }
            catch (Exception)
            {

               
            }
          
      
          
        }
        private void ShowNotification(string text, bool isTemorary = false)
        {
            var xDoc = new XDocument(

new XElement("toast", new XAttribute("launch", "samplenotification"),

new XElement("visual",

new XElement("binding", new XAttribute("template", "ToastGeneric"),

new XElement("text", "Sendenarium"),

new XElement("text", text)))));

            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();

            xmlDoc.LoadXml(xDoc.ToString());
            var toast = new ToastNotification(xmlDoc);
            if (isTemorary)
            {
                toast.ExpirationTime = DateTime.Now.AddSeconds(5);
            }
            var notifi = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            notifi.Show(toast);
        }


        private async System.Threading.Tasks.Task<bool> CopyDatabase()
        {
            bool isDatabaseExisting = false;

            try
            {
                StorageFile storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync("app-db.db");
                isDatabaseExisting = true;
                return true;
            }
            catch
            {
                isDatabaseExisting = false;
            }

            try
            {
                if (!isDatabaseExisting)
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/data/db.sqlite"));
                    await file.CopyAsync(Windows.Storage.ApplicationData.Current.LocalFolder, "app-db.db",NameCollisionOption.ReplaceExisting);
                    return true;
                }
            }
            catch (Exception)
            {

                return false;
            }

            return false;

        }


        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
