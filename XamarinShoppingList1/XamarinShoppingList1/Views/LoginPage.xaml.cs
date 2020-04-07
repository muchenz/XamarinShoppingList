using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinShoppingList1.Models;
using XamarinShoppingList1.Services;
using XamarinShoppingList1.ViewModels;

namespace XamarinShoppingList1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {

        public LoginPage(LoginViewModel loginViewModel)
        {
            InitializeComponent();
          
            BindingContext = loginViewModel;
            loginViewModel.Navigation=Navigation;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Application.Current.Properties.ContainsKey("UserName"))
                ((LoginViewModel)BindingContext).Model.UserName = Application.Current.Properties["UserName"].ToString();
            if (Application.Current.Properties.ContainsKey("Password"))
                ((LoginViewModel)BindingContext).Model.Password = Application.Current.Properties["Password"].ToString();
        }
    }
}