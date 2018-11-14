using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Microsoft.QueryStringDotNET;
using SenderPlatformHandler;

namespace ScheduledTaskBgHandler
{
    public sealed class bgTaskshandler: IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        //IBackgroundTaskInstance _taskInstance = null;
        private BackgroundTaskCancellationReason cancelReason = BackgroundTaskCancellationReason.Abort;
        private volatile bool cancelRequested = false;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += OnCanceled;
            try
            {
                if (cancelRequested == false)
                {

                    var deferral = taskInstance.GetDeferral();

                    var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                    if (details != null)
                    {
                        QueryString arguments = QueryString.Parse(details.Argument);
                        switch (arguments["action"])
                        {
                            case "send":
                                {
                                    string messageId = arguments["id"].ToString();
                                    using (dbHelperConnection db = new dbHelperConnection())
                                    {
                                        var task = db.tbl_ScheduledTasks.Where(t => t.Id == messageId).FirstOrDefault();
                                        if (task != null)
                                        {
                                            ShowNotification("Processing request...",true);
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
                                        if (SenderHandler.Result == "0")
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
                                        await  db.SaveChangesAsync();
                                    }
                                    break;
                                }

                            case "dismiss":
                                {
                                    string messageId = arguments["id"].ToString();
                                    using (dbHelperConnection db = new dbHelperConnection())
                                    {
                                        var task = db.tbl_ScheduledTasks.Where(t => t.Id == messageId).FirstOrDefault();
                                        if (task != null)
                                        {
                                            task.IsSent = 2;//dismissed
                                            task.IsScheduled = 0;
                                            db.tbl_ScheduledTasks.Attach(task);
                                            await db.SaveChangesAsync();
                                            ShowNotification("Discarded!", true);

                                        }

                                      

                                        break;
                                    }
                                }
                            default:
                                break;
                        }

                    }
                }
            }
            finally
            {
                _deferral.Complete();
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

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            cancelRequested = true;
            cancelReason = reason;
            switch (reason)
            {

                case BackgroundTaskCancellationReason.IdleTask:
                    ShowNotification("Background task isn't running, Please launch the app.");
                    break;
                case BackgroundTaskCancellationReason.SystemPolicy:
                    ShowNotification("Background task isn't running, Please launch the app.");
                    break;
            }
            if (_deferral != null)
            {
                _deferral.Complete();
            }
        }
    }
}
