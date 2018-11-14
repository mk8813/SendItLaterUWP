using System;
using System.Linq;
using System.Collections.Generic;
using Windows.Services.Store;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ScheduledSMSUWP
{
    public sealed partial class donateDialog : ContentDialog
    {
        private StoreContext context = null;
        public donateDialog()
        {
            this.InitializeComponent();
            Loading += DonateDialog_Loading;
        }

        private async void DonateDialog_Loading(FrameworkElement sender, object args)
        {
            await GetAddonList();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }


        public async System.Threading.Tasks.Task GetAddonList()
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();

            }

            // Specify the kinds of add-ons to retrieve.
            string[] productKinds = { "Durable" };
            List<String> filterList = new List<string>(productKinds);

            workingProgress.Visibility = Visibility.Visible;
            StoreProductQueryResult queryResult = await context.GetAssociatedStoreProductsAsync(filterList);


            if (queryResult.ExtendedError != null)
            {
                // The user may be offline or there might be some other server failure.
                await ShowMessageDialog($"{queryResult.ExtendedError.Message}");
                return;
            }
            List<Donate> lstDonatesAvailable = new List<Donate>();
            foreach (KeyValuePair<string, StoreProduct> item in queryResult.Products)
            {

                lstDonatesAvailable.Add(new Donate() { Price = item.Value.Price, StoreId = item.Value.StoreId, Title = item.Value.Title });
                // StoreProduct product = item.Value.;

                // Use members of the product object to access info for the product...
            }
            // var cultureInfo = new System.Globalization.CultureInfo("en-US");
            //List<string> langs = new List<string> { "en-US" };
            //var numberformat = new Windows.Globalization.NumberFormatting.NumeralSystemTranslator(langs);


            if (lstDonatesAvailable.Any())
            {
                //  var ss = (lstDonatesAvailable.First().Price.FormattedBasePrice.ToPersianNumber());
             
                lstDonation.ItemsSource = lstDonatesAvailable.OrderBy(l =>
                      decimal.Parse(l.Price.FormattedBasePrice.ToLatinNumber(), System.Globalization.NumberStyles.AllowCurrencySymbol
                    | System.Globalization.NumberStyles.Any));
               

            }
            lstDonation.Visibility = Visibility.Visible;
            workingProgress.Visibility = Visibility.Collapsed;

        }
      
      
        private async System.Threading.Tasks.Task ShowMessageDialog(string message)
        {
            try
            {
                ContentDialog msg = new ContentDialog()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    IsPrimaryButtonEnabled = true,
                    Title = "Sendenarium",
                    PrimaryButtonText = "OK",

                    Content = message,

                };

                await msg.ShowAsync();
            }
            catch (Exception)
            {


            }

        }

        private async void lstDonation_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var item = ((FrameworkElement)sender).DataContext as Donate;
                if (item!=null)
                {
                    workingProgress.Visibility = Visibility.Visible;
                    await PurchaseAddOn(item.StoreId);
                    workingProgress.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {

             
            }
        }

        public async System.Threading.Tasks.Task PurchaseAddOn(string storeId)
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

          
            StorePurchaseResult result = await context.RequestPurchaseAsync(storeId);
           

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null)
            {
                extendedError = result.ExtendedError.Message;
            }

            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                  await  ShowMessageDialog("Already purchased , Thank you! ");
                    break;

                case StorePurchaseStatus.Succeeded:
                  await  ShowMessageDialog("The donation was successful. Thank you!");
                    break;

                case StorePurchaseStatus.NotPurchased:
                   await ShowMessageDialog( extendedError);
                    break;

                case StorePurchaseStatus.NetworkError:
                    await ShowMessageDialog(extendedError);
                    break;

                case StorePurchaseStatus.ServerError:
                    await ShowMessageDialog(extendedError);
                    break;

                default:
                    await ShowMessageDialog(extendedError);
                    break;
            }
        }
    }
    public static class LatinConverter
    {
        public static string ToLatinNumber(this string input)
        {
           
            if (input.Trim() == "") return "";

        /////// PERSIAN ۰ ۱ ۲ ۳ ۴ ۵ ۶ ۷ ۸ ۹
        input = input.Replace("۰", "0")
        .Replace("۱", "1")
        .Replace("۲", "2")
        .Replace("۳", "3")
        .Replace("۴", "4")
        .Replace("۵", "5")
        .Replace("۶", "6")
        .Replace("۷", "7")
        .Replace("۸", "8")
        .Replace("۹", "9")
         ///////////////ARABIC //'٩', '٨', '٧', '٦', '٥', '٤', '٣', '٢', '١','٠
        .Replace("٠", "0")
        .Replace("١", "1")
        .Replace("٢", "2")
        .Replace("٣", "3")
        .Replace("٤", "4")
        .Replace("٥", "5")
        .Replace("٦", "6")
        .Replace("٧", "7")
        .Replace("٨", "8")
        .Replace("٩", "9")
        ///////////////
        .Replace("$", "")
        .Replace("٫", ".")
        .Replace(",", ".")
        .Replace("/", ".")
        .Insert(0, "$");

            return input.Trim();
        }

    }
    public class Donate
    {

        string title;
        StorePrice price;
        string storeId;

 

        public StorePrice Price
        {
            get
            {
                return price;
            }

            set
            {
                price = value;
            }
        }

        public string StoreId
        {
            get
            {
                return storeId;
            }

            set
            {
                storeId = value;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }
    }

}
