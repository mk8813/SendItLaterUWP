using Windows.Storage;
using System;

namespace ScheduledSMSUWP
{

    public class AppLogoTypeSource : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            try
            {
                string path = "ms-appx:///Assets/apps-icon/";
                switch (value.ToString())
                {
                    case "CALL":
                        return path + "call.png";
                    case "SMS":
                        return path + "sms.png";

                    case "WHATSAPP":
                        return path + "whatsapp.png";

                    case "TGMSG":
                        return path + "telegram.png";

                    case "FBMSG":
                        return path + "facebook-messenger.png";

                    case "TWITTER":
                        return path + "twitter.png";

                    case "EMAIL":
                        return path + "email.png";

                    case "SKYPE":
                        return path + "skype.png";


                }

                return "";
            }
            catch (Exception)
            {
                return "";
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsScheduledDone : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            try
            {
                if (value.ToString()=="1")//sent yes
                {
                    return "ms-appx:///Assets/yes.png";
                }
                else if(value.ToString()=="2")
                {
                    return "ms-appx:///Assets/no.png";
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            try
            {
                return DateTime.Parse(value.ToString()).ToString("HH:mm");
            }
            catch (Exception)
            {
                return value.ToString();
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }


}


