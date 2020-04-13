using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace XamarinShoppingList1.Converters
{
    public class PermissionToColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          

            if (value == null) return "Gray";

            if (System.Convert.ToInt32(value) == 1) return "Green"; 
            if (System.Convert.ToInt32(value) == 2) return "Blue";
            if (System.Convert.ToInt32(value) == 3) return "Gray";

            return "Gray";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //For this case you can ignore this
            return null;
        }
    }
}
