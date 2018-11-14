using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ScheduledSMSUWP
{
    public sealed partial class popupAddJob : ContentDialog
    {
        public popupAddJob()
        {
            this.InitializeComponent();
            FillComboBox();
        }

       
        private class AppNames
        {
            private string name;
            private string iconPath;
            private AppType appType;
            public string Name
            {
                get
                {
                    return name;
                }

                set
                {
                    name = value;
                }
            }

            public string IconPath
            {
                get
                {
                    return iconPath;
                }

                set
                {
                    iconPath = value;
                }
            }

            public AppType AppType
            {
                get
                {
                    return appType;
                }

                set
                {
                    appType = value;
                }
            }
        }
        void FillComboBox()
        {
            try
            {
                var apps = new[] {
                      new AppNames() { AppType= AppType.CALL, Name = "Call" , IconPath="Assets/apps-icon/call.png"} ,
                    new AppNames() { AppType= AppType.SMS, Name = "SMS" , IconPath="Assets/apps-icon/sms.png"} ,
                      new AppNames{AppType=AppType.EMAIL, Name = "E-mail" , IconPath="Assets/apps-icon/email.png"} ,
                    new AppNames{AppType=AppType.WHATSAPP, Name = "WhatsApp" , IconPath="Assets/apps-icon/whatsapp.png"} ,
                     new AppNames{AppType=AppType.TWITTER, Name = "Twitter" , IconPath="Assets/apps-icon/twitter.png"} ,
                       new AppNames{AppType=AppType.FBMSG, Name = "Facebook Messenger" , IconPath="Assets/apps-icon/facebook-messenger.png"} ,
                     //new AppNames{ AppType=AppType.TGMSG,Name = "Telegram Messenger" , IconPath="Assets/apps-icon/telegram.png"} ,
                       new AppNames{AppType=AppType.SKYPE, Name = "Skype" , IconPath="Assets/apps-icon/skype.png"} ,

                };

                comboSelectActionType.ItemsSource = apps.ToArray();
                comboSelectActionType.SelectedIndex = 1;
            }
            catch (Exception)
            {


            }
        }


        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {


                if (comboSelectActionType.SelectedIndex != -1 && txtMsg.Text != "")
                {
                    var selectedDate = Convert.ToDateTime(txtDate.Date.Value.ToString("yyyy-MM-dd") + " " + txtTime.Time.ToString());
                    if (selectedDate > DateTime.Now.AddMinutes(1))//correct fields
                    {
                        using (dbHelperConnection db = new dbHelperConnection())///////////begin insert
                        {
                            tbl_ScheduledTasks ts = new tbl_ScheduledTasks();
                            objAlarm taskAlarm = new objAlarm();
                            ////////////////////////////////////////////TASK
                            string  GID= Guid.NewGuid().ToString();
                            ts.Id = GID;
                            ts.AppType = (comboSelectActionType.SelectedValue as AppNames).AppType.ToString();
                            ts.AttachmentPath = "";
                            ts.DateInsert = DateTime.Now.ToString();
                            ts.HasAttachment = 0;
                            ts.IsSent = 0;
                            ts.Message = txtMsg.Text;
                           
                            if (txtSubject.Visibility == Visibility.Visible)
                            {
                                ts.Subject = txtSubject.Text;
                            }
                            ts.TargetDate = txtDate.Date.Value.ToString("yyyy-MM-dd");
                            ts.TargetTime = txtTime.Time.ToString();
                            ts.ToReceiver = SelectedToReceiver != "" ? SelectedToReceiver : txtTo.Text;
                            ts.DisplayName = SelectedContactFullName;
                            ts.IsScheduled = 1;
                            //////////////////////////////////////ALARM
                            taskAlarm.appType = (comboSelectActionType.SelectedValue as AppNames).AppType;
                            taskAlarm.Id = GID;
                            taskAlarm.Message = txtMsg.Text;
                            taskAlarm.Name = (comboSelectActionType.SelectedValue as AppNames).Name;
                            taskAlarm.SingleFireTime = selectedDate;
                            taskAlarm.Subject = txtSubject.Text;
                            taskAlarm.To = SelectedToReceiver;
                            taskAlarm.DisplayName = SelectedContactFullName;
                            //////////////////////////////////

                            db.tbl_ScheduledTasks.Add(ts);
                            if (await db.SaveChangesAsync()>0)
                            {
                             
                                AlarmHelper.ScheduleAlarm(taskAlarm);  
                            }
                            else
                            {
                                db.tbl_ScheduledTasks.Remove(new tbl_ScheduledTasks() { Id=GID});
                               await db.SaveChangesAsync();
                                await new MessageDialog("Sorry, we couldn't complete your request please try again later!").ShowAsync();
                            }


                        }

                        //await new MessageDialog(txtDate.Date.Value.ToString("yyyy-MM-dd") + " " + txtTime.Time.ToString()).ShowAsync();
                    }
                    else //incorect date time
                    {
                        lblDateError.Visibility = Visibility.Visible;
                        args.Cancel = true;
                        // await new MessageDialog("Incorrect").ShowAsync();
                    }
                }
                else
                {
                    args.Cancel = true;
                    //await new MessageDialog("Please fill required fields => App, To, Message").ShowAsync();
                }
            }
            catch (Exception)
            {
                //await new MessageDialog(ex.InnerException.Message + ex.InnerException.StackTrace).ShowAsync();

            }

        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void comboSelectActionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboSelectActionType.SelectedIndex==-1)
                {
                    btnChooseContact.IsEnabled = false;

                }
                else
                {
                    btnChooseContact.IsEnabled = true;
                }
                var selectedApp = (comboSelectActionType.SelectedValue as AppNames).AppType;
                switch (selectedApp)
                {
                    case AppType.CALL:
                        txtTo.PlaceholderText = "Phone number";
                        txtMsg.PlaceholderText = "Note...";
                        txtTo.Visibility = btnChooseContact.Visibility = Visibility.Visible;
                        txtSubject.Visibility = Visibility.Collapsed;
                        break;
                    case AppType.SMS:
                        txtTo.PlaceholderText = "To";
                        txtMsg.PlaceholderText = "Type your message...";
                        txtTo.Visibility = btnChooseContact.Visibility = Visibility.Visible;
                        txtSubject.Visibility = Visibility.Collapsed;
                        break;
                    //case AppType.WHATSAPP:

                    //    break;
                    //case AppType.TGMSG:
                    //    break;
                    //case AppType.FBMSG:
                    //    break;
                    case AppType.TWITTER:
                        txtTo.Visibility = btnChooseContact.Visibility = Visibility.Collapsed;
                        txtSubject.Visibility = Visibility.Collapsed;
                        txtMsg.PlaceholderText = "Type your tweet...";
                        break;
                    case AppType.EMAIL:
                        txtTo.PlaceholderText = "Email address";
                        txtMsg.PlaceholderText = "Body...";
                        txtTo.Visibility = btnChooseContact.Visibility = Visibility.Visible;
                        txtSubject.Visibility = Visibility.Visible;
                        break;
                    case AppType.SKYPE:
                        txtMsg.PlaceholderText = "Type your message...";
                        txtTo.PlaceholderText = "Username or phone number";
                        txtTo.Visibility = btnChooseContact.Visibility = Visibility.Visible;
                        txtSubject.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        txtTo.PlaceholderText = "To";
                        txtMsg.PlaceholderText = "Type your message...";
                        txtTo.Visibility = btnChooseContact.Visibility = Visibility.Visible;
                        txtSubject.Visibility = Visibility.Collapsed;
                        break;
                }
              

            }
            catch (Exception)
            {

               
            }
        }

        private string SelectedContactFullName = "";
        private string SelectedToReceiver = "";

        private async void btnChooseContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var contactPicker = new Windows.ApplicationModel.Contacts.ContactPicker();
                contactPicker.SelectionMode = Windows.ApplicationModel.Contacts.ContactSelectionMode.Fields;

                AppType ChoosedApp=AppType.SMS;

                switch ((comboSelectActionType.SelectedValue as AppNames).AppType.ToString())
                {

                    case "EMAIL":
                        ChoosedApp = AppType.EMAIL;
                        contactPicker.DesiredFieldsWithContactFieldType.Add(Windows.ApplicationModel.Contacts.ContactFieldType.Email);
                        break;

                    //case "SKYPE":
                    //    ChoosedApp = AppType.SKYPE;
                      
                    //    contactPicker.DesiredFieldsWithContactFieldType.Add(Windows.ApplicationModel.Contacts.ContactFieldType.ConnectedServiceAccount);
                    //    break;

                    default:
                       
                        contactPicker.DesiredFieldsWithContactFieldType.Add(Windows.ApplicationModel.Contacts.ContactFieldType.PhoneNumber);
                        break;

                }

                Windows.ApplicationModel.Contacts.Contact contact = await contactPicker.PickContactAsync();

                if (contact!=null)
                {
                    switch (ChoosedApp)
                    {
                        //case AppType.SMS:
                        //    break;
                        //case AppType.WHATSAPP:
                        //    break;
                        //case AppType.TGMSG:
                        //    break;
                        //case AppType.FBMSG:
                        //    break;
                        //case AppType.TWITTER:
                        //    break;
                        case AppType.EMAIL:
                            SelectedContactFullName = contact.FullName;
                            SelectedToReceiver = contact.Emails.FirstOrDefault().Address.ToLower();
                            txtTo.Text = SelectedContactFullName + " <"+SelectedToReceiver +">";
                            break;
                        //case AppType.SKYPE:
                        //    txtTo.Text = contact.ConnectedServiceAccounts.FirstOrDefault().Id;
                        //    break;
                        default:
                            SelectedContactFullName = contact.FullName;
                            SelectedToReceiver = contact.Phones.FirstOrDefault().Number.Trim();
                            txtTo.Text = SelectedContactFullName + " <"+SelectedToReceiver+">";
                            break;
                    }
                   
                }
                else
                {
                    SelectedToReceiver = "";
                    SelectedContactFullName = "";
                    txtTo.Text = "";
                }

            }
            catch (Exception)
            {


            }
        }

        private void txtTime_GotFocus(object sender, RoutedEventArgs e)
        {
            lblDateError.Visibility = Visibility.Collapsed;
        }
    }
}
