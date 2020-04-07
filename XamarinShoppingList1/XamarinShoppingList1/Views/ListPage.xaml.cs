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
    public partial class ListPage : ContentPage
    {
      

        public ListPage(ListViewModel listViewModel)
        {
            InitializeComponent();
            BindingContext = listViewModel;
            listViewModel.Navigation = Navigation;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            ((ListViewModel)BindingContext).OnAppearingAsyncCommand.Execute(null);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ((ListViewModel)BindingContext).OnDisappearingAsyncCommand.Execute(null);
        }
    }
}