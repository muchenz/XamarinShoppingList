using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;
using Xamarin.Forms;
using XamarinShoppingList1.Models;
using XamarinShoppingList1.Services;
using XamarinShoppingList1.Views;

namespace XamarinShoppingList1.ViewModels
{
    public class InvitationsViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly ListAggregator _listAggregator;

        
        Invitation _selectedItem;
        public Invitation SelectedItem { get { return _selectedItem; } set { SetProperty(ref _selectedItem, value); } }

        string _informationLabel;
        public string InformationLabel { get { return _informationLabel; } set { SetProperty(ref _informationLabel, value); } }
        public InvitationsViewModel(UserService userService, ListAggregator listAggregator)
        {
            _userService = userService;
            _listAggregator = listAggregator;                  

          

            base.InitAsyncCommand.Execute(null);

        }
        ObservableCollection<Invitation> _invitations { get; set; }

        public ObservableCollection<Invitation> Invitations
        {
            get { return _invitations; }
            set
            {
                _invitations = value;
                OnPropertyChanged();

            }
        }


        public ICommand ManageCommand
        {
            get
            {
                return new Command(async () =>
                {

                    await Navigation.PushAsync(App.Container.Resolve<PermissionsPage>());
                });
            }
        }

        public ICommand LoadItemsCommand {
            get
            {
                return new Command(async () => {
                    try
                    {
                        var temPlist = await _userService.GetInvitationsListAsync(App.UserName);

                        Invitations = new ObservableCollection<Invitation>(temPlist);
                    }
                    catch { }

                    IsBusy = false;

                    if (Invitations==null || Invitations.Count == 0)
                    {
                        InformationLabel = "No invitation";
                    }
                });
            }
        }
        public ICommand AcceptCommand { get {
                return new Command<Invitation>(async (item)=> {
                    try
                    {
                        await _userService.AcceptInvitationAsync(item);

                        await InitAsync();
                    }
                    catch { }
                });
            } }
        public ICommand RejectCommand {  get
            {
                return new Command<Invitation>(async (item) => {

                    try
                    {
                        await _userService.RejectInvitaionAsync(item);

                        await InitAsync();
                    }
                    catch { }
                });
            }
        }

        protected override async Task InitAsync()
        {
            try
            {
                var temPlist = await _userService.GetInvitationsListAsync(App.UserName);

                Invitations = new ObservableCollection<Invitation>(temPlist);
            }
            catch { }
            if (Invitations == null || Invitations.Count == 0)
            {
                InformationLabel = "No invitation";
            }

        }
    }
}
