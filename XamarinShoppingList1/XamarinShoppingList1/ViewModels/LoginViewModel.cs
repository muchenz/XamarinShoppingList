using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;
using Xamarin.Forms;
using XamarinShoppingList1.Models;
using XamarinShoppingList1.Models.Response;
using XamarinShoppingList1.Services;
using XamarinShoppingList1.Views;

namespace XamarinShoppingList1.ViewModels
{
    public class LoginViewModel: BaseViewModel
    {        
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public Command LoginCommand { get; set; }

        public LoginModel Model { get; set; } 

        
        public LoginViewModel(UserService userService, IConfiguration configuration, string loginError=null)
        {
            LoginError = loginError;

            _userService = userService;
            _configuration = configuration;
            LoginCommand = new Command(async () => await Login());
            
            Model = new LoginModel();
            
            if (Application.Current.Properties.ContainsKey("UserName"))
                Model.UserName = Application.Current.Properties["UserName"].ToString();
            if (Application.Current.Properties.ContainsKey("Password"))
                Model.Password = Application.Current.Properties["Password"].ToString();

            Model.PropertyChanged += LoginViewModel_PropertyChanged;
        }

        private void LoginViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName== "UserName")
                Application.Current.Properties["UserName"]=Model.UserName;
            if (e.PropertyName == "Password")
                Application.Current.Properties["Password"] = Model.Password;
        }
        private string _loginError;

        public string LoginError
        {
            get { return _loginError; }
            set { SetProperty(ref _loginError, value); }
        }

        public async Task Login()
        {
            if (!String.IsNullOrEmpty(Model.Password) && !String.IsNullOrEmpty(Model.UserName))
            {
                LoginError = null;

                MessageAndStatusAndData<UserNameAndTokenResponse> response =
                    await _userService.LoginAsync(Model.UserName, Model.Password);


                if (!response.IsError)
                {
                    App.UserName = Model.UserName;
                    App.Token = response.Message;

                    await Navigation.PushAsync(App.Container.Resolve<ListAggregationPage>());
                }
                else
                {
                    LoginError = response.Message;
                }
            }

        }

        
        public ICommand CreateAccountCommand
        {
            get
            {
                return new Command(async (list) => {

                    await Navigation.PushAsync(App.Container.Resolve<RegistrationPage>());



                });

            }
        }
        
        public ICommand LoginFacebookCommand
        {
            get
            {
                return new Command(async (list) => {

                    await Navigation.PushAsync(App.Container.Resolve<LoginWebPage>());

                });

            }
        }

    }
}
