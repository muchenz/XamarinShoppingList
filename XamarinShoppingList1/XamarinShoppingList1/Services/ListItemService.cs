using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using XamarinShoppingList1.Helpers;

namespace XamarinShoppingList1.Services
{
    public class ListItemService
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ListItemService(HttpClient httpClient, IConfiguration configuration)
        {
            //_httpClient = httpClient;

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;


            _httpClient = new HttpClient(handler);

            var baseAddress = configuration.GetSection("AppSettings")["ShoppingWebAPIBaseAddress"];
            _httpClient.BaseAddress = new Uri(baseAddress);
            
            // _httpClient.BaseAddress = new Uri("https://192.168.8.222:5003/api/");
            // _httpClient.BaseAddress = new Uri("https://94.251.148.92:5003/api/");

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

        public async Task<T> SaveItemProperty<T>(T item, string propertyName, int listAggregationId)
        {
            var querry = new QueryBuilder();
            querry.Add("listAggregationId", listAggregationId.ToString());
            querry.Add("propertyName", propertyName);

            string serializedUser = JsonConvert.SerializeObject(item);

            string typeName = typeof(T).ToString().Split('.').LastOrDefault();

            // var requestMessage = new HttpRequestMessage(HttpMethod.Post, "ListItem/AddItemListItemToList"+querry.ToString());
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, typeName + "/SaveProperty" + querry);


            requestMessage.Content = new StringContent(serializedUser);

            requestMessage.Content.Headers.ContentType
              = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            await SetRequestBearerAuthorizationHeader(requestMessage);
            SetRequestAuthorizationLevelHeader(requestMessage, listAggregationId);


            var response = await _httpClient.SendAsync(requestMessage);

            var responseStatusCode = response.StatusCode;

            var responseBody = await response.Content.ReadAsStringAsync();

            var returnedUser = JsonConvert.DeserializeObject<T>(responseBody);


            return await Task.FromResult(returnedUser);
        }


        public async Task<T> GetItem<T>(int itemId, int listAggregationId)
        {
            var querry = new QueryBuilder();
            querry.Add("listAggregationId", listAggregationId.ToString());
            querry.Add("itemId", itemId.ToString());

            //string serializedUser = JsonConvert.SerializeObject(item);

            string typeName = typeof(T).ToString().Split('.').LastOrDefault();

            // var requestMessage = new HttpRequestMessage(HttpMethod.Post, "ListItem/AddItemListItemToList"+querry.ToString());
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, typeName + "/GetItem" + typeName + querry);


            requestMessage.Content = new StringContent("");

            //requestMessage.Content.Headers.ContentType
            //  = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            await SetRequestBearerAuthorizationHeader(requestMessage);
            SetRequestAuthorizationLevelHeader(requestMessage, listAggregationId);


            var response = await _httpClient.SendAsync(requestMessage);

            var responseStatusCode = response.StatusCode;

            var responseBody = await response.Content.ReadAsStringAsync();

            var returnedItem = JsonConvert.DeserializeObject<T>(responseBody);


            return await Task.FromResult(returnedItem);
        }

        public async Task<T> AddItem<T>(int parentId, T item, int listAggregationId)
        {           

            var querry = new QueryBuilder();
            querry.Add("parentId", parentId.ToString());
            querry.Add("listAggregationId", listAggregationId.ToString());
            // querry.Add("password", password);

            string serializedUser = JsonConvert.SerializeObject(item);

            string typeName = typeof(T).ToString().Split('.').LastOrDefault();

            // var requestMessage = new HttpRequestMessage(HttpMethod.Post, "ListItem/AddItemListItemToList"+querry.ToString());
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, typeName + "/Add" + typeName + querry.ToString());


            requestMessage.Content = new StringContent(serializedUser);

            requestMessage.Content.Headers.ContentType
              = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            await SetRequestBearerAuthorizationHeader(requestMessage);
            SetRequestAuthorizationLevelHeader(requestMessage, listAggregationId);

            var response = await _httpClient.SendAsync(requestMessage);

            var responseStatusCode = response.StatusCode;

            var responseBody = await response.Content.ReadAsStringAsync();

            var returnedUser = JsonConvert.DeserializeObject<T>(responseBody);


            return await Task.FromResult(returnedUser);
        }

        public async Task<T> EditItem<T>(T item, int listAggregationId)
        {
          
            var querry = new QueryBuilder();
            querry.Add("listAggregationId", listAggregationId.ToString());
            querry.Add("lvl", 2.ToString());

            string serializedUser = JsonConvert.SerializeObject(item);

            string typeName = typeof(T).ToString().Split('.').LastOrDefault();

            // var requestMessage = new HttpRequestMessage(HttpMethod.Post, "ListItem/AddItemListItemToList"+querry.ToString());
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, typeName + "/Edit" + typeName + querry.ToString());


            requestMessage.Content = new StringContent(serializedUser);

            requestMessage.Content.Headers.ContentType
              = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            await SetRequestBearerAuthorizationHeader(requestMessage);
            SetRequestAuthorizationLevelHeader(requestMessage, listAggregationId);


            var response = await _httpClient.SendAsync(requestMessage);

            var responseStatusCode = response.StatusCode;

            if (responseStatusCode == HttpStatusCode.Forbidden)
                throw new WebPermissionException(HttpStatusCode.Forbidden.ToString());

            var responseBody = await response.Content.ReadAsStringAsync();

            var returnedUser = JsonConvert.DeserializeObject<T>(responseBody);


            return await Task.FromResult(returnedUser);
        }

        public async Task<int> Delete<T>(int ItemId, int listAggregationId) where T : new()
        {

            var querry = new QueryBuilder();
            querry.Add("ItemId", ItemId.ToString());
            querry.Add("listAggregationId", listAggregationId.ToString());
            // querry.Add("password", password);

            string typeName = typeof(T).ToString().Split('.').LastOrDefault();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, typeName + "/Delete" + typeName + querry.ToString());

            //var requestMessage = new HttpRequestMessage(HttpMethod.Post, "ListItem/DeleteListItem" + querry.ToString());                                  


            await SetRequestBearerAuthorizationHeader(requestMessage);
            SetRequestAuthorizationLevelHeader(requestMessage, listAggregationId);


            var response = await _httpClient.SendAsync(requestMessage);

            var responseStatusCode = response.StatusCode;
            if (responseStatusCode == HttpStatusCode.Forbidden)
                throw new WebPermissionException(HttpStatusCode.Forbidden.ToString());

            var responseBody = await response.Content.ReadAsStringAsync();

            var returnedUser = JsonConvert.DeserializeObject<int>(responseBody);

            return await Task.FromResult(returnedUser);
        }
    }
}
