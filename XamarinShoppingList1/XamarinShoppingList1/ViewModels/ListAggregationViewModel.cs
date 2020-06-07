using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
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
    public class ListAggregationViewModel : BaseViewModel
    {
        HubConnection _hubConnection;

        private readonly UserService _userService;
        private readonly ListItemService _listItemService;
        string _userName;
        ListAggregator _selectedItem;
        public ListAggregator SelectedItem { get { return _selectedItem; } set { SetProperty(ref _selectedItem, value); } }

        ListAggregator _addListAggregatorModel = new ListAggregator();
        public ListAggregator AddListAggregatorModel { get { return _addListAggregatorModel; } set { SetProperty(ref _addListAggregatorModel, value); } }

        public ListAggregationViewModel(UserService userService, ListItemService listItemService)
        {
            _userName = App.UserName;
            _userService = userService;
            _listItemService = listItemService;
            MessagingCenter.Subscribe<ListItemViewModel>(this, "Request for New Data", async (a) =>
            {
                var data = await RequestForNewData();
                LoadListAggregatorsOrder();

                MessagingCenter.Send<ListAggregationViewModel, User>(this, "New Data", data);

            });

            MessagingCenter.Subscribe<ListViewModel>(this, "Request for New Data", async (a) =>
            {
                var data = await RequestForNewData();
                LoadListAggregatorsOrder();
                MessagingCenter.Send<ListAggregationViewModel, User>(this, "New Data", data);

            });

            MessagingCenter.Subscribe<ListItemViewModel>(this, "Save And Refresh New Order",  (a) =>
            {
                SaveAllOrder(App.User.ListAggregators);
                LoadListAggregatorsOrder();
            });


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
                                await _listItemService.Delete<ListAggregator>(iDItemToDelete, iDItemToDelete);

                                ListAggr.Remove(ListAggr.Single(a => a.ListAggregatorId == iDItemToDelete));
                            }
                            catch { }
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
        public ICommand AddListAggregatorCommand
        { get {

                return new Command(async () =>{

                    if (isEdit)
                    {
                        var  tempSelectedItem = SelectedItem;
                        var tempName = SelectedItem.Name;

                        SelectedItem.Name = AddListAggregatorModel.Name;
                        AddListAggregatorModel.Name = "";
                        isEdit = false;
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
                            listAggr = await _listItemService.AddItem(App.User.UserId, AddListAggregatorModel, -1);
                        }
                        catch (WebPermissionException)
                        {
                            Message.MessageDontHavePermission(Application.Current);
                        }
                        catch { }

                        if (listAggr != null)
                            ListAggr.Add(listAggr);
                    }
                   
                    SelectedItem = null;
                    IsVisibleAddItem = false;
                    isEdit = false;
                    AddListAggregatorModel = new ListAggregator();
            });
            
            } }

        public ICommand LoadItemsCommand
        {
            get
            {

                return new Command(async () => {

                    _ = await RequestForNewData();

                    IsBusy = false;

                });

            }
        }
        public ICommand ItemClickedCommand { get {
                return new Command(async (listAggr) => {
                                  
                    
                    await Navigation.PushAsync(App.Container.Resolve<ListPage>(
                        new ResolverOverride[] { new ParameterOverride("listAggregator", listAggr) }));
                });
            
            }
        }
         public ICommand InvitationsCommand
        {
            get
            {
                return new Command(async (listAggr) => {


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
                return new Command( () => {

                    if (IsVisibleDeleteLabel) IsVisibleDeleteLabel = false;

                    SelectedItem = null;

                    if (IsVisibleAddItem)
                        IsVisibleAddItem = false;
                    else
                        IsVisibleAddItem = true;


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
                        iDItemToDelete = SelectedItem.ListAggregatorId;
                        nameItemToDelete = SelectedItem.ListAggregatorName;
                        DeleteCommand.Execute(null);
                        return;
                    }

                    if (IsVisibleAddItem)
                    {
                        AddListAggregatorModel.Name = SelectedItem.Name;
                        isEdit = true;
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

        public ObservableCollection<ListAggregator> ListAggr { get { return _listAggr; }
            set {
                _listAggr = value;
                OnPropertyChanged();
            
            } }

        Task reperterTask;


        async Task<User> RequestForNewData()
        {
            User data = null;

            if (string.IsNullOrEmpty(_userName)) return data;

            try
            {
                data = await _userService.GetUserDataTreeObjectsgAsync(_userName);

                App.User = data;

                ListAggr = new ObservableCollection<ListAggregator>(data.ListAggregators);
            }
            catch { }

            LoadListAggregatorsOrder();

            return data;
        }

        protected override async Task InitAsync()
        {



            if (!string.IsNullOrEmpty(_userName))
            {
                 _=  await RequestForNewData();

                // reperterTask = Task.Run(() => reperterTaskFunctionAsync());
            }

            try
            {
                
                _hubConnection = new HubConnectionBuilder().WithUrl("https://94.251.148.92:5013/chatHub", (opts) =>
                {
                 //   _hubConnection = new HubConnectionBuilder().WithUrl("https://192.168.8.222:91/chatHub", (opts) =>
                  //  {
                        opts.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                            // bypass SSL certificate
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        return message;
                    };
                }).WithAutomaticReconnect().Build();
                                

                _hubConnection.On("DataAreChanged_"+App.User.UserId, async (string command, int? id1, int? listAggregationId, int? parentId) =>
                {


                    if (string.IsNullOrEmpty(command)){

                        var data = await RequestForNewData();

                        MessagingCenter.Send<ListAggregationViewModel, User>(this, "New Data", data);

                        return;
                    }
                    

                    if (command.EndsWith("ListItem")) {
                        var item = await _listItemService.GetItem<ListItem>((int)id1, (int)listAggregationId);

                        if (command == "Edit/Save_ListItem")
                        {
                            var lists = App.User.ListAggregators.Where(a => a.ListAggregatorId == listAggregationId).FirstOrDefault();

                            ListItem foundListItem = null;
                            foreach (var listItem in lists.Lists)
                            {
                                foundListItem = listItem.ListItems.Single(a => a.Id == id1);
                                if (foundListItem != null) break;
                            }
                            foundListItem.ListItemName = item.ListItemName;
                            foundListItem.State = item.State;

                        } else
                         if (command == "Add_ListItem")
                        {
                            var tempList = App.User.ListAggregators.Where(a => a.ListAggregatorId == listAggregationId).FirstOrDefault().
                            Lists.Where(a => a.ListId == parentId).FirstOrDefault().ListItems;

                            if (!tempList.Where(a => a.Id == item.Id).Any())
                            {
                                tempList.Add(item);
                            }
                            MessagingCenter.Send<ListAggregationViewModel, User>(this, "New Data", App.User);
                        }
                        else
                         if (command == "Delete_ListItem")
                        {

                            var lists = App.User.ListAggregators.Where(a => a.ListAggregatorId == listAggregationId).FirstOrDefault();

                            ListItem foundListItem = null;
                            List founfList = null;

                            foreach (var listItem in lists.Lists)
                            {
                                founfList = listItem;
                                foundListItem = listItem.ListItems.Single(a => a.Id == id1);
                                if (foundListItem != null) break;
                            }

                            founfList.ListItems.Remove(foundListItem);

                            MessagingCenter.Send<ListAggregationViewModel, User>(this, "New Data", App.User);

                        }
                    }
                });

                await _hubConnection.StartAsync();

            }

            catch(Exception ex)
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

        /// <summary>
        /// ////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        void LoadListAggregatorsOrder()
        {
            

            if (App.User == null || App.User.ListAggregators == null || !App.User.ListAggregators.Any()) return;


            SetEntryOrder(App.User.ListAggregators);

            string UserName = "";

            if (Application.Current.Properties.ContainsKey("UserName"))
                UserName = Application.Current.Properties["UserName"].ToString();

            if (!Application.Current.Properties.ContainsKey($"{UserName}ListOrder"))
                return;
            List<OrderListAggrItem> tempListFromFile = null;

            try
            {
                var toDeserialize = Application.Current.Properties[$"{UserName}ListOrder"] as string;

                tempListFromFile = JsonConvert.DeserializeObject<List<OrderListAggrItem>>(toDeserialize);
            }
            catch
            {

            }

            // if (tempListFromFile == null) return;  //????????????????? nie


            foreach (var listAggr in App.User.ListAggregators)
            {

                var itemAggrFromFile = tempListFromFile?.Where(a => a.Id == listAggr.ListAggregatorId).FirstOrDefault();

                if (itemAggrFromFile != null)
                {

                    listAggr.Order = itemAggrFromFile.Order;

                }


                ////////////////////
                SetEntryOrder(listAggr.Lists);


                foreach (var listList in listAggr.Lists)
                {

                    var itemListFromFile = itemAggrFromFile?.List.Where(a => a.Id == listList.ListId).FirstOrDefault();

                    if (itemListFromFile != null)
                    {

                        listList.Order = itemListFromFile.Order;

                    }


                    SetEntryOrder(listList.ListItems);


                    foreach (var listItem in listList.ListItems)
                    {

                        var itemItemFromFile = itemListFromFile?.List.Where(a => a.Id == listItem.ListItemId).FirstOrDefault();

                        if (itemItemFromFile != null)
                        {

                            listItem.Order = itemItemFromFile.Order;

                        }

                    }

                    ResolveDoubleOrderValue(listList.ListItems);

                    listList.ListItems = listList.ListItems.OrderBy(a => a.Order).ToList();
                }

                ResolveDoubleOrderValue(listAggr.Lists);

                listAggr.Lists = listAggr.Lists.OrderBy(a => a.Order).ToList();
                ////////////////////////////


            }


            ResolveDoubleOrderValue(App.User.ListAggregators);


            App.User.ListAggregators = App.User.ListAggregators.OrderBy(a => a.Order).ToList();

        }

        void ResolveDoubleOrderValue(IEnumerable<IModelItemOrder> list)
        {
            foreach (var item in list)
            {
                foreach (var item2 in list)
                {
                    if (item.Id != item2.Id)
                        if (item.Order == item2.Order)
                            item2.Order = list.Max(a => a.Order) + 1;
                }
            }

        }

        void SetEntryOrder(IEnumerable<IModelItemOrder> list)
        {
            int i = 1;
            foreach (var item in list)
            {
                //if (item.Order == 0)
                //{
                //    item.Order = list.Max(a => a.Order) + 1;

                //}

                item.Order = i++;
            }
            
        }

        

        void  SaveAllOrder(ICollection<ListAggregator> list)
        {
            var tempAggrList = new List<OrderListAggrItem>();

            int i=1, j=1, k = 1;
            list.ToList().ForEach(aggr =>
            {

                var itemAggr = new OrderListAggrItem { Id = aggr.ListAggregatorId, Order = i++ };


                aggr.Lists.ToList().ForEach(lista =>
                {
                    var itemList = new OrderListItem { Id = lista.ListId, Order = j++ };

                    lista.ListItems.ToList().ForEach(item =>
                    {

                        var itemItem = new OrderItem { Id = item.ListItemId, Order = k++ };

                        itemList.List.Add(itemItem);

                    });


                    itemAggr.List.Add(itemList);
                });


                tempAggrList.Add(itemAggr);

            });




            if (Application.Current.Properties.ContainsKey("UserName")) {
                var UserName = Application.Current.Properties["UserName"].ToString();

                string serializedObject= JsonConvert.SerializeObject(tempAggrList);

                Application.Current.Properties[$"{UserName}ListOrder"] = serializedObject;
                
            }
        }
    }
}
