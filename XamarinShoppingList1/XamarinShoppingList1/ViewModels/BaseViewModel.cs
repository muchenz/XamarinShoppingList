using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

using XamarinShoppingList1.Models;
using XamarinShoppingList1.Services;

namespace XamarinShoppingList1.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
       public  INavigation Navigation { get; set; }

        public BaseViewModel()
        {
            InitAsyncCommand = new Command(async () => {
                await InitAsync();
            });

            OnDisappearingAsyncCommand = new Command(async () => {
                await OnDisappearingAsync();
            });

            OnAppearingAsyncCommand = new Command(async () => {
                await OnAppearingAsync();
            });
            //  InitAsyncCommand.Execute(null);
        }
        public ICommand InitAsyncCommand { protected set; get; }
        public ICommand OnDisappearingAsyncCommand { protected set; get; }
        public ICommand OnAppearingAsyncCommand { protected set; get; }

        protected virtual async Task InitAsync()
        {
            await DummyTask();
        }

        protected virtual async Task OnDisappearingAsync()
        {
            await DummyTask();
        }

        protected virtual async Task OnAppearingAsync()
        {
            await DummyTask();
        }

        public async Task DummyTask()
        {
            await Task.Delay(0);
        }

        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>();

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
