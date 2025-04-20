using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;
using Unity.Injection;
using Unity.Resolution;
using Xamarin.Forms;
using XamarinShoppingList1.Helpers;
using XamarinShoppingList1.Models;
using XamarinShoppingList1.Services;
using XamarinShoppingList1.Views;

namespace XamarinShoppingList1.ViewModels
{
    public class ListAggregationViewModel : BaseViewModel, IDisposable
    {
        HubConnection _hubConnection;

        private readonly UserService _userService;
        private readonly ListItemService _listItemService;
        private readonly IConfiguration _configuration;
        string _userName;
        ListAggregator _selectedItem;
        public ListAggregator SelectedItem { get { return _selectedItem; } set { SetProperty(ref _selectedItem, value); } }

        string _invitationsString = "";
        public string InvitationsString
        {
            get { return _invitationsString == "" ? "Manage" : $"Manage\n({_invitationsString})"; }
            set { SetProperty(ref _invitationsString, value); }
        }

        public int WidthRequest { get; set; }
        public string AddEdit => "Add|Edit";

        ListAggregator _addListAggregatorModel = new ListAggregator();
        public ListAggregator AddListAggregatorModel { get { return _addListAggregatorModel; } set { SetProperty(ref _addListAggregatorModel, value); } }

        public ListAggregationViewModel(UserService userService, ListItemService listItemService, IConfiguration configuration)
        {
            _userName = App.UserName;
            _userService = userService;
            _listItemService = listItemService;
            _configuration = configuration;
            MessagingCenter.Subscribe<ListItemViewModel>(this, "Request for New Data", async (a) =>
            {
                var data = await RequestForNewData();
                LoadSaveOrderDataHelper.LoadListAggregatorsOrder();

                MessagingCenter.Send<ListAggregationViewModel, User>(this, "New Data", data);

            });

            MessagingCenter.Subscribe<ListViewModel>(this, "Request for New Data", async (a) =>
            {
                var data = await RequestForNewData();
                LoadSaveOrderDataHelper.LoadListAggregatorsOrder();
                MessagingCenter.Send<ListAggregationViewModel, User>(this, "New Data", data);

            });

            MessagingCenter.Subscribe<ListItemViewModel>(this, "Save And Refresh New Order", (a) =>
           {
               LoadSaveOrderDataHelper.SaveAllOrder(App.User.ListAggregators);
               LoadSaveOrderDataHelper.LoadListAggregatorsOrder();
           });


            base.InitAsyncCommand.Execute(null);
        }

        int iDItemToDelete;
        string nameItemToDelete;

