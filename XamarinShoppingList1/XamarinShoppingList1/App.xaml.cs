using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using Unity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinShoppingList1.Models;
using XamarinShoppingList1.Services;
using XamarinShoppingList1.ViewModels;
using XamarinShoppingList1.Views;

namespace XamarinShoppingList1
{
    public partial class App : Application
    {
        public static UnityContainer Container { get; set; }
        public static string Token { get; set; }
        public static string UserName { get; set; }
       //public static string Password { get; set; }
        public static User User { get; set; }
        public static ObservableCollection<ListAggregator> Data { get; set; }
        public App()
        {


            InitializeComponent();

            InitContainer();
            InitMessage();

            MainPage = new NavigationPage( App.Container.Resolve<LoginPage>());
        }


        private  void InitContainer()
        {
            App.Container = new UnityContainer();

            App.Container.RegisterType<IDataStore<Item>, MockDataStore>();

            App.Container.RegisterType<LoginPage>();
            App.Container.RegisterType<LoginViewModel>(); 
            App.Container.RegisterType<ListAggregationPage>(); 
            App.Container.RegisterType<ListAggregationViewModel>();
            App.Container.RegisterType<ListPage>();
            App.Container.RegisterType<ListViewModel>(); 
            App.Container.RegisterType<RegistrationPage>(); 
            App.Container.RegisterType<RegistrationViewModel>();
            App.Container.RegisterType<PermissionsPage>();
            App.Container.RegisterType<PermissionsViewModel>();

            App.Container.RegisterType<HttpClient>();
            App.Container.RegisterType<UserService>(); 
            App.Container.RegisterType<ListItemService>(); 

            
        }

        private  void InitMessage()
        {
            MessagingCenter.Subscribe<Application, DisplayAlertMessage>(this, "ShowAlert", async (sender, message) =>
            {          

                var result = true;
                if (!string.IsNullOrEmpty(message.Accept))
                    result = await this.MainPage.DisplayAlert(message.Title, message.Message, message.Accept, message.Cancel);
                else
                {
                     await this.MainPage.DisplayAlert(message.Title, message.Message, message.Cancel);
                }

                if (message.OnCompleted != null)
                    message.OnCompleted(result);

            }, Application.Current);

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
