using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinShoppingList1.Models;
using XamarinShoppingList1.Services;
using XamarinShoppingList1.ViewModels;

namespace XamarinShoppingList1.Helpers
{
    public class HubConnectionHelper
    {
        public static async Task<HubConnection> EstablishSignalRConnectionAsync(string token, ListAggregationViewModel vm, 
            IConfiguration configuration,Func<Task<User>> RequestForNewData, ListItemService listItemService,
            Action<string> SetInvitationString)
        {
            var signalRAddress = configuration.GetSection("AppSettings")["SignlRAddress"];
            HubConnection hubConnection = new HubConnectionBuilder().WithUrl(signalRAddress, (opts) =>
            {
                opts.Headers.Add("Access_Token", token);

                opts.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                        // bypass SSL certificate
                        clientHandler.ServerCertificateCustomValidationCallback +=
                            (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    return message;
                };
            }).WithAutomaticReconnect().Build();


            hubConnection.On("DataAreChanged_" + App.User.UserId, async () =>
            {
                    var data = await RequestForNewData();

                    MessagingCenter.Send<ListAggregationViewModel, User>(vm, "New Data", data);

                    return;
            });

             hubConnection.On("ListItemAreChanged_" + App.User.UserId, async (string command, int? id1, int? listAggregationId, int? parentId) =>
             {
                if (command.EndsWith("ListItem"))
                {
                    var item = await listItemService.GetItem<ListItem>((int)id1, (int)listAggregationId);

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

                    }
                    else
                     if (command == "Add_ListItem")
                    {
                        var tempList = App.User.ListAggregators.Where(a => a.ListAggregatorId == listAggregationId).FirstOrDefault().
                        Lists.Where(a => a.ListId == parentId).FirstOrDefault().ListItems;

                        if (!tempList.Where(a => a.Id == item.Id).Any())
                        {
                            tempList.Add(item);
                        }
                        MessagingCenter.Send<ListAggregationViewModel, User>(vm, "New Data", App.User);
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

                        MessagingCenter.Send<ListAggregationViewModel, User>(vm, "New Data", App.User);

                    }
                }
            });

            hubConnection.On("NewInvitation_" + App.User.UserId, async () =>
            {

                SetInvitationString("NEW");
            });

            await hubConnection.StartAsync();

            return hubConnection;

        }
    }
}
