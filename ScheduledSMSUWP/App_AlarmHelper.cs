using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Notifications;
using Microsoft.QueryStringDotNET;

namespace ScheduledSMSUWP
{
    public static  class AlarmHelper
    {
       
        private const int DAYS_IN_ADVANCE_TO_SCHEDULE = 5;

        public static void ScheduleAlarm(objAlarm alarm)
        {
            EnsureScheduled(alarm, checkForExisting: false);
        }

        public static void RemoveAlarms(string tag)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            //var tag = alarm.Id;// GetTag(alarm);

            // Find all of the scheduled toasts for the alarm
            var scheduledNotifs = notifier.GetScheduledToastNotifications()
                .Where(i => i.Tag.Equals(tag));

            // Remove all of those from the schedule
            foreach (var n in scheduledNotifs)
            {
                notifier.RemoveFromSchedule(n);
            }
        }

        public static void EnsureScheduled(objAlarm alarm)
        {
            EnsureScheduled(alarm, checkForExisting: true);
        }

        private static void EnsureScheduled(objAlarm alarm, bool checkForExisting)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            IReadOnlyList<ScheduledToastNotification> existing = null;
            if (checkForExisting)
            {
                var tag = GetTag(alarm);
                existing = notifier.GetScheduledToastNotifications()
                    .Where(i => i.Tag.Equals(tag))
                    .ToList();
            }

            DateTimeOffset now = DateTimeOffset.Now;

            DateTimeOffset[] alarmTimes = GetAlarmTimesForAlarm(alarm);

