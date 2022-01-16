using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using XamarinShoppingList1.Droid;
using XamarinShoppingList1.Services;

[assembly: Dependency(typeof(ClearCookies))]  
namespace XamarinShoppingList1.Droid
{
    public class ClearCookies : IClearCookies
    {
        public void ClearAllCookies()
        {
            var cookieManager = CookieManager.Instance;
         
            cookieManager.RemoveAllCookie();
          
        }
    }    
}