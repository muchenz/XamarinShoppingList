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
using XamarinShoppingList1.Services;
using XamarinShoppingList1.Views;

namespace XamarinShoppingList1.ViewModels
{
    public class LoginViewModel: BaseViewModel
    {        
        private readonly UserService _userService;

        public Command LoginCommand { get; set; }

        public LoginModel Model { get; set; } 

        public LoginViewModel(UserService userService)
        {           
            _userService = userService;
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
                string token = null;
                LoginError = null;
                try
                {
                     token = await _userService.LoginAsync(Model.UserName, Model.Password);
                    LoginError = null;
                }
                catch
                {
                    if (string.IsNullOrEmpty(App.Token))
                        LoginError = "Connection problem.";
                }

                if (!string.IsNullOrEmpty(token))
                {
                    App.UserName = Model.UserName;
                    App.Token = token;

                    await Navigation.PushAsync(App.Container.Resolve<ListAggregationPage>()); 
                }else
                {
                    if (string.IsNullOrEmpty(LoginError))
                        LoginError = "Bad login or passwrod.";
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

    }
}
