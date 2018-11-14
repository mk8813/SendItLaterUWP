using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace SenderPlatform
{
    public static class SenderHandler
    {

        private static async Task SendSms(string to, string message)
        {
            var chatMessage = new Windows.ApplicationModel.Chat.ChatMessage();
            chatMessage.Body = message;
            chatMessage.Recipients.Add(to);
            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(chatMessage);

        }
        private static async Task SendToWhatsapp(string message)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("whatsapp://send?text=" + message));
        }
        private static async Task SendToTelegram(string to, string message)
        {
            //await Windows.System.Launcher.LaunchUriAsync(new Uri("tg:share?url=www.aaa.com&text=" + message + "&to=" + to));
            await Windows.System.Launcher.LaunchUriAsync(new Uri("tg:msg?text=" + message + "&to=" + to));
        }

        private static async Task SendToTwitter(string message)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://twitter.com/intent/tweet?text=" + message));
        }
        private static async Task SendToFbMessenger(string message)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("fb-messenger://share?text=" + message));
        }
        private static async Task SendToSkype(string to, string msg)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("skype:" + to + "?chat&topic=" + msg));
        }

        private static async Task SendEmail(string to, string subject, string body)
        {
            try
            {
                var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
                emailMessage.Body = body;
                emailMessage.To.Add(new Windows.ApplicationModel.Email.EmailRecipient(to));
                emailMessage.Subject = subject;
                await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
            }
            catch (Exception)
            {


            }


        }
    }
}
