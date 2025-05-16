using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class RegistrationViewModel: BaseViewModel
    {
        private readonly UserService _userService;

        public ICommand RegistrationCommand { get; set; }

        public RegistrationModel Model { get; set; } = new RegistrationModel { UserName = "", Password = "", PasswordConfirm="" };
      
        public RegistrationModelError ModelError { get; set; } 

        public RegistrationViewModel(UserService userService)
        {
            _userService = userService;
            RegistrationCommand = new Command(async () => await Registration());

            ModelError = new RegistrationModelError(Model);
            
        }

        string _registrationError;
        public string RegistrationError {

            get { return _registrationError; }
            set
            {
                //_registrationError = value;
                 SetProperty(ref _registrationError, value);
                //OnPropertyChanged("RegistrationError");
            }
        }
        public async Task Registration()
        {
            MessageAndStatusAndData<string> response = null;

            RegistrationError = "";
            if (!ModelError.IsValid)
            {
                RegistrationError = "Form is invalid.";
                return;
            }
            try
            {
                response = await _userService.RegisterAsync(Model);

                if (response.IsError)
                {
                    RegistrationError = response.Message;

                    return;
                }
                else
                {
                    App.UserName = Model.UserName;

                    App.Token = response.Data;


                    Application.Current.Properties["UserName"] = Model.UserName;

                    Application.Current.Properties["Password"] = Model.Password;

                    Navigation.RemovePage(Navigation.NavigationStack.Last());
                    await Navigation.PushAsync(App.Container.Resolve<ListAggregationPage>());

                    


                }
            }
            catch (Exception ex)
            {
                RegistrationError = "Some error,  try again.";
            }

            

        }

        public ICommand CreateAccountCommand
        {
            get
            {
                return new Command(async (list) => {

                    //await Navigation.PushAsync(App.Container.Resolve<ListItemPage2>());



                });

            }
        }

    }
}
