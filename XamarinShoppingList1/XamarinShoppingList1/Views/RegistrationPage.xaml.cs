using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinShoppingList1.ViewModels;

namespace XamarinShoppingList1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistrationPage : ContentPage
    {
        public RegistrationPage(RegistrationViewModel registrationViewModel)
        {
            InitializeComponent();
            BindingContext = registrationViewModel;
            registrationViewModel.Navigation = Navigation;
        }
    }
}