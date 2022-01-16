using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinShoppingList1.ViewModels;

namespace XamarinShoppingList1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginWebPage : ContentPage
    {
        public LoginWebPage(LoginWebViewModel loginWebViewModel)
        {
            InitializeComponent();
            
            BindingContext = loginWebViewModel;
            loginWebViewModel.Navigation = Navigation;
        }

        private async void WebView_Navigated(object sender, WebNavigatedEventArgs e)
        { 
            if (GetParameter(e.Url, "access_token") != null)
            {
                await ((LoginWebViewModel)BindingContext).ObtainedAccessTokenAsync(GetParameter(e.Url, "access_token"));
                //Navigation.RemovePage(this);
            }
        }

        public static string GetParameter(string urlString, string param)
        {
            var uri = "";
            if (urlString.EndsWith("#_=_"))
                uri = urlString.Replace("#_=_", "");
            else
                uri = urlString;

            var index = uri.IndexOf("?");

            if (index == -1) return null;

            if (uri[index+1] =='#')
                uri = uri.Substring(index + 2);
            else
                uri = uri.Substring(index + 1);

            var arrayOfParams = uri.Split("&");

            var dic = new Dictionary<string, string>();

            foreach (var item in arrayOfParams)
            {
                var p = item.Split("=");

                if (p.Length>1)
                    dic.Add(HttpUtility.UrlDecode(p[0]), HttpUtility.UrlDecode(p[1]));
            }

            if (!dic.ContainsKey(param))
                return null;

            return dic[param];
        }

      
    }

}