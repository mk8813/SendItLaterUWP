using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace SenderPlatformHandler
{

    public static class SenderHandler
    {
       private static string result = "";

        public static string Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

        public static IAsyncOperation<string> SendSms(string to, string message)
        {
            return SendSmsHelper(to, message).AsAsyncOperation();
        }

        public static IAsyncOperation<string> SendToWhatsapp(string message)
        {
            return SendToWhatsappHelper(message).AsAsyncOperation();
        }

        public static IAsyncOperation<string> SendToTelegram(string to, string message)
        {

            return SendToTelegramHelper(to, message).AsAsyncOperation();
        }

        public static IAsyncOperation<string> SendToTwitter(string message)
        {
            return SendToTwitterHelper(message).AsAsyncOperation();
        }


        public static IAsyncOperation<string> SendToFbMessenger(string message)
        {
            return SendToFbMessengerHelper(message).AsAsyncOperation();
        }

        public static IAsyncOperation<string> SendToSkype(string to, string msg)
        {
            return SendToSkypeHelper(to, msg).AsAsyncOperation();
        }
        public static IAsyncOperation<string> SendEmail(string to, string subject, string body)
        {
            return SendEmailHelper(to, subject, body).AsAsyncOperation();
        }

        public static void MakePhoneCall(string phonenumber, string displayname)
        {
            MakeCallHelper(phonenumber, displayname);
        }

        ///Helper methods
        private static async Task<string> SendSmsHelper(string to, string message)
        {
            try
            {
                var chatMessage = new Windows.ApplicationModel.Chat.ChatMessage();
                chatMessage.Body = message;
                chatMessage.Recipients.Add(to);
                await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(chatMessage);
                result= "0";
            }
            catch (Exception ex)
            {

                result = ex.Message;
            }
            return result;

        }
        private static async Task<string> SendToWhatsappHelper(string message)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("whatsapp://send?text=" + message));
                 result = "0";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        private static async Task<string> SendToTelegramHelper(string to, string message)
        {
            try
            {
                //await Windows.System.Launcher.LaunchUriAsync(new Uri("tg:share?url=www.aaa.com&text=" + message + "&to=" + to));
                await Windows.System.Launcher.LaunchUriAsync(new Uri("tg:msg?text=" + message + "&to=" + to));
                 result = "0";

            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        private static async Task<string> SendToTwitterHelper(string message)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("https://twitter.com/intent/tweet?text=" + message));
                 result = "0";
            }
            catch (Exception ex)
            {

                result = ex.Message;
            }
            return result;
        }
        private static async Task<string> SendToFbMessengerHelper(string message)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("fb-messenger://share?text=" + message));
                 result = "0";
            }
            catch (Exception ex)
            {
                result = ex.Message;
               
            }
            return result;
        }
        private static async Task<string> SendToSkypeHelper(string to, string msg)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("skype:" + to + "?chat&topic=" + msg));
                 result = "0";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        private static async Task<string> SendEmailHelper(string to, string subject, string body)
        {
            try
            {
                var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
                emailMessage.Body = body;
                emailMessage.To.Add(new Windows.ApplicationModel.Email.EmailRecipient(to));
                emailMessage.Subject = subject;
                await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
                result = "0";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;

        }

        private static  string  MakeCallHelper(string phonenumber, string displayname)
        {
            try
            {
                //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.ApplicationModel.Calls.PhoneCallManager"))
                //{
                    Windows.ApplicationModel.Calls.PhoneCallManager.ShowPhoneCallUI(phonenumber, displayname);
                    result = "0";
                //}
                //else
                //{
                //    result = "PHONE CALL IS NOT SUPPORTED.";
                //}
            }
            catch (Exception ex)
            {

                result = ex.Message;
            }
            return result;
           
        }
    }
    

}
