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
    public partial class ListAggregationPage : ContentPage
    {
      

        public ListAggregationPage(ListAggregationViewModel listAggregationViewModel)
        {
            InitializeComponent();
            
            BindingContext = listAggregationViewModel;
            listAggregationViewModel.Navigation = Navigation;
        }

        //protected override bool OnBackButtonPressed()
        //{
        //    ((ListAggregationViewModel)BindingContext).Dispose();

        //    return base.OnBackButtonPressed();
        //}


        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent == null)
                DisposeBindingContext();
        }

        protected void DisposeBindingContext()
        {
            if (BindingContext is IDisposable disposableBindingContext)
            {
                disposableBindingContext.Dispose();
                BindingContext = null;
            }
        }
        ~ListAggregationPage()
        {
            DisposeBindingContext();
        }


    }
}