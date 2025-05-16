using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XamarinShoppingList1.Models;
using XamarinShoppingList1.Models.Requests;
using XamarinShoppingList1.Models.Response;

namespace XamarinShoppingList1.Services
{
    public  class UserService
    {


        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UserService(HttpClient httpClient, IConfiguration configuration)
        {
            //_httpClient = httpClient;

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

          
             _httpClient = new HttpClient(handler);

            var baseAddress = configuration.GetSection("AppSettings")["ShoppingWebAPIBaseAddress"];
            _httpClient.BaseAddress = new Uri(baseAddress);

            //_httpClient.BaseAddress = new Uri("https://192.168.8.222:5003/api/");
           // _httpClient.BaseAddress = new Uri("https://94.251.148.187:5003/api/");

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BlazorServer");
            _configuration = configuration;
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

        public async Task<MessageAndStatusAndData<TokenAndEmailData>> GetTokenFromFacebookAccessToken(string accessFacebookToken)
        {
            var querry = new QueryBuilder();
            querry.Add("access_token", accessFacebookToken);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "User/FacebookToken" + querry.ToString());

            MessageAndStatusAndData<TokenAndEmailData> message = null;
            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                var data = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    var tokenData = JsonConvert.DeserializeObject<TokenAndEmailData>(data);
                    return MessageAndStatusAndData<TokenAndEmailData>.Ok(tokenData);
                }
                message = MessageAndStatusAndData<TokenAndEmailData>.Fail(
                    JsonConvert.DeserializeObject<ProblemDetails >(data).Title);
            }
            catch
            {
                message = MessageAndStatusAndData<TokenAndEmailData>.Fail( "Connection problem.");
            }
            return message;
        }

        public async Task<MessageAndStatusAndData<UserNameAndTokenResponse>> LoginAsync(string userName, string password)
        {

            var loginRequest = new LoginRequest
            {
                UserName = userName,
                Password = password
            };

            var json = JsonConvert.SerializeObject(loginRequest);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "User/Login")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            try
            {
                var source = new CancellationTokenSource();

                _ = Task.Run(async () =>
                {
                    await Task.Delay(10000);
                    source.Cancel();
                });


                var response = await _httpClient.SendAsync(requestMessage, source.Token);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return MessageAndStatusAndData<UserNameAndTokenResponse>.Fail("Invalid username or password.");
                }
                if (!response.IsSuccessStatusCode)
                {
                    return MessageAndStatusAndData<UserNameAndTokenResponse>.Fail("Some errors occured.");
                }

                var data = await response.Content.ReadAsStringAsync();

                var tokenAndUsername = JsonConvert.DeserializeObject<UserNameAndTokenResponse>(data);
                return MessageAndStatusAndData<UserNameAndTokenResponse>.Ok(tokenAndUsername);
            }
            catch
            {
                return MessageAndStatusAndData<UserNameAndTokenResponse>.Fail("Connection problem.");
            }
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

        public async Task<MessageAndStatusAndData<string>> RegisterAsync(RegistrationModel model)
        {

            var loginRequest = new RegistrationRequest
            {
                UserName = model.UserName,
                Password = model.Password
            };

            var json = JsonConvert.SerializeObject(loginRequest);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "User/Register")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };


            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();

                return MessageAndStatusAndData<string>.Ok(token);
            }

            return response switch
            {
                { StatusCode: System.Net.HttpStatusCode.Conflict } =>
                     MessageAndStatusAndData<string>.Fail("User exists."),
                _ =>
                    MessageAndStatusAndData<string>.Fail("Server error."),
            };

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

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "Permissions/GetListAggregationForPermission" + querry.ToString());


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

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "Permissions/" + actionName + querry.ToString());


            requestMessage.Content = new StringContent(serializedUser);

            requestMessage.Content.Headers.ContentType
              = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            await SetRequestBearerAuthorizationHeader(requestMessage);
            SetRequestAuthorizationLevelHeader(requestMessage, listAggregationId);

            var response = await _httpClient.SendAsync(requestMessage);

            var responseStatusCode = response.StatusCode;

            var responseBody = await response.Content.ReadAsStringAsync();

            var message = JsonConvert.DeserializeObject<MessageAndStatus>(responseBody);

            return await Task.FromResult(message);
        }

        void SetRequestAuthorizationLevelHeader(HttpRequestMessage httpRequestMessage, int listAggregationId)
        {
            var token = App.Token;

            if (token != null)
            {
                httpRequestMessage.Headers.Add("listAggregationId", listAggregationId.ToString());

                using (SHA256 mySHA256 = SHA256.Create())
                {

                    var bytes = Encoding.ASCII.GetBytes(token + listAggregationId.ToString());

                    var hashBytes = mySHA256.ComputeHash(bytes);

                    var hashString = Convert.ToBase64String(hashBytes);

                    httpRequestMessage.Headers.Add("Hash", hashString);
                }
            }


        }
    }
}
