using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XamarinShoppingList1.Models;

namespace XamarinShoppingList1.Services
{
    public  class UserService
    {


        private readonly HttpClient _httpClient;
     
      
        public UserService(HttpClient httpClient)
        {
            //_httpClient = httpClient;

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

          
             _httpClient = new HttpClient(handler);
            

            _httpClient.BaseAddress = new Uri("https://192.168.8.222:5003/api/");
            
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BlazorServer");
           
        }


        private async Task SetRequestBearerAuthorizationHeader(HttpRequestMessage httpRequestMessage)
        {

            var token = App.Token;

            if (token != null)
            {

                httpRequestMessage.Headers.Authorization
                    = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
                // _httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer  {token}");
            }
        }


        public async Task<string> LoginAsync(string userName, string password)
        {

            var querry = new QueryBuilder();
            querry.Add("userName", userName);
            querry.Add("password", password);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "User/Login" + querry.ToString());

            // await SetRequestBearerAuthorizationHeader(requestMessage);

            requestMessage.Content = new StringContent("");

            requestMessage.Content.Headers.ContentType
                = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            var response = await _httpClient.SendAsync(requestMessage);

            var data = await response.Content.ReadAsStringAsync();

            var message = JsonConvert.DeserializeObject<MessageAndStatus>(data);


            if (message!=null && message.Status=="OK")
                App.Token = message.Message;

            return await Task.FromResult(message.Message);

        }

        public async Task<string> GetUserDataTreeStringAsync(string userName)
        {

            var querry = new QueryBuilder();
            querry.Add("userName", userName);
            // querry.Add("password", password);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "User/GetUserDataTree" + querry.ToString());


            await SetRequestBearerAuthorizationHeader(requestMessage);


            var response = await _httpClient.SendAsync(requestMessage);

            var data = await response.Content.ReadAsStringAsync();

            var message = JsonConvert.DeserializeObject<MessageAndStatus>(data);


            return await Task.FromResult(message.Message);
        }

        public async Task<User> GetUserDataTreeObjectsgAsync(string userName)
        {

            var dataString = await GetUserDataTreeStringAsync(userName);


            var dataObjects = JsonConvert.DeserializeObject<User>(dataString);

            return dataObjects;
        }

        public async Task<string> RegisterAsync(RegistrationModel model)
        {

            var querry = new QueryBuilder();
            querry.Add("userName", model.UserName);
            querry.Add("password", model.Password);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "User/Register" + querry.ToString());

            // await SetRequestBearerAuthorizationHeader(requestMessage);

            requestMessage.Content = new StringContent("");

            requestMessage.Content.Headers.ContentType
                = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            var response = await _httpClient.SendAsync(requestMessage);

            var token = await response.Content.ReadAsStringAsync();

            var message = JsonConvert.DeserializeObject<MessageAndStatus>(token);


            return await Task.FromResult(message.Message);

        }

        public async Task<List<Invitation>> GetInvitationsListAsync(string userName)
        {
            var querry = new QueryBuilder();
            querry.Add("userName", userName);
            // querry.Add("password", password);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "Invitation/GetInvitationsList" + querry.ToString());


            await SetRequestBearerAuthorizationHeader(requestMessage);


            var response = await _httpClient.SendAsync(requestMessage);

            var data = await response.Content.ReadAsStringAsync();

            var message = JsonConvert.DeserializeObject<MessageAndStatus>(data);

            var dataObjects = JsonConvert.DeserializeObject<List<Invitation>>(message.Message);


            return await Task.FromResult(dataObjects);
        }

        public async Task<string> AcceptInvitationAsync(Invitation invitation)
        {
            return await UniversalInvitationAction(invitation, "AcceptInvitation");

        }
        public async Task<string> RejectInvitaionAsync(Invitation invitation)
        {

            return await UniversalInvitationAction(invitation, "RejectInvitaion");

        }

        async Task<string> UniversalInvitationAction(Invitation invitation, string actionName)
        {
            string serialized = JsonConvert.SerializeObject(invitation);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "Invitation/" + actionName);


            requestMessage.Content = new StringContent(serialized);

            requestMessage.Content.Headers.ContentType
              = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            await SetRequestBearerAuthorizationHeader(requestMessage);

            var response = await _httpClient.SendAsync(requestMessage);

            var responseStatusCode = response.StatusCode;

            var responseBody = await response.Content.ReadAsStringAsync();

            var message = JsonConvert.DeserializeObject<MessageAndStatus>(responseBody);


            return await Task.FromResult(message.Message);
        }
        public async Task<List<ListAggregationForPermission>> GetListAggregationForPermissionAsync(string userName)
        {

            var querry = new QueryBuilder();
            querry.Add("userName", userName);
            // querry.Add("password", password);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "User/GetListAggregationForPermission" + querry.ToString());


            await SetRequestBearerAuthorizationHeader(requestMessage);


            var response = await _httpClient.SendAsync(requestMessage);

            var data = await response.Content.ReadAsStringAsync();

            var message = JsonConvert.DeserializeObject<MessageAndStatus>(data);


            //  var dataObjects = JsonConvert.DeserializeObject<List<ListAggregationForPermissionTransferClass>>(data);
            var dataObjects = JsonConvert.DeserializeObject<List<ListAggregationForPermission>>(message.Message);


            return await Task.FromResult(dataObjects);
        }

        public async Task<MessageAndStatus> AddUserPermission(UserPermissionToListAggregation userPermissionToList, int listAggregationId)
        {
            return await UniversalUserPermission(userPermissionToList, listAggregationId, "AddUserPermission");
        }

        public async Task<MessageAndStatus> ChangeUserPermission(UserPermissionToListAggregation userPermissionToList, int listAggregationId)
        {

            return await UniversalUserPermission(userPermissionToList, listAggregationId, "ChangeUserPermission");

        }


        public async Task<MessageAndStatus> DeleteUserPermission(UserPermissionToListAggregation userPermissionToList, int listAggregationId)
        {
            return await UniversalUserPermission(userPermissionToList, listAggregationId, "DeleteUserPermission");
        }

        public async Task<MessageAndStatus> InviteUserPermission(UserPermissionToListAggregation userPermissionToList, int listAggregationId)
        {
            return await UniversalUserPermission(userPermissionToList, listAggregationId, "InviteUserPermission");
        }

        private async Task<MessageAndStatus> UniversalUserPermission(UserPermissionToListAggregation userPermissionToList, int listAggregationId,
            string actionName)
        {
            var querry = new QueryBuilder();

            querry.Add("listAggregationId", listAggregationId.ToString());
            // querry.Add("password", password);

            string serializedUser = JsonConvert.SerializeObject(userPermissionToList);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "User/" + actionName + querry.ToString());


            requestMessage.Content = new StringContent(serializedUser);

            requestMessage.Content.Headers.ContentType
              = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            await SetRequestBearerAuthorizationHeader(requestMessage);

            var response = await _httpClient.SendAsync(requestMessage);

            var responseStatusCode = response.StatusCode;

            var responseBody = await response.Content.ReadAsStringAsync();

            var message = JsonConvert.DeserializeObject<MessageAndStatus>(responseBody);

            return await Task.FromResult(message);
        }


    }
}
