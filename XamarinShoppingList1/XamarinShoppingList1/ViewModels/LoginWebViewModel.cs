using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Resolution;
using Xamarin.Forms;
using XamarinShoppingList1.Services;
using XamarinShoppingList1.Views;

namespace XamarinShoppingList1.ViewModels
{
    public class LoginWebViewModel: BaseViewModel
    {

        string facbookUrl = "";
        public LoginWebViewModel(UserService userService)
        {
            WebUrl = "https://www.facebook.com/v10.0/dialog/oauth?client_id=259675572518658"
                    + "&response_type=token"
                    + "&redirect_uri=https://192.168.0.222:5003/api/User/FacebookToken"
                    + "&state=st=state123abc,ds=123456789&scope=public_profile,email";

            _userService = userService;
        }




        string _webUrl;
        private readonly UserService _userService;

        public string WebUrl { get { return _webUrl; } set { SetProperty(ref _webUrl, value); } }

        public async Task ObtainedAccessTokenAsync(string accessFacebookToken)
        {
            var response = await _userService.GetTokenFromFacebookAccessToken(accessFacebookToken);

            if (response.IsError == false)
            {

                App.UserName = response.Data.Email;
                App.Token = response.Data.Token;
                App.FacebookToken = accessFacebookToken;
                await Navigation.PushAsync(App.Container.Resolve<ListAggregationPage>());
                if (Navigation.NavigationStack.Count > 1)
                {
                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                }


            }
            else
            {
                ((LoginViewModel)Navigation.NavigationStack[0].BindingContext).LoginError = response.Message;
                await Navigation.PopAsync();
               
            }
            

        }

    }
}
