using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using XamarinShoppingList1.Models;

namespace XamarinShoppingList1.Helpers
{
    public class LoadSaveOrderDataHelper
    {
        /// <summary>
        /// ////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        public static void LoadListAggregatorsOrder()
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

        static void ResolveDoubleOrderValue(IEnumerable<IModelItemOrder> list)
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

        static void SetEntryOrder(IEnumerable<IModelItemOrder> list)
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

        public static void SaveAllOrder(ICollection<ListAggregator> list)
        {
            var tempAggrList = new List<OrderListAggrItem>();

            int i = 1, j = 1, k = 1;
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




            if (Application.Current.Properties.ContainsKey("UserName"))
            {
                var UserName = Application.Current.Properties["UserName"].ToString();

                string serializedObject = JsonConvert.SerializeObject(tempAggrList);

                Application.Current.Properties[$"{UserName}ListOrder"] = serializedObject;

            }
        }
    }
}
