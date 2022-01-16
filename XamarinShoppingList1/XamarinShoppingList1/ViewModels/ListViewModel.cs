using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;
using Unity.Resolution;
using Xamarin.Forms;
using XamarinShoppingList1.Helpers;
using XamarinShoppingList1.Models;
using XamarinShoppingList1.Services;
using XamarinShoppingList1.Views;

namespace XamarinShoppingList1.ViewModels
{
    public class ListViewModel:BaseViewModel
    {
        private readonly UserService _userService;
        private readonly ListItemService _listItemService;
        private readonly ListAggregator _listAggregator;
        string userName;
        List _addListModel = new List();
        public List AddListModel { get { return _addListModel; } set { SetProperty(ref _addListModel, value); } } 

        List _selectedItem;
        public List SelectedItem { get { return _selectedItem; } set { SetProperty(ref _selectedItem, value); } }
        public ListViewModel(UserService userService, ListItemService listItemService, ListAggregator listAggregator)
        {
            userName = App.UserName;
            _userService = userService;
            _listItemService = listItemService;
            _listAggregator = listAggregator;

            GetNewData(App.User);

            base.InitAsyncCommand.Execute(null);

        }

        int iDItemToDelete;
        string nameItemToDelete;
        public ICommand DeleteCommand
        {
            get
            {
                return new Command(() => {
                    var message = new DisplayAlertMessage();


                    message.Title = "Warning";
                    message.Message = $"You deleting '{nameItemToDelete}'.";
                    message.Accept = "I'm sure!";
                    message.Cancel = "Cancel";
                    message.OnCompleted += async (accept) =>
                    {
                        if (accept)
                        {
                            try
                            {
                                await _listItemService.Delete<List>(iDItemToDelete, _listAggregator.ListAggregatorId);

                                List.Remove(List.Single(a => a.ListId == iDItemToDelete));
                            }
                            catch (WebPermissionException ex)
                            {
                                Message.MessageDontHavePermission(Application.Current);

                            }
                            catch
                            { 
                            
                            }
                        }
                    };
                    MessagingCenter.Send<Application, DisplayAlertMessage>(Application.Current, "ShowAlert", message);

                });

            }
        }
        bool _isVisibleDeleteLabel;
        public bool IsVisibleDeleteLabel { get { return _isVisibleDeleteLabel; } set { SetProperty(ref _isVisibleDeleteLabel, value); } }

        public ICommand IsVisibleDeleteLabelCommand
        {
            get
            {
                return new Command(() => {

                    if (IsVisibleAddItem) IsVisibleAddItem = false;

                    SelectedItem = null;

                    if (IsVisibleDeleteLabel)
                        IsVisibleDeleteLabel = false;
                    else
                    {
                        IsVisibleDeleteLabel = true;

                    }


                });

            }
        }
        public ICommand AddListCommand
        {
            get
            {

                return new Command(async () =>
                {


                    if (isEdit)
                    {
                        string tempName = SelectedItem.Name;
                        var  temSelectedItem = SelectedItem;
                        SelectedItem.Name = AddListModel.Name;
                        try
                        {
                            await _listItemService.EditItem(SelectedItem, _listAggregator.ListAggregatorId);
                        }
                        catch (WebPermissionException ex)
                        {
                            Message.MessageDontHavePermission(Application.Current);
                            temSelectedItem.Name = tempName;

                        }
                        catch
                        {
                            temSelectedItem.Name = tempName;
                        }


                    }
                    else
                    {
                        List list = null;
                        try
                        {
                            list = await _listItemService.AddItem(_listAggregator.ListAggregatorId, AddListModel, _listAggregator.ListAggregatorId);
                        }
                        catch (WebPermissionException ex)
                        {
                            Message.MessageDontHavePermission(Application.Current);

                        }
                        catch
                        {
                        }
                        if (list != null)
                        {
                            List.Add(list);                           
                        }
                    }

                    AddListModel = new List();
                    isEdit = false;
                    SelectedItem = null;
                    IsVisibleAddItem = false;
                });

            }
        }

        bool _isVisibleAddItem;
        public bool IsVisibleAddItem { get { return _isVisibleAddItem; } set { SetProperty(ref _isVisibleAddItem, value); } }

        public ICommand AddToolbarCommand
        {
            get
            {
                return new Command(() => {

                    if (IsVisibleDeleteLabel) IsVisibleDeleteLabel = false;

                    SelectedItem = null;

                    if (IsVisibleAddItem)
                        IsVisibleAddItem = false;
                    else
                        IsVisibleAddItem = true;


                });

            }
        }

        ListAggregator _listAggr;
        public ListAggregator ListAggr
        {
            get { return _listAggr; }
            set { SetProperty(ref _listAggr, value); }
        }

        private void GetNewData(User arg)
        {
            if (arg== null) return;
          
            try
            {
                ListAggregator temPlist = arg.ListAggregators.Where(a => a.ListAggregatorId == _listAggregator.ListAggregatorId).FirstOrDefault();
                ListAggr = temPlist;

                if (temPlist==null)
                {
                    List = new ObservableCollection<List>();
                }
                else
                {
                    List = new ObservableCollection<List>(temPlist.Lists);
                }
            }
            catch
            {
                List = new ObservableCollection<List>();
            }
        }

        protected override async  Task OnAppearingAsync()
        {

            MessagingCenter.Subscribe<ListAggregationViewModel, User>(this, "New Data", async (sender, arg) =>
            {

                GetNewData(arg);

                if (IsBusy) IsBusy = false;
            });

            GetNewData(App.User);

        }

        protected override async Task OnDisappearingAsync()
        {
            MessagingCenter.Unsubscribe<ListAggregationViewModel, User>(this, "New Data");
        }

        public ICommand LoadItemsCommand => new Command(()=> MessagingCenter.Send(this, "Request for New Data"));

        public ICommand ItemClickedCommand
        {
            get
            {
                return new Command(async (list) => {

                    await Navigation.PushAsync(App.Container.Resolve<ListItemPage>(
                       new ResolverOverride[] { new ParameterOverride("listAggregator", _listAggregator), 
                           new  ParameterOverride("list", list) }));
                });

            }
        }

        bool isEdit;
        public ICommand SelectionChangedCommand
        {
            get
            {
                return new Command(async () => {

                    if (SelectedItem == null) return;

                    if (IsVisibleDeleteLabel)
                    {
                        iDItemToDelete = SelectedItem.ListId;
                        nameItemToDelete = SelectedItem.ListName;
                        DeleteCommand.Execute(null);
                        return;
                    }

                    if (IsVisibleAddItem)
                    {
                        AddListModel.Name = SelectedItem.Name;
                        isEdit = true;
                        return;
                    }

                    var temp = SelectedItem;

                    await Navigation.PushAsync(App.Container.Resolve<ListItemPage>(
                       new ResolverOverride[] { new ParameterOverride("listAggregator", _listAggregator),
                           new  ParameterOverride("list", temp) }));

                    SelectedItem = null;
                });

            }
        }



        ObservableCollection<List> _list { get; set; }

        public ObservableCollection<List> List
        {
            get { return _list; }
            set
            {
                _list = value;
                OnPropertyChanged();

            }
        }

        

        protected override async Task InitAsync()
        {

        }
    }
}
