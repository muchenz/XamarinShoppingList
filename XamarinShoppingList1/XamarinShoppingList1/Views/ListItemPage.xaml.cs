using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinShoppingList1.Helpers.DragAndDrop.Interfaces;
using XamarinShoppingList1.ViewModels;

namespace XamarinShoppingList1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListItemPage : ContentPage, IDragAndDropContainer
    {        

        public ListItemPage(ListItemViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            viewModel.Navigation = Navigation;

        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            ((ListItemViewModel)BindingContext).OnAppearingAsyncCommand.Execute(null);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            ((ListItemViewModel)BindingContext).OnDisappearingAsyncCommand.Execute(null);
        }

        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {

        }
    }
    
}