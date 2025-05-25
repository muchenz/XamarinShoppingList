using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinShoppingList1.Models;
using XamarinShoppingList1.Services;
using XamarinShoppingList1.ViewModels;

namespace XamarinShoppingList1.Helpers
{
    public class HubConnectionHelper
    {
        public static async Task<(List<IDisposable>, HubConnection)> EstablishSignalRConnectionAsync(string token, ListAggregationViewModel vm,
            IConfiguration configuration, Func<Task<User>> RequestForNewData, ListItemService listItemService,
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


            var dataAreChangeDispose = hubConnection.On("DataAreChanged_" + App.User.UserId,
                async () =>
            {
                var data = await RequestForNewData();

                MessagingCenter.Send<ListAggregationViewModel, User>(vm, "New Data", data);

                return;
            });

            var listItemArechangeDispose = hubConnection.On("ListItemAreChanged_" + App.User.UserId,
                async (string signaREnvelope) =>
                {

                    var envelope = JsonSerializer.Deserialize<SignaREnvelope>(signaREnvelope);
                    var eventName = envelope.SiglREventName;
                    var signaREventSerialized = envelope.SerializedEvent;

                    ListItemSignalREvent signaREvent = GetDeserializedSinglaREvent(signaREventSerialized);


                    var listAggregationId = signaREvent.ListAggregationId;
                    var listItemId = signaREvent.ListItemId;

                    var item = await listItemService.GetItem<ListItem>(listItemId, listAggregationId);

                    switch (eventName)
                    {
                        case SiganalREventName.ListItemEdited:
                            {
                                var lists = App.User.ListAggregators.Where(a => a.ListAggregatorId == listAggregationId).FirstOrDefault();

                                ListItem foundListItem = null;
                                foreach (var listItem in lists.Lists)
                                {
                                    foundListItem = listItem.ListItems.FirstOrDefault(a => a.Id == listItemId);
                                    if (foundListItem != null) break;
                                }
                                foundListItem.ListItemName = item.ListItemName;
                                foundListItem.State = item.State;

                                break;
                            }
                        case SiganalREventName.ListItemAdded:
                            {
                                var addSignalREvent = signaREvent as ListItemAddedSignalREvent;

                                var tempList = App.User.ListAggregators.Where(a => a.ListAggregatorId == listAggregationId).FirstOrDefault().
                         Lists.Where(a => a.ListId == addSignalREvent.ListId).FirstOrDefault().ListItems;

                                if (!tempList.Where(a => a.Id == item.Id).Any())
                                {
                                    tempList.Insert(0, item);
                                }
                                MessagingCenter.Send<ListAggregationViewModel, User>(vm, "New Data", App.User);
                                break;
                            }
                        case SiganalREventName.ListItemDeleted:
                            {

                                var lists = App.User.ListAggregators.Where(a => a.ListAggregatorId == listAggregationId).FirstOrDefault();

                                ListItem foundListItem = null;
                                List founfList = null;

                                foreach (var listItem in lists.Lists)
                                {
                                    founfList = listItem;
                                    foundListItem = listItem.ListItems.FirstOrDefault(a => a.Id == listItemId);
                                    if (foundListItem != null) break;
                                }

                                founfList.ListItems.Remove(foundListItem);

                                MessagingCenter.Send<ListAggregationViewModel, User>(vm, "New Data", App.User);

                                break;
                            }
                        default:
                            break;
                    }


                    ListItemSignalREvent GetDeserializedSinglaREvent(string signaREventSerialized)
                    {
                        return eventName switch
                        {
                            SiganalREventName.ListItemAdded => JsonSerializer.Deserialize<ListItemAddedSignalREvent>(signaREventSerialized),
                            SiganalREventName.ListItemEdited => JsonSerializer.Deserialize<ListItemEditedSignalREvent>(signaREventSerialized),
                            SiganalREventName.ListItemDeleted => JsonSerializer.Deserialize<ListItemDeletedSignalREvent>(signaREventSerialized),
                            _ => throw new ArgumentException("Unknown signaREvent")
                        };
                    }
                });

            var newInvitationDispose = hubConnection.On("InvitationAreChanged_" + App.User.UserId, async () =>
            {

                SetInvitationString("NEW");
            });



            List<IDisposable> disposables = new List<IDisposable>(new[]
            { newInvitationDispose, listItemArechangeDispose, dataAreChangeDispose });


            await hubConnection.StartAsync();

            return (disposables, hubConnection);

        }
    }
}