            foreach (var time in alarmTimes)
            {
                if (time.AddSeconds(5) > now)
                {
                    // If the alarm isn't scheduled already
                    if (!checkForExisting || !existing.Any(i => i.DeliveryTime == time))
                    {
                        var scheduledNotif = GenerateAlarmNotification(alarm, time);
                        notifier.AddToSchedule(scheduledNotif);
                    }
                }
            }
        }

        private static string GetTag(objAlarm alarm)
        {
            // Tag needs to be 16 chars or less, so hash the Id
            //return alarm.Id.GetHashCode().ToString();
            return alarm.Id.ToString();
        }
        
        private static ScheduledToastNotification GenerateAlarmNotification(objAlarm alarm, DateTimeOffset alarmTime)
        {
            // Using NuGet package Microsoft.Toolkit.Uwp.Notifications


            ToastContent content = new ToastContent()
            {
                Launch= "app-defined-string",
                Scenario = ToastScenario.Alarm,
              
                Visual = new ToastVisual()
                {

                    BindingGeneric = new ToastBindingGeneric()
                    {

                       

                        AppLogoOverride = new ToastGenericAppLogo()
                        {

                            HintCrop = ToastGenericAppLogoCrop.Circle,
                            Source = "ms-appx:///Assets/apps-icon/" + GetAppIcon(alarm.appType)
                        },

                        
                        Children =
                        {
                             new AdaptiveText()
                        {
                             HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                Text = SetAlarmMessageBody(alarm)


                        },
                           
                            new AdaptiveGroup()
                            {
                                
                               Children =
                                {
                                    new AdaptiveSubgroup()
                                      {
                    Children =
                    {
                        new AdaptiveText()
                            {
                                   HintStyle=AdaptiveTextStyle.Body,

                                   Text = SetTitleForBody(alarm)
                            },
                        new AdaptiveText()
                        {
                            Text =" [To:] \n"+GetDisplayName(alarm.DisplayName)+"<"+alarm.To+">",
                               HintStyle = AdaptiveTextStyle.CaptionSubtle
                        }
                    }
                                   },


                                }
                            },

                        },

                        Attribution = new ToastGenericAttributionText()
                        {
                            Text = alarm.Name

                        },
                    }
                },
         
                Actions = new ToastActionsCustom()
                {

                    Buttons =
                    {


                        new ToastButton(SetButtonText(alarm.appType), new QueryString() {
                            {"action","send" },
                            {"id",alarm.Id }
                        }.ToString())
                        {

                            ActivationType =GetActivationType(alarm.appType),

                        },
                        new ToastButton("Discard",new QueryString() {
                            {"action","dismiss" },
                            {"id",alarm.Id }
                        }.ToString())
                        {
                              ActivationType =ToastActivationType.Background
                        }



                   }

                },
                
                //Audio = new ToastAudio()
                //{
                //    Src = new Uri("ms-appx:///Assets/sounds/" + GetCurrentNava().ToString()+".mp3")
                    
                //}

            };

    
            return new ScheduledToastNotification(content.GetXml(), alarmTime)
            {
                
                Tag =alarm.Id,
                
             
            };
        }

        private static string SetButtonText(AppType appt)
        {
            string text = "";
            switch (appt)
            {
                case AppType.CALL:
                    text = "Call";
                    break;
              
                case AppType.TWITTER:
                    text = "Tweet";
                    break;
               
                default:
                    text = "Send";
                    break;
            }
            return text;
        }
        private static string GetDisplayName(string dname)
        {
            return string.IsNullOrEmpty(dname) ? "" : dname;
        }
        private static ToastActivationType GetActivationType(AppType appt)
        {

            return ToastActivationType.Foreground;
            //if (appt == AppType.EMAIL)
            //{
            //    return ToastActivationType.Foreground;
            //}
            //else
            //{
            //    return ToastActivationType.Background;
            //}
           
        }

        private static string SetTitleForBody(objAlarm alarm)
        {
            string title = "";
            switch (alarm.appType)
            {
                case AppType.CALL:
                    title = " [Note:] \n" + alarm.Message;
                    break;
                case AppType.SMS:
                    title = " [Message:] \n" + alarm.Message;
                    break;
                case AppType.WHATSAPP:
                    title = " [Message:] \n" + alarm.Message;
                    break;
                case AppType.TGMSG:
                    title = " [Message:] \n" + alarm.Message;
                    break;
                case AppType.FBMSG:
                    title = " [Message:] \n" + alarm.Message;
                    break;
                case AppType.TWITTER:
                    title = " [Your Tweet:] \n" + alarm.Message;
                    break;
                case AppType.EMAIL:
                    title = " [Text :] \n" + alarm.Message;
                    break;
                case AppType.SKYPE:
                    title = " [Message:] \n" + alarm.Message;
                    break;
                default:
                    break;
            }
            return title;
        }
        private static string SetAlarmMessageBody(objAlarm alarm)
        {
            string msg = "";
            switch (alarm.appType)
            {
                case AppType.CALL:
                    msg = "Scheduled Phone call";
                    break;
              
                case AppType.TWITTER:
                    msg = "Scheduled " + alarm.Name + " post";
                    break;
                case AppType.EMAIL:
                    msg = "Scheduled " + alarm.Name;
                    break;
               
                default:
                    msg = "Scheduled " + alarm.Name + " message";
                    break;
            }
            return msg;
          
        }
        private static string GetAppIcon(AppType appT)
        {
            try
            {
                string logopath = "";
                switch (appT)
                {
                    case AppType.CALL:
                        logopath = "call.png";
                        break;
                    case AppType.SMS:
                        logopath= "sms.png";
                        break;
                    case AppType.WHATSAPP:
                        logopath = "whatsapp.png";
                        break;
                    case AppType.TGMSG:
                        logopath = "telegram.png";
                        break;
                    case AppType.FBMSG:
                        logopath = "facebook-messenger.png";
                        break;
                    case AppType.TWITTER:
                        logopath = "twitter.png";
                        break;
                    case AppType.EMAIL:
                        logopath = "email.png";
                        break;
                    case AppType.SKYPE:
                        logopath = "skype.png";
                        break;
                    default:
                        logopath = "";
                        break;
                }

                return logopath;
            }
            catch (Exception)
            {

                return "";
            }
        }

        private static DateTimeOffset[] GetAlarmTimesForAlarm(objAlarm alarm)
        {
            if (alarm.IsOneTime())
            {
                return new DateTimeOffset[] { alarm.SingleFireTime };
                
            }
            return null;
            //else
            //{
            //    DateTimeOffset today = DateTimeOffset.Now.Date;
            //    List<DateTime> answer = new List<DateTime>();
            //    for (int i = 0; i < DAYS_IN_ADVANCE_TO_SCHEDULE; i++)
            //    {
            //        if (alarm.DaysOfWeek.Contains(today.DayOfWeek))
            //        {
            //            answer.Add(today.Add(alarm.TimeOfDay));
            //        }

            //        today = today.AddDays(1);
            //    }

            //    return answer.ToArray();
            //}
        }


    }

}