        public ICommand DeleteCommand
        {
            get
            {
                return new Command(() =>
                {
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
                                await _listItemService.Delete<ListAggregator>(iDItemToDelete, iDItemToDelete);

                                ListAggr.Remove(ListAggr.Single(a => a.ListAggregatorId == iDItemToDelete));
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
                return new Command(() =>
                {

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
        public ICommand AddListAggregatorCommand
        {
            get
            {

                return new Command(async () =>
                {

                    if (_isEdit)
                    {
                        var tempSelectedItem = SelectedItem;
                        var tempName = SelectedItem.Name;

                        SelectedItem.Name = AddListAggregatorModel.Name;
                        AddListAggregatorModel.Name = "";
                        _isEdit = false;
                        try
                        {
                            await _listItemService.EditItem(SelectedItem, SelectedItem.ListAggregatorId);
                            // AddListAggregatorModel = new ListAggregator();
                        }
                        catch (WebPermissionException)
                        {

                            Message.MessageDontHavePermission(Application.Current);
                            tempSelectedItem.Name = tempName;


                        }
                        catch
                        {
                            tempSelectedItem.Name = tempName;
                        }

                    }
                    else
                    {
                        ListAggregator listAggr = null;
                        try
                        {
                            AddListAggregatorModel.Order = ListAggr.Any() ? ListAggr.Max(a => a.Order) + 1:1;

                            listAggr = await _listItemService.AddItem(App.User.UserId, AddListAggregatorModel, -1);
                        }
                        catch (WebPermissionException)
                        {
                            Message.MessageDontHavePermission(Application.Current);
                        }
                        catch { }

                        if (listAggr != null)
                            ListAggr.Insert(0,listAggr);
                            //ListAggr.Add(listAggr);
                    }

                    SelectedItem = null;
                    IsVisibleAddItem = false;
                    _isEdit = false;
                    AddListAggregatorModel = new ListAggregator();
                });

            }
        }

        public ICommand LoadItemsCommand
        {
            get
            {

                return new Command(async () =>
                {

                    _ = await RequestForNewData();

                    IsBusy = false;

                });

            }
        }
        public ICommand Logout
        {
            get
            {

                return new Command(() =>
                {


                    var message = new DisplayAlertMessage();


                    message.Title = "Logout";
                    message.Message = $"Are you want logout?";
                    message.Accept = "I'm sure!";
                    message.Cancel = "Cancel";
                    message.OnCompleted += async (accept) =>
                    {
                        if (accept)
                        {
                            try
                            {
                                var a = DependencyService.Get<IClearCookies>();
                                a.ClearAllCookies();

                                Application.Current.Properties["UserName"] = "";
                                Application.Current.Properties["Password"] = "";

                                await Navigation.PopAsync();
                            }

                            catch
                            {

                            }
                        }
                    };
                    MessagingCenter.Send<Application, DisplayAlertMessage>(Application.Current, "ShowAlert", message);




                    IsBusy = false;

                });

            }
        }
        public ICommand ItemClickedCommand
        {
            get
            {
                return new Command(async (listAggr) =>
                {


                    await Navigation.PushAsync(App.Container.Resolve<ListPage>(
                        new ResolverOverride[] { new ParameterOverride("listAggregator", listAggr) }));
                });

            }
        }
        public ICommand InvitationsCommand
        {
            get
            {
                return new Command(async (listAggr) =>
                {
                    InvitationsString = "";

                    await Navigation.PushAsync(App.Container.Resolve<InvitationsPage>(
                        new ResolverOverride[] { new ParameterOverride("listAggregator", listAggr) }));
                });

            }
        }

        bool _isVisibleAddItem;
        public bool IsVisibleAddItem { get { return _isVisibleAddItem; } set { SetProperty(ref _isVisibleAddItem, value); } }
        public ICommand AddToolbarCommand
        {
            get
            {
                return new Command(() =>
                {

                    if (IsVisibleDeleteLabel) IsVisibleDeleteLabel = false;

                    SelectedItem = null;

                    if (IsVisibleAddItem)
                        IsVisibleAddItem = false;
                    else
                        IsVisibleAddItem = true;


                });

            }
        }
        bool _isEdit;
        public ICommand SelectionChangedCommand
        {
            get
            {
                return new Command(async () =>
                {

                    if (SelectedItem == null) return;

                    if (IsVisibleDeleteLabel)
                    {
                        iDItemToDelete = SelectedItem.ListAggregatorId;
                        nameItemToDelete = SelectedItem.ListAggregatorName;
                        DeleteCommand.Execute(null);
                        return;
                    }

                    if (IsVisibleAddItem)
                    {
                        AddListAggregatorModel.Name = SelectedItem.Name;
                        _isEdit = true;
                        return;
                    }

                    var temp = SelectedItem;

                    await Navigation.PushAsync(App.Container.Resolve<ListPage>(
                        new ResolverOverride[] { new ParameterOverride("listAggregator", temp) }));

                    SelectedItem = null;
                });

            }
        }


        ObservableCollection<ListAggregator> _listAggr;

        public ObservableCollection<ListAggregator> ListAggr
        {
            get { return _listAggr; }
            set
            {
                _listAggr = value;
                OnPropertyChanged();

            }
        }

        Task reperterTask;


        async Task<User> RequestForNewData()
        {
            User data = null;

            if (string.IsNullOrEmpty(_userName)) return data;

            try
            {
                data = await _userService.GetUserDataTreeObjectsgAsync(_userName);

                App.User = data;
                LoadSaveOrderDataHelper.LoadListAggregatorsOrder();

                ListAggr = new ObservableCollection<ListAggregator>(data.ListAggregators);
            }
            catch { }


            return data;
        }

        List<IDisposable> _listDisposable = null;
        protected override async Task InitAsync()
        {



            if (!string.IsNullOrEmpty(_userName))
            {
                _ = await RequestForNewData();

                // reperterTask = Task.Run(() => reperterTaskFunctionAsync());
            }

            try
            {
                (_listDisposable, _hubConnection) = await HubConnectionHelper.EstablishSignalRConnectionAsync(App.Token, this, _configuration,
                    RequestForNewData, _listItemService, (a) => InvitationsString = a);

                var invList = await _userService.GetInvitationsListAsync(App.UserName);

                if (invList.Count > 0)
                    InvitationsString = "NEW";

            }

            catch (Exception ex)
            {


            }
        }


        async Task reperterTaskFunctionAsync()
        {

            while (true)
            {
                await Task.Delay(60000);

                User data;
                try
                {
                    data = await RequestForNewData();

                    MessagingCenter.Send<ListAggregationViewModel, User>(this, "New Data", data);
                }
                catch { }

            }
        }


        public void Dispose()
        {

            _listDisposable?.ForEach(x => x?.Dispose());
            //Task.Run(async () =>                         //hang
            //{
            //    if (_hubConnection != null)
            //        await _hubConnection.DisposeAsync();

            //}).GetAwaiter().GetResult();


            //if (_hubConnection != null)
            //    _hubConnection.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();  //hang

            if (_hubConnection != null)
                _ = _hubConnection.DisposeAsync();// from codeoverflow

        }
    }
}
