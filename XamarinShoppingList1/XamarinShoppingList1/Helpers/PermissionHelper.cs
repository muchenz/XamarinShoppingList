using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XamarinShoppingList1.Models;

namespace XamarinShoppingList1.Helpers
{
    public class PermissionHelper
    {
       public static IEnumerable<PermissionsToListAggr> GetPerrmissionsFromToke(string token)
        {

            var tokenLoad = token.Split('.')[1];

            var bytes = Convert.FromBase64String(tokenLoad);

            var toDeserialize = Encoding.Default.GetString(bytes);

            // var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(toDeserialize);

            if (!toDeserialize.Contains("\"ListAggregator\":[")) return new List<PermissionsToListAggr>();

            string[] array = new string[] { "\"ListAggregator\":[" };

            var next = toDeserialize.Split(array, StringSplitOptions.RemoveEmptyEntries).Last().Split(']').First().Split(',').Select(a =>
            {

                var b = a.Substring(1, a.Length - 2).Split('.');

                int id = int.Parse(b.First());
                int perm = int.Parse(b.Last());

                return new PermissionsToListAggr { Id = id, Permission = perm };

            });

            return next;
        }



        public static bool IsPermissionToOperationFromToken(string token, int idListAggr, int operation)
        {
            var listOfPermission = GetPerrmissionsFromToke(token);                   


            var perm = listOfPermission.Where(a => a.Id == idListAggr).FirstOrDefault();

            if (perm == null) return false;

            if (perm.Permission <= operation)
                return true;
            else
                return false;

        }
        public static bool IsPermissionToOperationFromPermissionList(IEnumerable<PermissionsToListAggr> listOfPermission, int idListAggr, int operation)
        {           

            var perm = listOfPermission.Where(a => a.Id == idListAggr).FirstOrDefault();

            if (perm == null) return false;

            if (perm.Permission <= operation)
                return true;
            else
                return false;

        }

    }
}
