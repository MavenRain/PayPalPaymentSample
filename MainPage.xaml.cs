using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.Xaml;
using PayPal.Here.SDK;
using PayPal.Here.SDK.Authentication;
using PayPal.Here.SDK.Domain;
using PayPal.Here.SDK.Errors;
using PayPal.Here.SDK.Errors.Enumerations;
using PayPal.Here.SDK.Invoicing;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PayPalPaymentSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var startUri = new Uri("https://api.sandbox.paypal.com/v1/oauth2/token");
            var httpClient = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, startUri);
            var iDictionary = new Dictionary<string, string>();
            iDictionary.Add("grant_type", "client_credentials");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("AYSkoxC6MolMcRAlHJF6xWTvIWj4A6XcyAdhBSxsL8CJ1-wTo8L3L69V5A6c:ELxcaRDSmbrGcSSAGSZMiCMM4nRoufAwiXiFcfdGeuIQoziPwuMIm8d4LFC-")));
            httpRequest.Content = new FormUrlEncodedContent(iDictionary);
            var httpResponseObject = JsonObject.Parse(await((await httpClient.SendAsync(httpRequest)).Content.ReadAsStringAsync()));
            //Output.Text = httpResponseObject.GetNamedString("access_token", "");
            await
                PayPalHereSDK.SetCredentials(CancellationToken.None,
                    new OAuthCredentials(httpResponseObject.GetNamedString("access_token", "")), PayPalHereSDK.Sandbox);
            var invoice = PayPalHereSDK.TransactionManager.BeginPayment();
            invoice.ReferrerCode = "NoBNCode";
            var item = DomainFactory.CreateInvoiceItem(Guid.NewGuid().ToString(), "Onyeka.Obi@gmail.com", 2m, "0.089");
            item.Description = "Credit transfer";
            invoice.AddOrUpdateItem(item, 1);
            var manualCard = DomainFactory.CreateKeyedInCardData("4123456789012345", "112017", "567");
            manualCard.CardHoldersName = "Onyeka Obi";
            manualCard.PostalCode = "98005";
            var response =
                await PayPalHereSDK.TransactionManager.AuthorizePayment(CancellationToken.None, manualCard);
            if (response.IsSuccess) OnPaymentSuccess(response.Result);
            else OnPaymentError(response.Error);
            
        }

        void OnPaymentSuccess(PaymentResponse response)
        {
            DispatchedHandler procession = () => Output.Text = response.TransactionRecord.AuthCode + "\n" + response.TransactionRecord.CorrelationId + "\n" + response.TransactionRecord.ResponseCode;
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, procession);
        }

        void OnPaymentError(PPError<PaymentErrors> response)
        {
            DispatchedHandler procession = () => Output.Text = "error:" + response.DetailedDescription;
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, procession);
        }
    }
}
